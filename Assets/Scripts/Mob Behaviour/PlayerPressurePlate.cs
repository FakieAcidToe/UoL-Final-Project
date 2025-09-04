using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GOUnityEvent : UnityEvent<GameObject> { }

[RequireComponent(typeof(Collider2D))]
public class PlayerPressurePlate : MonoBehaviour
{
	[SerializeField] bool alsoIncludeEnemies = false;
	[SerializeField] bool alsoIncludeHitboxes = false;
	public GOUnityEvent OnPlayerEnter = new GOUnityEvent();

	void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
		if (player != null)
			OnPlayerEnter.Invoke(collision.gameObject);
		else
		{
			Enemy enemy = collision.gameObject.GetComponent<Enemy>();
			if (enemy != null && (alsoIncludeEnemies || enemy.IsBeingControlledByPlayer()))
				OnPlayerEnter.Invoke(collision.gameObject);
			else
			{
				Hitbox hitbox = collision.gameObject.GetComponent<Hitbox>();
				if (hitbox != null && alsoIncludeHitboxes && hitbox.CanHitPressurePlates())
					OnPlayerEnter.Invoke(collision.gameObject);
			}
		}
	}
}