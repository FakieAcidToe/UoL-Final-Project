using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinding
{
	private static readonly Vector2Int[] Directions = new Vector2Int[]
	{
		Vector2Int.up,
		Vector2Int.down,
		Vector2Int.left,
		Vector2Int.right
	};

	public static List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos, HashSet<Vector2Int> tiles, Dictionary<Vector2Int, List<Vector2Int>> neighborCache = null)
	{
		PriorityQueue<Vector2Int> openSet = new PriorityQueue<Vector2Int>();
		Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
		Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();
		HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
		if (neighborCache == null) neighborCache = new Dictionary<Vector2Int, List<Vector2Int>>();

		gScore[startPos] = 0;
		openSet.Enqueue(startPos, Heuristic(startPos, targetPos));

		while (openSet.Count > 0)
		{
			Vector2Int current = openSet.Dequeue();

			if (closedSet.Contains(current)) continue;

			if (current == targetPos) return ReconstructPath(cameFrom, current);

			closedSet.Add(current);

			List<Vector2Int> neighbors;
			if (!neighborCache.TryGetValue(current, out neighbors))
			{
				neighbors = new List<Vector2Int>();
				for (int i = 0; i < Directions.Length; ++i)
				{
					Vector2Int neighbor = current + Directions[i];
					if (tiles.Contains(neighbor))
						neighbors.Add(neighbor);
				}
				neighborCache[current] = neighbors;
			}

			for (int i = 0; i < neighbors.Count; ++i)
			{
				Vector2Int neighbor = neighbors[i];

				if (closedSet.Contains(neighbor))
					continue;

				int tentativeGScore = gScore[current] + 1;

				int existingScore;
				bool hasScore = gScore.TryGetValue(neighbor, out existingScore);
				if (!hasScore || tentativeGScore < existingScore)
				{
					cameFrom[neighbor] = current;
					gScore[neighbor] = tentativeGScore;
					int fScore = tentativeGScore + Heuristic(neighbor, targetPos);
					openSet.Enqueue(neighbor, fScore);
				}
			}
		}

		return new List<Vector2Int>(); // no path found
	}

	private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
	{
		List<Vector2Int> path = new List<Vector2Int> { current };
		Vector2Int prev;
		while (cameFrom.TryGetValue(current, out prev))
		{
			current = prev;
			path.Add(current);
		}
		path.Reverse();
		return path;
	}

	private static int Heuristic(Vector2Int a, Vector2Int b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
	}

	private class PriorityQueue<T>
	{
		private List<KeyValuePair<T, int>> heap = new List<KeyValuePair<T, int>>();

		public int Count { get { return heap.Count; } }

		public void Enqueue(T item, int priority)
		{
			heap.Add(new KeyValuePair<T, int>(item, priority));
			HeapifyUp(heap.Count - 1);
		}

		public T Dequeue()
		{
			if (heap.Count == 0) return default;

			T item = heap[0].Key;
			heap[0] = heap[heap.Count - 1];
			heap.RemoveAt(heap.Count - 1);
			HeapifyDown(0);
			return item;
		}

		private void HeapifyUp(int index)
		{
			while (index > 0)
			{
				int parent = (index - 1) / 2;
				if (heap[index].Value >= heap[parent].Value)
					break;

				Swap(index, parent);
				index = parent;
			}
		}

		private void HeapifyDown(int index)
		{
			int lastIndex = heap.Count - 1;
			while (true)
			{
				int left = index * 2 + 1;
				int right = index * 2 + 2;
				int smallest = index;

				if (left <= lastIndex && heap[left].Value < heap[smallest].Value)
					smallest = left;
				if (right <= lastIndex && heap[right].Value < heap[smallest].Value)
					smallest = right;

				if (smallest == index)
					break;

				Swap(index, smallest);
				index = smallest;
			}
		}

		private void Swap(int i, int j)
		{
			KeyValuePair<T, int> temp = heap[i];
			heap[i] = heap[j];
			heap[j] = temp;
		}
	}
}