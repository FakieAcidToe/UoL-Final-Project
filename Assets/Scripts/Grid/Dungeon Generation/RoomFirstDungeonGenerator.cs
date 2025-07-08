using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
	public enum RoomTypes
	{
		RandomWalk,
		Rectangle,
		Circle
	}

	[SerializeField] DungeonParamsSO dungeonParams;

	List<BoundsInt> roomsList;

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

		base.OnDrawGizmosSelected();
	}

	protected override void RunProceduralGeneration()
	{
		CreateRooms();
	}

	void CreateRooms()
	{
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

		HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

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
				floor = ProceduralGenerationAlgorithms.GenerateNoise(floor, room, dungeonParams.noiseChance);

		// add room tiles and border
		floor.UnionWith(rooms);
		if (dungeonParams.border > 0)
			floor = ProceduralGenerationAlgorithms.RemoveBorder(floor, roomsList, dungeonParams.border);

		// apply cellular automata
		for (int iter = 0; iter < dungeonParams.cellularAutomataIterations; ++iter)
		{
			floor = ProceduralGenerationAlgorithms.ApplyCellularAutomata(floor);

			// dont let cellular automata override rooms and border
			if (dungeonParams.cellularAutomataDontOverrideRooms)
				floor.UnionWith(rooms);
			if (dungeonParams.border > 0)
				floor = ProceduralGenerationAlgorithms.RemoveBorder(floor, roomsList, dungeonParams.border);
		}

		// generate corridors from room centers
		if (dungeonParams.corridorWidth > 0)
			ProceduralGenerationAlgorithms.GenerateCorridorsMST(roomsList, floor, dungeonParams.corridorWidth, dungeonParams.corridorExtraLoopChance);

		spawnPosition = (Vector2Int)Vector3Int.FloorToInt(roomsList[Random.Range(0, roomsList.Count)].center);

		// flood fill
		if (dungeonParams.applyFloodFill)
			floor = ProceduralGenerationAlgorithms.FloodFill(floor, spawnPosition);

		exitPosition = ProceduralGenerationAlgorithms.FindFurthestExit(floor, spawnPosition);

		TileGenerator.GenerateTiles(floor, tilemapVisualizer);
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