using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
	public Transform target;
	[SerializeField, Range(0, 1)] float smoothSpeed = 0.125f;
	[SerializeField] Vector3 offset;

	void LateUpdate()
	{
		if (target == null) return;

		Vector3 smoothedPosition = Vector3.Lerp(transform.position, target.position + offset, smoothSpeed);
		transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
	}

	public void SetPositionToTarget()
	{
		Vector3 targetPosition = target.position + offset;
		transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
	}
}