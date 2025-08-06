using System.Collections.Generic;
using UnityEngine;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
	public enum RoomTypes
	{
		RandomWalk,
		Rectangle,
		Circle
	}

	public DungeonParamsSO dungeonParams;

	protected Vector2Int treasurePosition = Vector2Int.left;

	public List<BoundsInt> roomsList { private set; get; }

	protected override void OnDrawGizmosSelected()
	{
		// dungeon bounds
		Gizmos.color = Color.blue;
		Vector2 dungeonBounds = new Vector2(dungeonParams.dungeonWidth, dungeonParams.dungeonHeight);
		Gizmos.DrawWireCube((Vector2)startPosition + dungeonBounds / 2, dungeonBounds);

		if (roomsList != null)
			foreach (BoundsInt room in roomsList)
			{
				// actual room
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireCube(room.center, room.size - (Vector3Int)(Vector2Int.one * dungeonParams.offset * 2));

				// room border
				if (dungeonParams.border > 0)
				{
					Gizmos.color = Color.magenta;
					Gizmos.DrawWireCube(room.center, room.size - (Vector3Int)(Vector2Int.one * dungeonParams.border * 2));
				}

				// room bounds
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireCube(room.center, room.size);
			}


		// treasure point
		if (treasurePosition != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(GetTreasureLocation(), 1);
		}

		base.OnDrawGizmosSelected();
	}

	public Vector2 GetTreasureLocation()
	{
		return treasurePosition + (Vector2)tilemapVisualizer.GetTilemapAnchor();
	}

	protected override void RunProceduralGeneration()
	{
		CreateRooms();
	}

	void CreateRooms()
	{
		// ensure min num of rooms met
		do
		{
			// partition
			roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
				new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonParams.dungeonWidth, dungeonParams.dungeonHeight, 0)),
				dungeonParams.minRoomWidth,
				dungeonParams.minRoomHeight
			);
		} while (roomsList == null || roomsList.Count < dungeonParams.minNumOfRooms);

		roomsList = ConvertRoomsTo1x1(roomsList, dungeonParams.percentageOf1x1Rooms);

		// generate room data
		HashSet<Vector2Int> rooms = new HashSet<Vector2Int>();
		switch (dungeonParams.roomType)
		{
			case RoomTypes.RandomWalk:
				rooms = CreateRoomsRandomly(roomsList);
				break;
			default:
				rooms = CreateSimpleRooms(roomsList);
				break;
			case RoomTypes.Circle:
				rooms = CreateCircleRooms(roomsList);
				break;
		}

		// generate noise in room area
		if (dungeonParams.noiseChance > 0f)
			foreach (BoundsInt room in roomsList)
				ProceduralGenerationAlgorithms.GenerateNoise(floorPositions, room, dungeonParams.noiseChance);

		// add room tiles and border
		floorPositions.UnionWith(rooms);
		if (dungeonParams.border > 0)
			ProceduralGenerationAlgorithms.RemoveBorder(floorPositions, roomsList, dungeonParams.border);

		// apply cellular automata
		for (int iter = 0; iter < dungeonParams.cellularAutomataIterations; ++iter)
		{
			ProceduralGenerationAlgorithms.ApplyCellularAutomata(floorPositions);

			// dont let cellular automata override rooms and border
			if (dungeonParams.cellularAutomataDontOverrideRooms)
				floorPositions.UnionWith(rooms);
			if (dungeonParams.border > 0)
				ProceduralGenerationAlgorithms.RemoveBorder(floorPositions, roomsList, dungeonParams.border);
		}

		// generate corridors from room centers
		if (dungeonParams.corridorWidth > 0)
			ProceduralGenerationAlgorithms.GenerateCorridorsMST(roomsList, floorPositions, dungeonParams.corridorWidth, dungeonParams.corridorExtraLoopChance);

		spawnPosition = (Vector2Int)Vector3Int.FloorToInt(roomsList[Random.Range(0, roomsList.Count)].center);

		// flood fill
		if (dungeonParams.applyFloodFill)
			ProceduralGenerationAlgorithms.FloodFill(floorPositions, spawnPosition);

		exitPosition = ProceduralGenerationAlgorithms.FindFurthestExit(floorPositions, spawnPosition);
		treasurePosition = ProceduralGenerationAlgorithms.FindFurthestExit(floorPositions, exitPosition);

		tilemapVisualizer.SetTilemapPalette(dungeonParams.tilemapPalette);
	}

	HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
	{
		HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
		for (int i = 0; i < roomsList.Count; ++i)
		{
			BoundsInt roomBounds = roomsList[i];
			Vector2Int roomCenter = new Vector2Int(Mathf.FloorToInt(roomBounds.center.x), Mathf.FloorToInt(roomBounds.center.y));
			HashSet<Vector2Int> roomFloor = RunRandomWalk(randomWalkParameters, roomCenter);
			foreach (Vector2Int position in roomFloor)
			{
				if (position.x >= (roomBounds.xMin + dungeonParams.offset) && position.x <= (roomBounds.xMax - dungeonParams.offset) &&
					position.y >= (roomBounds.yMin - dungeonParams.offset) && position.y <= (roomBounds.yMax - dungeonParams.offset))
					floor.Add(position);
			}
		}
		return floor;
	}

	List<BoundsInt> ConvertRoomsTo1x1(List<BoundsInt> _roomsList, float _percentageOf1x1Rooms)
	{
		List<BoundsInt> roomsList = new List<BoundsInt>(_roomsList);

		// determine how many 1x1 rooms there will be
		bool[] isRoom1x1 = new bool[roomsList.Count];
		int trueCount = (int)(isRoom1x1.Length * _percentageOf1x1Rooms);
		for (int i = 0; i < isRoom1x1.Length; ++i) isRoom1x1[i] = i < trueCount;
		//Debug.Log(trueCount);

		for (int i = isRoom1x1.Length - 1; i > 0; --i) // shuffle the array
		{
			int j	= Random.Range(0, i + 1);
			bool temp = isRoom1x1[i];
			isRoom1x1[i] = isRoom1x1[j];
			isRoom1x1[j] = temp;
		}
		//Debug.Log(string.Join(", ", isRoom1x1));

		for (int i = 0; i < isRoom1x1.Length; ++i) if (isRoom1x1[i]) // convert rooms to 1x1
			roomsList[i] = new BoundsInt(Vector3Int.FloorToInt(roomsList[i].center), new Vector3Int(dungeonParams.offset * 2 + 1, dungeonParams.offset * 2 + 1, 0));

		return roomsList;
	}

	HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
	{
		HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
		foreach (BoundsInt room in roomsList)
		{
			HashSet<Vector2Int> floorPositions = ProceduralGenerationAlgorithms.EmptyRectRoom((Vector2Int)Vector3Int.FloorToInt(room.center), (Vector2Int)room.size - Vector2Int.one * dungeonParams.offset * 2);
			floor.UnionWith(floorPositions);
		}
		return floor;
	}

	HashSet<Vector2Int> CreateCircleRooms(List<BoundsInt> roomsList)
	{
		HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
		foreach (BoundsInt room in roomsList)
		{
			HashSet<Vector2Int> floorPositions = ProceduralGenerationAlgorithms.EmptyCircleRoom((Vector2Int)Vector3Int.FloorToInt(room.center), (Vector2Int)room.size - Vector2Int.one * dungeonParams.offset * 2);
			floor.UnionWith(floorPositions);
		}
		return floor;
	}
}