using UnityEngine;

public class CircleDungeonGenerator : AbstractDungeonGenerator
{
	[SerializeField] Vector2Int dungeonSize = new Vector2Int(20, 20);

	protected override void RunProceduralGeneration()
	{
		floorPositions = ProceduralGenerationAlgorithms.EmptyCircleRoom(startPosition, dungeonSize);

		spawnPosition = startPosition;
		exitPosition = ProceduralGenerationAlgorithms.FindFurthestExit(floorPositions, spawnPosition);
	}

	public void SetDungeonSize(Vector2Int newDungeonSize)
	{
		dungeonSize = newDungeonSize;
	}
}