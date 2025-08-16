using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BestiaryManager : GeneralManager
{
	[Header("Bestiary List")]
	[SerializeField] PlayerAnimationSet playerSet;
	[SerializeField] EnemyStats[] enemyList;
	[SerializeField] PowerUpItem[] itemList;

	[Header("Prefabs")]
	[SerializeField] MobAnimation standeePrefab;
	[SerializeField] ItemPrefab itemPrefab;

	[Header("Name")]
	[SerializeField] Text nameText;
	[SerializeField] Text itemText;
	[SerializeField] Text itemDescText;
	[SerializeField] string playerName = "Charmer";
	[SerializeField] string itemName = "No Item";

	[Header("Bestiary Positioning")]
	[SerializeField] Vector2 spacingApart = new Vector2(4, 0.5f);
	[SerializeField] Vector2 itemSpacingApart = new Vector2(1, -3f);
	[SerializeField] Vector2 itemOrigin;
	[SerializeField] Vector2 standeeScale = Vector2.one * 3;
	[SerializeField] Vector2 selectedStandeeScale = Vector2.one * 4;
	[SerializeField] float lerpScaleSpeed = 5;
	[SerializeField] float lerpMoveSpeed = 5;
	[SerializeField] AudioClip uiSound;

	[Header("Scene Changing")]
	[SerializeField] int gameplaySceneIndex = 1;

	// controller
	PlayerInputActions controls;
	InputAction moveAction;
	bool isPressedRight = false;
	bool isPressedLeft = false;
	bool isPressedUp = false;
	bool isPressedDown = false;
	float threshold = 0.5f;
	// mouse drag
	Vector2 dragPos; // mouse pos when drag started
	int thenDragSelection = 0; // selection when drag started
	int thenDragSelectionItem = 0;

	List<Transform> standees;
	List<Transform> items;
	int currentSelection = 0;
	int currentSelectionItem = 0;

	protected override void Awake()
	{
		base.Awake();

		controls = new PlayerInputActions();
		moveAction = controls.Gameplay.Move;
		standees = new List<Transform>();
		items = new List<Transform>();
	}

	protected override void Start()
	{
		base.Start();

		currentSelection = SaveManager.Instance.CurrentMiscData.selectedCharacter;
		currentSelectionItem = SaveManager.Instance.CurrentMiscData.selectedUpgrade;
		UpdateName();

		// player
		MobAnimation playerStandee = Instantiate(standeePrefab, GetTargetedPosition(0, currentSelection), Quaternion.identity);
		playerStandee.UpdateSpriteIndex(playerSet.idle, _animSpeed: playerSet.idleSpeed);
		playerStandee.SetFlipX(Vector2.right * (playerSet.isFacingRight ? 1 : -1));
		playerStandee.transform.localScale = GetTargetedScale(0, currentSelection);
		standees.Add(playerStandee.transform);

		// enemies
		for (int i = 0; i < enemyList.Length; ++i)
		{
			EnemyAnimationSet anims = enemyList[i].animationSet;
			MobAnimation standee = Instantiate(standeePrefab, GetTargetedPosition(i + 1, currentSelection), Quaternion.identity);
			standee.UpdateSpriteIndex(anims.idle, _animSpeed: anims.idleSpeed);
			standee.GetShadowRenderer().transform.localScale = new Vector3(anims.shadow.x, anims.shadow.y, 1);
			standee.SetFlipX(Vector2.right * (anims.isFacingRight ? 1 : -1));
			standee.transform.localScale = GetTargetedScale(i + 1, currentSelection);
			if (!SaveManager.Instance.CurrentSaveData.unlockedMonsters[i]) standee.SetColour(Color.black);
			standees.Add(standee.transform);
		}

		// no item
		ItemPrefab noItem = Instantiate(itemPrefab, GetItemTargetedPosition(0, currentSelectionItem), Quaternion.identity);
		noItem.transform.localScale = GetTargetedScale(0, currentSelectionItem);
		items.Add(noItem.transform);

		// items
		for (int i = 0; i < itemList.Length; ++i)
		{
			ItemPrefab item = Instantiate(itemPrefab, GetItemTargetedPosition(i + 1, currentSelectionItem), Quaternion.identity);
			item.itemSO = itemList[i];
			item.transform.localScale = GetTargetedScale(i + 1, currentSelectionItem);
			if (!SaveManager.Instance.CurrentSaveData.unlockedItems[i]) item.SetColour(Color.black);
			items.Add(item.transform);
		}
	}

	void OnEnable()
	{
		controls.Gameplay.Enable();
	}

	void OnDisable()
	{
		controls.Gameplay.Disable();
	}

	void Update()
	{
		// drag
		if (controls.Gameplay.DragMap.WasPressedThisFrame())
		{
			dragPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			thenDragSelection = currentSelection;
			thenDragSelectionItem = currentSelectionItem;
		}
		if (controls.Gameplay.DragMap.IsPressed())
		{
			int prevSelection = currentSelection;
			int prevSelectionItem = currentSelectionItem;
			Vector2 camPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			currentSelection = Mathf.Clamp(
				thenDragSelection + Mathf.RoundToInt((dragPos.x - camPos.x) / spacingApart.x),
				0, standees.Count - 1);
			currentSelectionItem = Mathf.Clamp(
				thenDragSelectionItem + Mathf.RoundToInt((dragPos.y - camPos.y) / itemSpacingApart.y),
				0, items.Count - 1);

			if (currentSelection != prevSelection || currentSelectionItem != prevSelectionItem)
			{
				PlaySFX(uiSound);
				UpdateName();
			}
		}
		else
		{
			// controls
			Vector2 input = moveAction.ReadValue<Vector2>() + controls.Gameplay.ZoomMap.ReadValue<Vector2>();
			if (input.x > threshold && !isPressedRight)
			{
				// right pressed
				isPressedRight = true;
				currentSelection = Mathf.Min(currentSelection + 1, standees.Count - 1);
				PlaySFX(uiSound);
				UpdateName();
			}
			else if (input.x <= threshold && isPressedRight)
			{
				// right released
				isPressedRight = false;
			}
			if (input.x < -threshold && !isPressedLeft)
			{
				// left pressed
				isPressedLeft = true;
				currentSelection = Mathf.Max(currentSelection - 1, 0);
				PlaySFX(uiSound);
				UpdateName();
			}
			else if (input.x >= -threshold && isPressedLeft)
			{
				// left released
				isPressedLeft = false;
			}
			if (input.y > threshold && !isPressedUp)
			{
				// up pressed
				isPressedUp = true;
				currentSelectionItem = Mathf.Max(currentSelectionItem - 1, 0);
				PlaySFX(uiSound);
				UpdateName();
			}
			else if (input.y <= threshold && isPressedUp)
			{
				// up released
				isPressedUp = false;
			}
			if (input.y < -threshold && !isPressedDown)
			{
				// down pressed
				isPressedDown = true;
				currentSelectionItem = Mathf.Min(currentSelectionItem + 1, items.Count - 1);
				PlaySFX(uiSound);
				UpdateName();
			}
			else if (input.y >= -threshold && isPressedDown)
			{
				// down released
				isPressedDown = false;
			}
		}

		// standee positioning
		for (int i = 0; i < standees.Count; ++i)
		{
			Transform standee = standees[i];
			standee.localScale = Vector3.Lerp(
				standee.localScale, GetTargetedScale(i, currentSelection),
				1f - Mathf.Exp(-lerpScaleSpeed * Time.deltaTime));

			standee.localPosition = Vector3.Lerp(
				standee.localPosition, GetTargetedPosition(i, currentSelection),
				1f - Mathf.Exp(-lerpMoveSpeed * Time.deltaTime));
		}

		// items positioning
		for (int i = 0; i < items.Count; ++i)
		{
			Transform item = items[i];
			item.localScale = Vector3.Lerp(
				item.localScale, GetTargetedScale(i, currentSelectionItem),
				1f - Mathf.Exp(-lerpScaleSpeed * Time.deltaTime));

			item.localPosition = Vector3.Lerp(
				item.localPosition, GetItemTargetedPosition(i, currentSelectionItem),
				1f - Mathf.Exp(-lerpMoveSpeed * Time.deltaTime));
		}
	}

	Vector3 GetTargetedScale(int i, int selection)
	{
		return selection == i ? new Vector3(selectedStandeeScale.x, selectedStandeeScale.y, 1) : new Vector3(standeeScale.x, standeeScale.y, 1);
	}

	Vector3 GetTargetedPosition(int i, int selection)
	{
		return new Vector3((i - selection) * spacingApart.x, Mathf.Abs(i - selection) * spacingApart.y, 1);
	}

	Vector3 GetItemTargetedPosition(int i, int selection)
	{
		return new Vector3(Mathf.Abs(i - selection) * itemSpacingApart.x + itemOrigin.x, (i - selection) * itemSpacingApart.y + itemOrigin.y, 1);
	}

	void UpdateName()
	{
		nameText.text = currentSelection == 0 ? playerName :
			SaveManager.Instance.CurrentSaveData.unlockedMonsters[currentSelection - 1] ? enemyList[currentSelection - 1].enemyName :
			"???";
		itemText.text = currentSelectionItem == 0 ? itemName :
			SaveManager.Instance.CurrentSaveData.unlockedItems[currentSelectionItem - 1] ? itemList[currentSelectionItem - 1].itemName :
			"???";
		itemDescText.text = currentSelectionItem > 0 && SaveManager.Instance.CurrentSaveData.unlockedItems[currentSelectionItem - 1] ? itemList[currentSelectionItem - 1].itemDesc :
			"";
	}


	public void SelectCharacter()
	{
		if ((currentSelection <= 0 || SaveManager.Instance.CurrentSaveData.unlockedMonsters[currentSelection - 1]) &&
			(currentSelectionItem <= 0 || SaveManager.Instance.CurrentSaveData.unlockedItems[currentSelectionItem - 1]))
		{
			SaveSelectCharacter();
			ChangeScene(gameplaySceneIndex);
		}
		else
		{
			if (currentSelection > 0 && !SaveManager.Instance.CurrentSaveData.unlockedMonsters[currentSelection - 1]) currentSelection = 0;
			if (currentSelectionItem > 0 && !SaveManager.Instance.CurrentSaveData.unlockedItems[currentSelectionItem - 1]) currentSelectionItem = 0;
			UpdateName();
		}
	}


	public void SaveSelectCharacter()
	{
		SaveManager.Instance.CurrentMiscData.selectedCharacter = currentSelection;
		SaveManager.Instance.CurrentMiscData.selectedUpgrade = currentSelectionItem;
	}
}