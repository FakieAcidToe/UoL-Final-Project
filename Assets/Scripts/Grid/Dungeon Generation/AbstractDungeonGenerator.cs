using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
	[SerializeField] private TilemapVisualizer tilemapVisualizer = null;
	[SerializeField] protected Vector2Int startPosition = Vector2Int.zero;

	protected Vector2Int spawnPosition = Vector2Int.zero;
	protected Vector2Int exitPosition = Vector2Int.right;

	public HashSet<Vector2Int> floorPositions { protected set; get; }

	public void GenerateDungeon()
	{
		tilemapVisualizer.Clear();
		if (floorPositions == null) floorPositions = new HashSet<Vector2Int>();
		else floorPositions.Clear();

		RunProceduralGeneration();

		TileGenerator.GenerateTiles(floorPositions, tilemapVisualizer);
	}

	public Vector2 GetSpawnLocation()
	{
		return spawnPosition + (Vector2)tilemapVisualizer.GetTilemapAnchor();
	}

	public Vector2 GetExitLocation()
	{
		return exitPosition + (Vector2)tilemapVisualizer.GetTilemapAnchor();
	}

	public Vector2 GetTilemapOfset()
	{
		return (Vector2)tilemapVisualizer.GetTilemapAnchor();
	}

	protected abstract void RunProceduralGeneration();

	protected virtual void OnDrawGizmosSelected()
	{
		// spawn point
		if (spawnPosition != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(GetSpawnLocation(), 1);
		}

		// spawn point
		if (exitPosition != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(GetExitLocation(), 1);
		}
	}
}