using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
	enum RoomTypes
	{
		RandomWalk,
		Rectangle,
		Circle
	}

	[SerializeField, Min(1)] int minRoomWidth = 4, minRoomHeight = 4;
	[SerializeField, Min(1)] int dungeonWidth = 20, dungeonHeight = 20;
	[SerializeField, Range(0, 10)] int offset = 1; // border size of each room
	[SerializeField] RoomTypes roomType = RoomTypes.RandomWalk; // what kind of rooms will be used?
	[SerializeField] bool generateCorridors = true;
	[SerializeField, Range(0, 1)] float percentageOf1x1Rooms = 0.1f; // some rooms remain 1x1 size
	[SerializeField, Min(0)] int cellularAutomataIterations = 0; // number of times to apply cellular automata loops

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
			new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
			minRoomWidth,
			minRoomHeight
		);

		roomsList = ConvertRoomsTo1x1(roomsList, percentageOf1x1Rooms);

		// generate rooms
		HashSet <Vector2Int> floor = new HashSet<Vector2Int>();
		switch (roomType)
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
		floor = ProceduralGenerationAlgorithms.ApplyCellularAutomata(floor, cellularAutomataIterations);

		if (generateCorridors) // generate corridors from room centers
		{
			List<Vector2Int> roomCenters = new List<Vector2Int>();
			foreach (BoundsInt room in roomsList)
				roomCenters.Add((Vector2Int)Vector3Int.FloorToInt(room.center));

			HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
			floor.UnionWith(corridors);
		}

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
				if (position.x >= (roomBounds.xMin + offset) && position.x <= (roomBounds.xMax - offset) &&
					position.y >= (roomBounds.yMin - offset) && position.y <= (roomBounds.yMax - offset))
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
			int j = Random.Range(0, i + 1);
			bool temp = isRoom1x1[i];
			isRoom1x1[i] = isRoom1x1[j];
			isRoom1x1[j] = temp;
		}
		//Debug.Log(string.Join(", ", isRoom1x1));

		for (int i = 0; i < isRoom1x1.Length; ++i) if (isRoom1x1[i]) // convert rooms to 1x1
			roomsList[i] = new BoundsInt(Vector3Int.FloorToInt(roomsList[i].center), new Vector3Int(offset * 2 + 1, offset * 2 + 1, 0));

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
			HashSet<Vector2Int> floorPositions = ProceduralGenerationAlgorithms.EmptyRectRoom((Vector2Int)Vector3Int.FloorToInt(room.center), (Vector2Int)room.size - Vector2Int.one * offset * 2);
			floor.UnionWith(floorPositions);
		}
		return floor;
	}

	HashSet<Vector2Int> CreateCircleRooms(List<BoundsInt> roomsList)
	{
		HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
		foreach (BoundsInt room in roomsList)
		{
			HashSet<Vector2Int> floorPositions = ProceduralGenerationAlgorithms.EmptyCircleRoom((Vector2Int)Vector3Int.FloorToInt(room.center), (Vector2Int)room.size - Vector2Int.one * offset * 2);
			floor.UnionWith(floorPositions);
		}
		return floor;
	}
}