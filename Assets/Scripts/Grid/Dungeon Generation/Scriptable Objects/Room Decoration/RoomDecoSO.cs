using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Room Deco", menuName = "PCG/Room Decoration")]
public class RoomDecoSO : ScriptableObject
{
	[Header("Objects that Spawn in Rooms")]
	[SerializeField] SpawnedObjects[] objects;

	[System.Serializable]
	struct SpawnedObjects
	{
		public GameObject objectPrefab;
		public PlacementType placementType;
		[Range(0, 1)] public float placementChance;
		[Range(0, 1)] public float placementChanceIfNearby; // increased chance -> more items gathered together

		public void PlaceDecoration(Vector2Int position)
		{
			Instantiate(objectPrefab, position + Vector2.one / 2, Quaternion.identity);
		}
	}

	enum PlacementType
	{
		byWalls
	}

	public void PlaceDecorations(HashSet<Vector2Int> floorPositions)
	{
		foreach (SpawnedObjects obj in objects)
		{
			HashSet<Vector2Int> placedPositions = new HashSet<Vector2Int>();
			switch (obj.placementType)
			{
				case PlacementType.byWalls:
					foreach (Vector2Int position in floorPositions)
					{
						//Debug.DrawLine((Vector3Int)position, position+Vector2.one, Color.red, 20f);
						foreach (Vector2Int dir in Direction2D.cardinalDirectionsList) // check dir if tile is beside wall
						{
							if (!floorPositions.Contains(position + dir)) // has wall
							{
								bool hasCopyNearby = false;
								foreach (Vector2Int dir2 in Direction2D.cardinalDirectionsList) // check dir2 if tile is beside existing deco
								{
									if (placedPositions.Contains(position + dir2))
									{
										hasCopyNearby = true;
										break;
									}
								}

								// different chance of spawning if has existing deco beside
								if (Random.value <= (hasCopyNearby ? obj.placementChanceIfNearby : obj.placementChance))
								{
									obj.PlaceDecoration(position);
									placedPositions.Add(position);
								}
								break;
							}
						}
					}
					break;
			}
		}
	}
}
