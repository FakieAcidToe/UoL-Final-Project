using UnityEngine;

public class ItemPrefab : MonoBehaviour
{
	[SerializeField] SpriteRenderer spriteRenderer;
	public PowerUpItem itemSO;
	
	void Start()
	{
		spriteRenderer.sprite = itemSO.GetIconSprite();
	}

	public void OnContact(GameObject go)
	{
		ItemUser itemUser = go.GetComponent<ItemUser>();
		if (itemUser != null)
		{
			if (itemUser.currentItem == null) // has no items
			{
				itemUser.PickUpItem(itemSO);
				gameObject.SetActive(false);
			}
			else // has item already; swap
			{
				PowerUpItem prevItem = itemUser.currentItem;
				itemUser.PickUpItem(itemSO);

				itemSO = prevItem;
				spriteRenderer.sprite = itemSO.GetIconSprite();
			}
		}
	}
}