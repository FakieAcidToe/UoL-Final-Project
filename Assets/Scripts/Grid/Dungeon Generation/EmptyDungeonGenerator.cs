using System.Collections.Generic;
using UnityEngine;

public class EmptyDungeonGenerator : AbstractDungeonGenerator
{
	[SerializeField] Vector2Int dungeonSize = new Vector2Int(20, 20);

	protected override void RunProceduralGeneration()
	{
		floorPositions = ProceduralGenerationAlgorithms.EmptyRectRoom(startPosition, dungeonSize);

		spawnPosition = startPosition;
		exitPosition = ProceduralGenerationAlgorithms.FindFurthestExit(floorPositions, spawnPosition);
	}
}