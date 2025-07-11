using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Dungeon Tile Palette", menuName = "PCG/Dungeon Tile Palette")]
public class TilemapPalette : ScriptableObject
{
	public TileBase[] floorTiles;
	public TileBase[] wallTiles;
	public Color bgColour;
}