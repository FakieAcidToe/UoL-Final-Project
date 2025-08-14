using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Dash Item", menuName = "Items/Dash Item")]
public class DashItem : PowerUpItem
{
	[SerializeField, Min(0)] float dashTime = 0.2f;
	[SerializeField, Min(0)] float dashSpeed = 10f;

	Dictionary<ItemUser, UniqueVariables> varsDict = new Dictionary<ItemUser, UniqueVariables>();
	struct UniqueVariables
	{
		public float dashTimer;
		public Vector2 direction;
	}

	public override void UseItem(ItemUser self)
	{
		UniqueVariables vars = varsDict.ContainsKey(self) ? varsDict[self] : new UniqueVariables();

		vars.dashTimer = 0;

		vars.direction = self.controls.Gameplay.Move.ReadValue<Vector2>();
		if (vars.direction.magnitude <= 0)
			vars.direction = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - self.transform.position;
		vars.direction.Normalize();

		if (varsDict.ContainsKey(self)) varsDict[self] = vars;
		else varsDict.Add(self, vars);
	}

	public override void ItemFixedUpdate(ItemUser self)
	{
		if (varsDict.ContainsKey(self))
		{
			UniqueVariables vars = varsDict[self];

			if (vars.dashTimer < dashTime)
			{
				vars.dashTimer += Time.fixedDeltaTime;
				Rigidbody2D rb = self.GetComponent<Rigidbody2D>();
				if (rb != null)
					rb.MovePosition(self.transform.position + (Vector3)vars.direction * dashSpeed * Time.fixedDeltaTime);
			}

			varsDict[self] = vars;
		}
	}

	public override void DropItem(ItemUser self)
	{
		if (varsDict.ContainsKey(self))
		{
			varsDict.Remove(self);
		}
	}
}
