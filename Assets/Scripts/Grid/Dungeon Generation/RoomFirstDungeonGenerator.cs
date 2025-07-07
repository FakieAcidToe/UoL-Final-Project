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

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		if (roomsList != null)
			foreach (BoundsInt room in roomsList)
				Gizmos.DrawWireCube(room.center, room.size);
	}

	protected override void RunProceduralGeneration()
	{
		CreateRooms();
	}

	void CreateRooms()
	{
		roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
			new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonParams.dungeonWidth, dungeonParams.dungeonHeight, 0)),
			dungeonParams.minRoomWidth,
			dungeonParams.minRoomHeight
		);

		roomsList = ConvertRoomsTo1x1(roomsList, dungeonParams.percentageOf1x1Rooms);

		// generate rooms
		HashSet <Vector2Int> floor = new HashSet<Vector2Int>();
		switch (dungeonParams.roomType)
		{
			case RoomTypes.RandomWalk:
				floor = CreateRoomsRandomly(roomsList);
				break;
			default:
				floor = CreateSimpleRooms(roomsList);
				break;
			case RoomTypes.Circle:
				floor = CreateCircleRooms(roomsList);
				break;
		}

		// apply cellular automata (should apply before corridors)
		floor = ProceduralGenerationAlgorithms.ApplyCellularAutomata(floor, dungeonParams.cellularAutomataIterations);

		if (dungeonParams.generateCorridors) // generate corridors from room centers
		{
			List<Vector2Int> roomCenters = new List<Vector2Int>();
			foreach (BoundsInt room in roomsList)
				roomCenters.Add((Vector2Int)Vector3Int.FloorToInt(room.center));

			HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
			floor.UnionWith(corridors);
		}

		TileGenerator.GenerateTiles(floor, tilemapVisualizer);

		spawnPosition = (Vector2Int)Vector3Int.FloorToInt(roomsList[Random.Range(0, roomsList.Count)].center);
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

	HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
	{
		HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
		Vector2Int currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
		roomCenters.Remove(currentRoomCenter);

		while (roomCenters.Count > 0)
		{
			Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
			roomCenters.Remove(closest);
			HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
			currentRoomCenter = closest;
			corridors.UnionWith(newCorridor);
		}
		return corridors;
	}

	HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
	{
		HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
		Vector2Int position = currentRoomCenter;
		corridor.Add(position);
		while (position.y != destination.y)
		{
			if (destination.y > position.y)
				position += Vector2Int.up;
			else if (destination.y < position.y)
				position += Vector2Int.down;
			corridor.Add(position);
		}
		while (position.x != destination.x)
		{
			if (destination.x > position.x)
				position += Vector2Int.right;
			else if (destination.x < position.x)
				position += Vector2Int.left;
			corridor.Add(position);
		}
		return corridor;
	}

	Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
	{
		Vector2Int closest = Vector2Int.zero;
		float distance = float.MaxValue;
		foreach (Vector2Int position in roomCenters)
		{
			float currentDistance = Vector2.Distance(position, currentRoomCenter);
			if (currentDistance < distance)
			{
				distance = currentDistance;
				closest = position;
			}
		}
		return closest;
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