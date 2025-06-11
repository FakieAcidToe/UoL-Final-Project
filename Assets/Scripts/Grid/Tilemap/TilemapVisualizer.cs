using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
	[SerializeField] Tilemap floorTilemap, wallTilemap;
	[SerializeField] TileBase[] floorTiles;
	[SerializeField] TileBase[] wallTiles;

	public Vector3 GetTilemapAnchor()
	{
		return floorTilemap.tileAnchor;
	}

	//public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
	//{
	//	PaintTiles(floorPositions, floorTilemap, floorTile);
	//}
	//
	//void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
	//{
	//	foreach (Vector2Int position in positions)
	//		PaintSingleTile(tilemap, tile, position);
	//}

	internal void PaintSingleFloor(Vector2Int position, string binaryType)
	{
		int typeAsInt = Convert.ToInt32(binaryType, 2);
		TileBase tile = null;
		for (int i = 0; i < WallTypesHelper.wallList.Length; ++i)
		{
			HashSet<int> wallSet = WallTypesHelper.wallList[i];
			if (wallSet.Contains(typeAsInt))
			{
				tile = floorTiles[i];
				break;
			}
		}

		if (tile != null)
			PaintSingleTile(floorTilemap, tile, position);
	}

	internal void PaintSingleWall(Vector2Int position, string binaryType)
	{
		int typeAsInt = Convert.ToInt32(binaryType, 2);
		TileBase tile = null;
		for (int i = 0; i < WallTypesHelper.wallList.Length; ++i)
		{
			HashSet<int> wallSet = WallTypesHelper.wallList[i];
			if (wallSet.Contains(typeAsInt))
			{
				tile = wallTiles[i];
				break;
			}
		}

		if (tile != null)
			PaintSingleTile(wallTilemap, tile, position);
	}

	void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
	{
		Vector3Int tilePosition = tilemap.WorldToCell((Vector3Int)position);
		tilemap.SetTile(tilePosition, tile);
	}

	public void Clear()
	{
		floorTilemap.ClearAllTiles();
		wallTilemap.ClearAllTiles();
	}
}