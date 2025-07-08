using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ProceduralGenerationAlgorithms
{
	public static HashSet<Vector2Int> EmptyRectRoom(Vector2Int position, Vector2Int dungeonSize)
	{
		HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
		for (int i = 0; i < dungeonSize.x; ++i)
			for (int j = 0; j < dungeonSize.y; ++j)
				floorPositions.Add(new Vector2Int(position.x + i - dungeonSize.x / 2, position.y + j - dungeonSize.y / 2));
		return floorPositions;
	}

	public static HashSet<Vector2Int> EmptyCircleRoom(Vector2Int position, Vector2Int dungeonSize)
	{
		float a = dungeonSize.x / 2f;
		float b = dungeonSize.y / 2f;

		HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
		for (int i = 0; i < dungeonSize.x; ++i)
			for (int j = 0; j < dungeonSize.y; ++j)
			{
				float px = i + 0.5f - a;
				float py = j + 0.5f - b;
				if ((px * px) / (a * a) + (py * py) / (b * b) <= 1f)
					floorPositions.Add(new Vector2Int(position.x + i - dungeonSize.x / 2, position.y + j - dungeonSize.y / 2));
			}
		return floorPositions;
	}

	public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
	{
		HashSet<Vector2Int> path = new HashSet<Vector2Int>();

		path.Add(startPosition);
		Vector2Int previousPosition = startPosition;

		for (int i = 0; i < walkLength; ++i)
		{
			Vector2Int newPosition = previousPosition + Direction2D.GetRandomCardinalDirection();
			path.Add(newPosition);
			previousPosition = newPosition;
		}
		return path;
	}

	public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
	{
		List<Vector2Int> corridor = new List<Vector2Int>();
		Vector2Int direction = Direction2D.GetRandomCardinalDirection();
		Vector2Int currentPosition = startPosition;
		corridor.Add(currentPosition);

		for (int i = 0; i < corridorLength; ++i)
		{
			currentPosition += direction;
			corridor.Add(currentPosition);
		}
		return corridor;
	}

	public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
	{
		Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
		List<BoundsInt> roomsList = new List<BoundsInt>();
		roomsQueue.Enqueue(spaceToSplit);
		while (roomsQueue.Count > 0)
		{
			BoundsInt room = roomsQueue.Dequeue();
			if (room.size.y >= minHeight && room.size.x >= minWidth)
			{
				if (Random.value < 0.5f)
				{
					if (room.size.y >= minHeight * 2)
						SplitHorizontally(minHeight, roomsQueue, room);
					else if (room.size.x >= minWidth * 2)
						SplitVertically(minWidth, roomsQueue, room);
					else
						roomsList.Add(room);
				}
				else
				{
					if (room.size.x >= minWidth * 2)
						SplitVertically(minWidth, roomsQueue, room);
					else if (room.size.y >= minHeight * 2)
						SplitHorizontally(minHeight, roomsQueue, room);
					else
						roomsList.Add(room);
				}
			}
		}
		return roomsList;
	}

	static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
	{
		int xSplit = Random.Range(1, room.size.x);
		BoundsInt room1 = new BoundsInt(room.min,
			new Vector3Int(xSplit, room.size.y, room.size.z));
		BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
			new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
		roomsQueue.Enqueue(room1);
		roomsQueue.Enqueue(room2);
	}

	static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
	{
		int ySplit = Random.Range(1, room.size.y);
		BoundsInt room1 = new BoundsInt(room.min,
			new Vector3Int(room.size.x, ySplit, room.size.z));
		BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
			new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
		roomsQueue.Enqueue(room1);
		roomsQueue.Enqueue(room2);
	}

	public static HashSet<Vector2Int> ApplyCellularAutomata(HashSet<Vector2Int> floorPositions, int cellularIterations = 1)
	{
		RectInt mapBounds = GetBounds(floorPositions);

		HashSet<Vector2Int> newFloorPositions = new HashSet<Vector2Int>(floorPositions);
		for (int iter = 0; iter < cellularIterations; ++iter)
		{
			HashSet<Vector2Int> currGrid = new HashSet<Vector2Int>();
			for (int y = mapBounds.yMin; y < mapBounds.yMax; ++y) // loop through all tiles
				for (int x = mapBounds.xMin; x < mapBounds.xMax; ++x)
				{
					int neighbourWallCount = 0; // check neighbouring tiles
					for (int y2 = y - 1; y2 <= y + 1; ++y2)
						for (int x2 = x - 1; x2 <= x + 1; ++x2)
							if (!newFloorPositions.Contains(new Vector2Int(x2, y2)))
								++neighbourWallCount;
					if (neighbourWallCount <= 4) // floor if <= 4 
						currGrid.Add(new Vector2Int(x, y));
				}
			newFloorPositions = currGrid;
		}

		return newFloorPositions;
	}

	public static HashSet<Vector2Int> GenerateNoise(HashSet<Vector2Int> floorPositions, BoundsInt size, float noiseChance = 0.5f)
	{
		HashSet<Vector2Int> newFloor = new HashSet<Vector2Int>(floorPositions);
		newFloor.UnionWith(GenerateNoise(size, noiseChance));
		return newFloor;
	}

	public static HashSet<Vector2Int> GenerateNoise(BoundsInt size, float noiseChance = 0.5f)
	{
		HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
		for (int i = 0; i < size.size.x; ++i)
			for (int j = 0; j < size.size.y; ++j)
				if (Random.Range(0f, 1f) < noiseChance)
					floorPositions.Add(new Vector2Int(size.x + i, size.y + j));
		return floorPositions;
	}

	public static HashSet<Vector2Int> RemoveBorder(HashSet<Vector2Int> floorPositions, List<BoundsInt> roomsList, int borderWidth = 1)
	{
		HashSet<Vector2Int> newFloor = new HashSet<Vector2Int>(floorPositions);
		foreach (BoundsInt room in roomsList)
			newFloor = RemoveBorder(newFloor, room, borderWidth);
		return newFloor;
	}

	public static HashSet<Vector2Int> RemoveBorder(HashSet<Vector2Int> floorPositions, BoundsInt room, int borderWidth = 1)
	{
		HashSet<Vector2Int> newFloor = new HashSet<Vector2Int>(floorPositions);
		for (int i = 0; i < room.size.x; ++i)
			for (int j = 0; j < room.size.y; ++j)
				if (i < borderWidth || i >= room.size.x-borderWidth || j < borderWidth || j >= room.size.y - borderWidth)
					newFloor.Remove(new Vector2Int(room.x + i, room.y + j));
		return newFloor;
	}

	static RectInt GetBounds(HashSet<Vector2Int> points)
	{
		if (points == null || points.Count == 0)
			return new RectInt(); // Returns RectInt(0,0,0,0)

		int minX = int.MaxValue;
		int maxX = int.MinValue;
		int minY = int.MaxValue;
		int maxY = int.MinValue;

		foreach (var point in points)
		{
			if (point.x < minX) minX = point.x;
			if (point.x > maxX) maxX = point.x;
			if (point.y < minY) minY = point.y;
			if (point.y > maxY) maxY = point.y;
		}

		return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
	}
}