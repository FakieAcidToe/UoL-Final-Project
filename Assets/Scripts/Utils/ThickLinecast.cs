using UnityEngine;

public static class ThickLinecast
{
	public static RaycastHit2D[] ThickLinecast2D(Vector2 start, Vector2 end, float thickness, LayerMask layerMask)
	{
		Vector2 direction = end - start;
		float distance = direction.magnitude;
		direction.Normalize();

		Vector2 center = (start + end) / 2;
		Vector2 size = new Vector2(distance, thickness);
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

		RaycastHit2D[] hits = Physics2D.BoxCastAll(center, size, angle, Vector2.zero, 0f, layerMask);
		return hits;
	}

	public static void DrawThickLineGizmo(Vector2 start, Vector2 end, float thickness, Color color)
	{
		Vector2 direction = end - start;
		float length = direction.magnitude;
		direction.Normalize();

		Vector2 center = (start + end) / 2f;
		Vector2 size = new Vector2(length, thickness);
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

		Gizmos.color = color;
		Matrix4x4 oldMatrix = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, size);
		Gizmos.matrix = oldMatrix;
	}
}