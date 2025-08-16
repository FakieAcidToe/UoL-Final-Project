using UnityEngine;

public class ItemPrefab : MonoBehaviour
{
	[SerializeField] SpriteRenderer spriteRenderer;
	public PowerUpItem itemSO;
	[SerializeField] AudioClip pickupSfx;

	bool canTrigger = false;

	void Start()
	{
		if (itemSO != null)
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

			SoundManager.Instance.Play(pickupSfx);
		}
	}

	void EnableTrigger()
	{
		canTrigger = true;
	}

	public void SetColour(Color _newColor)
	{
		spriteRenderer.color = _newColor;
	}
}