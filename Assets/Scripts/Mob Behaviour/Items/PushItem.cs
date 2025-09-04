//using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Push Item", menuName = "Items/Push Item")]
public class PushItem : PowerUpItem
{
	[SerializeField] Hitbox hitboxPrefab;
	[SerializeField] AudioClip pushSFX;

	//Dictionary<ItemUser, UniqueVariables> varsDict = new Dictionary<ItemUser, UniqueVariables>();
	//struct UniqueVariables
	//{
	//	public float dashTimer;
	//	public Vector2 direction;
	//	public AfterImageSpawner spawner;
	//}

	public override void UseItem(ItemUser self)
	{
		//UniqueVariables vars = varsDict.ContainsKey(self) ? varsDict[self] : new UniqueVariables();

		//vars.dashTimer = 0;

		//vars.direction = self.controls.Gameplay.Move.ReadValue<Vector2>();
		//if (vars.direction.magnitude <= 0)
		//	vars.direction = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - self.transform.position;
		//vars.direction.Normalize();

		AudioSource audioSource = self.GetComponent<AudioSource>();
		if (audioSource == null) SoundManager.Instance.Play(pushSFX);
		else audioSource.PlayOneShot(pushSFX);

		Hitbox hbox = Instantiate(hitboxPrefab, self.transform.position, Quaternion.identity, self.transform);
		hbox.SetDirection(Vector2.right);
		hbox.owner = self.GetComponent<Enemy>();

		//vars.spawner = self.GetComponent<AfterImageSpawner>();
		//if (vars.spawner != null) vars.spawner.enabled = true;

		//if (varsDict.ContainsKey(self)) varsDict[self] = vars;
		//else varsDict.Add(self, vars);
	}
	/*
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
					rb.MovePosition(rb.position + vars.direction * dashSpeed * Time.fixedDeltaTime);
			}
			else
			{
				if (vars.spawner != null) vars.spawner.enabled = false;
			}

			varsDict[self] = vars;
		}
	}

	public override void DropItem(ItemUser self)
	{
		if (varsDict.ContainsKey(self))
		{
			UniqueVariables vars = varsDict[self];
			if (vars.spawner != null) vars.spawner.enabled = false;
			varsDict.Remove(self);
		}
	}*/
}
