using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
	[SerializeField] int corridorLength = 14, corridorCount = 5;
	[SerializeField, Range(0.1f, 1)] float roomPercent = 0.8f;

	protected override void RunProceduralGeneration()
	{
		CorridorFirstGeneration();
	}

	void CorridorFirstGeneration()
	{
		HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
		HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();

		CreateCorridors(floorPositions, potentialRoomPositions);

		HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);

		List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

		CreateRoomsAtDeadEnd(deadEnds, roomPositions);

		floorPositions.UnionWith(roomPositions);

		tilemapVisualizer.PaintFloorTiles(floorPositions);
		WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);

	}

	void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
	{
		foreach (Vector2Int position in deadEnds)
		{
			if (!roomFloors.Contains(position))
			{
				HashSet<Vector2Int> room = RunRandomWalk(randomWalkParameters, position);
				roomFloors.UnionWith(room);
			}
		}
	}

	List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
	{
		List<Vector2Int> deadEnds = new List<Vector2Int>();
		foreach (Vector2Int position in floorPositions)
		{
			int neighboursCount = 0;
			foreach (Vector2Int direction in Direction2D.cardinalDirectionsList)
				if (floorPositions.Contains(position + direction)) neighboursCount++;
			if (neighboursCount == 1) deadEnds.Add(position);
		}
		return deadEnds;
	}

	HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
	{
		HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
		int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

		List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();

		foreach (Vector2Int roomPosition in roomsToCreate)
		{
			HashSet<Vector2Int> roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);
			roomPositions.UnionWith(roomFloor);
		}
		return roomPositions;
	}

	void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions)
	{
		Vector2Int currentPosition = startPosition;
		potentialRoomPositions.Add(currentPosition);

		for (int i = 0; i < corridorCount; ++i)
		{
			List<Vector2Int> corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
			currentPosition = corridor[corridor.Count - 1];
			potentialRoomPositions.Add(currentPosition);
			floorPositions.UnionWith(corridor);
		}
	}
}