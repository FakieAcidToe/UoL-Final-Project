using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinding
{
	static public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos, HashSet<Vector2Int> tiles)
	{
		PriorityQueue<Vector2Int> openSet = new PriorityQueue<Vector2Int>();
		Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
		Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();

		openSet.Enqueue(startPos, 0);
		gScore[startPos] = 0;

		while (openSet.Count > 0)
		{
			Vector2Int current = openSet.Dequeue();

			if (current == targetPos)
				return ReconstructPath(cameFrom, current);

			foreach (Vector2Int neighbor in GetNeighbors(current))
			{
				if (!tiles.Contains(neighbor))
					continue;

				int tentativeGScore = gScore[current] + 1;

				if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
				{
					cameFrom[neighbor] = current;
					gScore[neighbor] = tentativeGScore;
					int fScore = tentativeGScore + Heuristic(neighbor, targetPos);
					openSet.Enqueue(neighbor, fScore);
				}
			}
		}

		return new List<Vector2Int>(); // No path found
	}

	static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
	{
		List<Vector2Int> path = new List<Vector2Int> { current };
		while (cameFrom.ContainsKey(current))
		{
			current = cameFrom[current];
			path.Add(current);
		}
		path.Reverse();
		return path;
	}

	static int Heuristic(Vector2Int a, Vector2Int b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
	}

	static List<Vector2Int> GetNeighbors(Vector2Int pos)
	{
		return new List<Vector2Int>
		{
			pos + Vector2Int.up,
			pos + Vector2Int.down,
			pos + Vector2Int.left,
			pos + Vector2Int.right
		};
	}

	class PriorityQueue<T>
	{
		private List<(T item, int priority)> elements = new List<(T, int)>();

		public int Count => elements.Count;

		public void Enqueue(T item, int priority)
		{
			elements.Add((item, priority));
		}

		public T Dequeue()
		{
			int bestIndex = 0;

			for (int i = 1; i < elements.Count; i++)
			{
				if (elements[i].priority < elements[bestIndex].priority)
				{
					bestIndex = i;
				}
			}

			T bestItem = elements[bestIndex].item;
			elements.RemoveAt(bestIndex);
			return bestItem;
		}
	}
}