using UnityEngine;

[CreateAssetMenu(fileName = "DungeonParameters", menuName = "PCG/DungeonParamData")]
public class DungeonParamsSO : ScriptableObject
{
	[Header("Room and Dungeon Size")]
	[Min(1)] public int minRoomWidth = 12;
	[Min(1)] public int minRoomHeight = 12;
	[Min(1)] public int dungeonWidth = 90, dungeonHeight = 90;
	[Min(1)] public int minNumOfRooms = 3; // regenerate if less than this number

	[Header("Room Borders")]
	[Min(0)] public int offset = 1; // border size of each room (noise can generate here)
	[Range(0, 1)] public float noiseChance = 0.5f; // noise to generate outside offsets
	[Min(0)] public int border = 1; // border size of each room (nothing can generate here)

	[Header("Palette")]
	public TilemapPalette tilemapPalette;

	[Header("Room Generation Type")]
	public RoomFirstDungeonGenerator.RoomTypes roomType = RoomFirstDungeonGenerator.RoomTypes.RandomWalk; // what kind of rooms will be used?
	[Range(0, 1)] public float percentageOf1x1Rooms = 0.1f; // some rooms remain 1x1 size

	[Header("Corridors")]
	[Min(0)] public int corridorWidth = 1;
	[Range(0, 1)] public float corridorExtraLoopChance = 0.15f;

	[Header("Cellular Automata")]
	public bool cellularAutomataDontOverrideRooms = true;
	[Min(0)] public int cellularAutomataIterations = 2; // number of times to apply cellular automata loops

	[Header("Flood Fill")]
	public bool applyFloodFill = true;

	[Header("Enemy Spawn")]
	public EnemyStats[] enemyTypes;
}