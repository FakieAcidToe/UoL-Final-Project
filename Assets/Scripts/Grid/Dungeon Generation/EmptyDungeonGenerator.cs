using System.Collections.Generic;
using UnityEngine;

public class EmptyDungeonGenerator : AbstractDungeonGenerator
{
	[SerializeField] Vector2Int dungeonSize = new Vector2Int(20, 20);

	protected override void RunProceduralGeneration()
	{
		HashSet<Vector2Int> floorPositions = ProceduralGenerationAlgorithms.EmptyRectRoom(startPosition, dungeonSize);
		tilemapVisualizer.Clear();
		TileGenerator.GenerateTiles(floorPositions, tilemapVisualizer);

		spawnPosition = startPosition;
		exitPosition = ProceduralGenerationAlgorithms.FindFurthestExit(floorPositions, spawnPosition);
	}
}