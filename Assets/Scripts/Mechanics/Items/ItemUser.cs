using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ItemUser : MonoBehaviour
{
	public PowerUpItem currentItem { private set; get; }
	float cooldownTimer = 0;
	public PlayerInputActions controls { private set; get; }
	public Image itemIcon;

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
			if (itemIcon != null) itemIcon.fillAmount = 1 - cooldownTimer / currentItem.GetCooldownTime();
			if (cooldownTimer < 0)
			{
				cooldownTimer = 0;
				if (itemIcon != null) itemIcon.fillAmount = 1;
			}
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
		if (currentItem != null) currentItem.PickUpItem(this);
		UpdateItemIcon();
	}

	public void DropItem()
	{
		if (currentItem != null) currentItem.DropItem(this);
		currentItem = null;
		UpdateItemIcon();
	}

	public void HandOverItem(ItemUser _receivingUser)
	{
		PowerUpItem prevItem = currentItem;
		_receivingUser.itemIcon = itemIcon;

		DropItem();
		_receivingUser.PickUpItem(prevItem);

		itemIcon = null;
		_receivingUser.cooldownTimer = cooldownTimer;
		cooldownTimer = 0;
	}

	void UpdateItemIcon()
	{
		if (itemIcon == null) return;
		itemIcon.sprite = currentItem == null ? null : currentItem.GetIconSprite();
		itemIcon.enabled = itemIcon.sprite != null;
	}
}