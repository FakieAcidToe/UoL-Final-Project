using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemUser : MonoBehaviour
{
	[SerializeField] PowerUpItem currentItem;
	float cooldownTimer = 0;
	public PlayerInputActions controls { private set; get; }

	void Awake()
	{
		controls = KeybindLoader.GetNewInputActions();
	}

	void Start()
	{
		if (currentItem != null)
			PickUpItem(currentItem);
	}

	void OnEnable()
	{
		controls.Gameplay.Enable();
		controls.Gameplay.Item.performed += AttemptUseItem;
	}

	void OnDisable()
	{
		controls.Gameplay.Disable();
		controls.Gameplay.Item.performed -= AttemptUseItem;
	}

	void AttemptUseItem(InputAction.CallbackContext context)
	{
		AttemptUseItem();
	}

	public void AttemptUseItem()
	{
		if (currentItem != null && currentItem.IsActiveAbility() && cooldownTimer <= 0)
		{
			currentItem.UseItem(this);
			cooldownTimer = currentItem.GetCooldownTime();
		}
	}

	void Update()
	{
		if (cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer < 0)
				cooldownTimer = 0;
		}
		if (currentItem != null)
			currentItem.ItemUpdate(this);
	}

	void FixedUpdate()
	{
		if (currentItem != null)
			currentItem.ItemFixedUpdate(this);
	}

	public void PickUpItem(PowerUpItem _item)
	{
		DropItem();
		currentItem = _item;
		currentItem.PickUpItem(this);
	}

	public void DropItem()
	{
		if (currentItem != null) currentItem.DropItem(this);
		currentItem = null;
	}

	public void HandOverItem(ItemUser _receivingUser)
	{
		_receivingUser.PickUpItem(currentItem);
		DropItem();
	}
}