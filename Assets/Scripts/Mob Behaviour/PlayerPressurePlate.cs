using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PlayerPressurePlate : MonoBehaviour
{
	public UnityEvent OnPlayerEnter;

	void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
		if (player != null)
			OnPlayerEnter.Invoke();
		else
		{
			Enemy enemy = collision.gameObject.GetComponent<Enemy>();
			if (enemy != null && enemy.IsBeingControlledByPlayer())
				OnPlayerEnter.Invoke();
		}
	}
}