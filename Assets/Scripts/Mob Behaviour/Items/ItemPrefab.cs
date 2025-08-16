using UnityEngine;

public class ItemPrefab : MonoBehaviour
{
	[SerializeField] SpriteRenderer spriteRenderer;
	public PowerUpItem itemSO;

	bool canTrigger = false;

	void Start()
	{
		spriteRenderer.sprite = itemSO.GetIconSprite();

		// delay enabling trigger handling
		Invoke(nameof(EnableTrigger), 0.1f);
	}

	public void OnContact(GameObject go)
	{
		if (!canTrigger) return;

		ItemUser itemUser = go.GetComponent<ItemUser>();
		if (itemUser != null)
		{
			if (itemUser.currentItem == null) // has no items
			{
				itemUser.PickUpItem(itemSO);
				//gameObject.SetActive(false);
				Destroy(gameObject);
			}
			else // has item already; swap
			{
				PowerUpItem prevItem = itemUser.currentItem;
				itemUser.PickUpItem(itemSO);

				itemSO = prevItem;
				spriteRenderer.sprite = itemSO.GetIconSprite();
				transform.position = go.transform.position;
			}
		}
	}

	void EnableTrigger()
	{
		canTrigger = true;
	}
}