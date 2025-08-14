using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GOUnityEvent : UnityEvent<GameObject> { }

[RequireComponent(typeof(Collider2D))]
public class PlayerPressurePlate : MonoBehaviour
{
	public GOUnityEvent OnPlayerEnter = new GOUnityEvent();

	void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
		if (player != null)
			OnPlayerEnter.Invoke(collision.gameObject);
		else
		{
			Enemy enemy = collision.gameObject.GetComponent<Enemy>();
			if (enemy != null && enemy.IsBeingControlledByPlayer())
				OnPlayerEnter.Invoke(collision.gameObject);
		}
	}
}