using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ProceduralGenerationAlgorithms
{
	public static HashSet<Vector2Int> EmptyRectRoom(Vector2Int position, Vector2Int dungeonSize)
	{
		Vector2Int currentPosition = position; // currentPosition is the center of the room
		HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
		for (int i = 0; i < dungeonSize.x; ++i)
			for (int j = 0; j < dungeonSize.y; ++j)
				floorPositions.Add(new Vector2Int(currentPosition.x + i - dungeonSize.x / 2, currentPosition.y + j - dungeonSize.y / 2));
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
}