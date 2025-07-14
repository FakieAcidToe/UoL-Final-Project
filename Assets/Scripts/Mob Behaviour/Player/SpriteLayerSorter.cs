using UnityEngine;

public class SpriteLayerSorter : MonoBehaviour
{
	[SerializeField, Tooltip("Null = find during Awake")] SpriteRenderer spriteRenderer;
	[SerializeField, Tooltip("Origin position of the sprite for depth calculation")] float yOffset = 0f;

	void Awake()
	{
		if (spriteRenderer == null)
			spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void LateUpdate()
	{
		spriteRenderer.sortingOrder = Mathf.RoundToInt((transform.position.y + yOffset) * -100);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position + Vector3.up*yOffset, 0.01f);
	}
}