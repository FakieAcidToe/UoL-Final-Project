using System.Collections.Generic;
using UnityEngine;

public static class TileGenerator
{
	public static void GenerateTiles(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
	{
		HashSet<Vector2Int> wallPositions = FindWallsInDirections(floorPositions, Direction2D.eightDirectionsList);
		CreateFloor(tilemapVisualizer, floorPositions);
		CreateWall(tilemapVisualizer, wallPositions, floorPositions);
	}

	static void CreateFloor(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> floorPositions)
	{
		foreach (Vector2Int position in floorPositions)
		{
			string neighboursBinaryType = "";
			foreach (Vector2Int direction in Direction2D.eightDirectionsList)
			{
				Vector2Int neighbourPosition = position + direction;
				neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "0" : "1";
			}
			tilemapVisualizer.PaintSingleFloor(position, neighboursBinaryType);
		}
	}

	static void CreateWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> wallPositions, HashSet<Vector2Int> floorPositions)
	{
		foreach (Vector2Int position in wallPositions)
		{
			string neighboursBinaryType = "";
			foreach (Vector2Int direction in Direction2D.eightDirectionsList)
			{
				Vector2Int neighbourPosition = position + direction;
				neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "1" : "0";
			}
			tilemapVisualizer.PaintSingleWall(position, neighboursBinaryType);
		}
	}

	static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
	{
		HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
		foreach (Vector2Int position in floorPositions)
		{
			foreach (Vector2Int direction in directionList)
			{
				Vector2Int neighbourPosition = position + direction;
				if (!floorPositions.Contains(neighbourPosition))
					wallPositions.Add(neighbourPosition);
			}
		}
		return wallPositions;
	}
}