using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpItem", menuName = "Items/Abstract Item")]
public class PowerUpItem : ScriptableObject
{
	[SerializeField, Min(0)] int id;
	[SerializeField] Sprite itemIcon;
	[SerializeField, Min(0)] float cooldownTime;
	[SerializeField] bool isActiveAbility = false;

	public virtual void PickUpItem(ItemUser self)
	{

	}

	public virtual void DropItem(ItemUser self)
	{

	}

	public virtual void UseItem(ItemUser self)
	{

	}

	public virtual void ItemUpdate(ItemUser self)
	{

	}

	public virtual void ItemFixedUpdate(ItemUser self)
	{

	}

	public bool IsActiveAbility()
	{
		return isActiveAbility;
	}

	public float GetCooldownTime()
	{
		return cooldownTime;
	}

	public Sprite GetIconSprite()
	{
		return itemIcon;
	}
}