using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;
using static UnityEditor.PlayerSettings;

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

	public static HashSet<Vector2Int> FloodFill(HashSet<Vector2Int> tiles, Vector2Int startPos)
	{
		HashSet<Vector2Int> connected = new HashSet<Vector2Int>();
		if (!tiles.Contains(startPos))
			return connected; // startPos not in tiles, return empty

		Queue<Vector2Int> queue = new Queue<Vector2Int>();
		queue.Enqueue(startPos);
		connected.Add(startPos);

		while (queue.Count > 0)
		{
			Vector2Int current = queue.Dequeue();

			foreach (Vector2Int dir in Direction2D.cardinalDirectionsList)
			{
				Vector2Int neighbor = current + dir;
				if (tiles.Contains(neighbor) && !connected.Contains(neighbor))
				{
					connected.Add(neighbor);
					queue.Enqueue(neighbor);
				}
			}
		}

		return connected;
	}

	public static void GenerateCorridorsMST(List<BoundsInt> rooms, HashSet<Vector2Int> tiles, int width = 3, float extraLoopChance = 0.15f)
	{
		// Get room centers
		List<Vector2Int> centers = rooms.Select(GetRoomCenter).ToList();

		// Create all edges (room pairs) with weights (distance)
		List<Edge> edges = new List<Edge>();
		for (int i = 0; i < centers.Count; ++i)
			for (int j = i + 1; j < centers.Count; ++j)
			{
				float dist = Vector2Int.Distance(centers[i], centers[j]);
				edges.Add(new Edge(i, j, dist));
			}

		// Sort edges by weight
		edges.Sort((a, b) => a.weight.CompareTo(b.weight));

		// Disjoint set for MST (Union-Find)
		DisjointSet ds = new DisjointSet(centers.Count);

		// Keep track of edges in MST
		List<Edge> mstEdges = new List<Edge>();

		foreach (Edge edge in edges)
			if (ds.Union(edge.roomA, edge.roomB))
			{
				mstEdges.Add(edge);
				// Connect rooms in MST edge
				CreateWideCorridor(centers[edge.roomA], centers[edge.roomB], tiles, width);
			}

		// Add extra loops for interesting paths
		if (extraLoopChance > 0)
			foreach (Edge edge in edges)
				if (!mstEdges.Contains(edge) && Random.value < extraLoopChance)
					CreateWideCorridor(centers[edge.roomA], centers[edge.roomB], tiles, width);
	}

	private static Vector2Int GetRoomCenter(BoundsInt room)
	{
		Vector3Int center = room.position + new Vector3Int(room.size.x / 2, room.size.y / 2, 0);
		return new Vector2Int(center.x, center.y);
	}

	private static void CreateWideCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> tiles, int width)
	{
		List<Vector2Int> line = GetLine(start, end);
		Vector2 direction = end - start;
		direction.Normalize();
		Vector2 perpDirection = new Vector2(-direction.y, direction.x);
		int halfWidth = width / 2;

		foreach (Vector2Int point in line)
		{
			for (int offset = -halfWidth; offset <= halfWidth; offset++)
			{
				Vector2 offsetPos = point + perpDirection * offset;
				Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(offsetPos.x), Mathf.FloorToInt(offsetPos.y));
				tiles.Add(tilePos);
				tiles.Add(tilePos + Vector2Int.right);
				tiles.Add(tilePos + Vector2Int.up);
				tiles.Add(tilePos + Vector2Int.one); // Cover neighbors to prevent gaps
			}
		}
	}

	private static List<Vector2Int> GetLine(Vector2Int start, Vector2Int end)
	{
		List<Vector2Int> line = new List<Vector2Int>();

		int x0 = start.x;
		int y0 = start.y;
		int x1 = end.x;
		int y1 = end.y;

		int dx = Mathf.Abs(x1 - x0);
		int dy = Mathf.Abs(y1 - y0);

		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;

		int err = dx - dy;

		while (true)
		{
			line.Add(new Vector2Int(x0, y0));
			if (x0 == x1 && y0 == y1) break;
			int e2 = 2 * err;
			if (e2 > -dy)
			{
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx)
			{
				err += dx;
				y0 += sy;
			}
		}

		return line;
	}

	// Helper classes for MST
	private class Edge
	{
		public int roomA, roomB;
		public float weight;
		public Edge(int a, int b, float w) { roomA = a; roomB = b; weight = w; }
	}

	private class DisjointSet
	{
		int[] parent;
		int[] rank;

		public DisjointSet(int n)
		{
			parent = new int[n];
			rank = new int[n];
			for (int i = 0; i < n; ++i) parent[i] = i;
		}

		public int Find(int x)
		{
			if (parent[x] != x) parent[x] = Find(parent[x]);
			return parent[x];
		}

		public bool Union(int x, int y)
		{
			int rootX = Find(x);
			int rootY = Find(y);
			if (rootX == rootY) return false;

			if (rank[rootX] < rank[rootY]) parent[rootX] = rootY;
			else if (rank[rootY] < rank[rootX]) parent[rootY] = rootX;
			else
			{
				parent[rootY] = rootX;
				++rank[rootX];
			}
			return true;
		}
	}

	public static Vector2Int FindFurthestExit(HashSet<Vector2Int> dungeonTiles, Vector2Int spawnPos)
	{
		if (!dungeonTiles.Contains(spawnPos))
		{
			Debug.LogWarning("Spawn position not in dungeon tiles");
			return spawnPos;
		}

		Queue<Vector2Int> queue = new Queue<Vector2Int>();
		Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

		queue.Enqueue(spawnPos);
		distances[spawnPos] = 0;

		Vector2Int furthestTile = spawnPos;
		int maxDistance = 0;

		while (queue.Count > 0)
		{
			Vector2Int current = queue.Dequeue();
			int currentDistance = distances[current];

			// Check if current tile is the furthest so far
			if (currentDistance > maxDistance)
			{
				maxDistance = currentDistance;
				furthestTile = current;
			}

			// Explore neighbors
			foreach (Vector2Int dir in Direction2D.cardinalDirectionsList)
			{
				Vector2Int neighbor = current + dir;
				if (dungeonTiles.Contains(neighbor) && !distances.ContainsKey(neighbor))
				{
					distances[neighbor] = currentDistance + 1;
					queue.Enqueue(neighbor);
				}
			}
		}

		return furthestTile;
	}

	static RectInt GetBounds(HashSet<Vector2Int> points)
	{
		if (points == null || points.Count == 0)
			return new RectInt(); // Returns RectInt(0,0,0,0)

		int minX = int.MaxValue;
		int maxX = int.MinValue;
		int minY = int.MaxValue;
		int maxY = int.MinValue;

		foreach (Vector2Int point in points)
		{
			if (point.x < minX) minX = point.x;
			if (point.x > maxX) maxX = point.x;
			if (point.y < minY) minY = point.y;
			if (point.y > maxY) maxY = point.y;
		}

		return new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
	}
}