using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
	public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
	{
		HashSet<Vector2Int> basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList);
		HashSet<Vector2Int> cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.diagonalDirectionsList);
		CreateBasicWall(tilemapVisualizer, basicWallPositions, floorPositions);
		CreateCornerWalls(tilemapVisualizer, cornerWallPositions, floorPositions);
	}

	static void CreateCornerWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
	{
		foreach (Vector2Int position in cornerWallPositions)
		{
			string neighboursBinaryType = "";
			foreach (Vector2Int direction in Direction2D.eightDirectionsList)
			{
				Vector2Int neighbourPosition = position + direction;
				neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "1" : "0";
			}
			tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
		}
	}

	static void CreateBasicWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPositions, HashSet<Vector2Int> floorPositions)
	{
		foreach (Vector2Int position in basicWallPositions)
		{
			string neighboursBinaryType = "";
			foreach (Vector2Int direction in Direction2D.cardinalDirectionsList)
			{
				Vector2Int neighbourPosition = position + direction;
				neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "1" : "0";
			}
			tilemapVisualizer.PaintSingleBasicWall(position, neighboursBinaryType);
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