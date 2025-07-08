using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
	[SerializeField] protected TilemapVisualizer tilemapVisualizer = null;
	[SerializeField] protected Vector2Int startPosition = Vector2Int.zero;

	protected Vector2Int spawnPosition = Vector2Int.zero;
	protected Vector2Int exitPosition = Vector2Int.right;

	public void GenerateDungeon()
	{
		tilemapVisualizer.Clear();
		RunProceduralGeneration();
	}

	public Vector2 GetSpawnLocation()
	{
		return spawnPosition + (Vector2)tilemapVisualizer.GetTilemapAnchor();
	}

	protected abstract void RunProceduralGeneration();

	protected virtual void OnDrawGizmosSelected()
	{
		// spawn point
		if (spawnPosition != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere((Vector3Int)spawnPosition, 1);
		}

		// spawn point
		if (exitPosition != null)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere((Vector3Int)exitPosition, 1);
		}
	}
}