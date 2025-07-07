using UnityEngine;

[CreateAssetMenu(fileName = "DungeonParameters", menuName = "PCG/DungeonParamData")]
public class DungeonParamsSO : ScriptableObject
{
	[Min(1)] public int minRoomWidth = 12, minRoomHeight = 12;
	[Min(1)] public int dungeonWidth = 90, dungeonHeight = 90;
	[Range(0, 10)] public int offset = 1; // border size of each room
	public RoomFirstDungeonGenerator.RoomTypes roomType = RoomFirstDungeonGenerator.RoomTypes.RandomWalk; // what kind of rooms will be used?
	public bool generateCorridors = true;
	[Range(0, 1)] public float percentageOf1x1Rooms = 0.1f; // some rooms remain 1x1 size
	[Min(0)] public int cellularAutomataIterations = 2; // number of times to apply cellular automata loops
}