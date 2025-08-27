using UnityEngine;
using UnityEngine.Events;

public class EventOnDirectionPress : MonoBehaviour
{
	public UnityEvent upPressed;
	public UnityEvent downPressed;
	public UnityEvent leftPressed;
	public UnityEvent rightPressed;

	bool upCalled = false;
	bool downCalled = false;
	bool leftCalled = false;
	bool rightCalled = false;

	public UnityEvent allPressed;

	void Update()
	{
		Vector2 movement;
		movement.x = Input.GetAxisRaw("Horizontal");
		movement.y = Input.GetAxisRaw("Vertical");

		if (movement.x < 0)
		{
			leftPressed.Invoke();
			leftCalled = true;
			CheckIfAllPressed();
		}
		else if (movement.x > 0)
		{
			rightPressed.Invoke();
			rightCalled = true;
			CheckIfAllPressed();
		}

		if (movement.y < 0)
		{
			downPressed.Invoke();
			downCalled = true;
			CheckIfAllPressed();
		}
		else if (movement.y > 0)
		{
			upPressed.Invoke();
			upCalled = true;
			CheckIfAllPressed();
		}
	}

	void CheckIfAllPressed()
	{
		if (upCalled && downCalled && leftCalled && rightCalled)
		{
			allPressed.Invoke();
			upCalled = false;
			downCalled = false;
			leftCalled = false;
			rightCalled = false;
		}
	}
}