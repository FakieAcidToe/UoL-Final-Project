using UnityEngine;
using UnityEngine.Events;

public class DungeonExit : MonoBehaviour
{
	public UnityEvent OnPlayerExitDungeon;

	void OnTriggerEnter2D(Collider2D collision)
	{
		PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
		if (player != null)
			OnPlayerExitDungeon.Invoke();
		else
		{
			Enemy enemy = collision.gameObject.GetComponent<Enemy>();
			if (enemy != null && enemy.IsBeingControlledByPlayer())
				OnPlayerExitDungeon.Invoke();
		}
	}
}