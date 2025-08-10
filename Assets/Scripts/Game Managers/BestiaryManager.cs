using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BestiaryManager : GeneralManager
{
	[Header("Bestiary List")]
	[SerializeField] PlayerAnimationSet playerSet;
	[SerializeField] EnemyStats[] enemyList;

	[Header("Prefabs")]
	[SerializeField] MobAnimation standeePrefab;

	[Header("Name")]
	[SerializeField] Text nameText;
	[SerializeField] string playerName = "Charmer";

	[Header("Bestiary Positioning")]
	[SerializeField] Vector2 spacingApart = new Vector2(4, 0.5f);
	[SerializeField] Vector2 standeeScale = Vector2.one * 3;
	[SerializeField] Vector2 selectedStandeeScale = Vector2.one * 4;
	[SerializeField] float lerpScaleSpeed = 5;
	[SerializeField] float lerpMoveSpeed = 5;

	[Header("Scene Changing")]
	[SerializeField] int gameplaySceneIndex = 1;

	// controller
	PlayerInputActions controls;
	InputAction moveAction;
	bool isPressedRight = false;
	bool isPressedLeft = false;
	float threshold = 0.5f;

	List<Transform> standees;
	int currentSelection = 0;

	protected override void Awake()
	{
		base.Awake();

		controls = new PlayerInputActions();
		moveAction = controls.Gameplay.Move;
		standees = new List<Transform>();
	}

	void Start()
	{
		currentSelection = SaveManager.Instance.CurrentMiscData.selectedCharacter;
		UpdateName();

		// player
		MobAnimation playerStandee = Instantiate(standeePrefab, GetTargetedPosition(0), Quaternion.identity);
		playerStandee.UpdateSpriteIndex(playerSet.idle, _animSpeed: playerSet.idleSpeed);
		playerStandee.SetFlipX(Vector2.right * (playerSet.isFacingRight ? 1 : -1));
		playerStandee.transform.localScale = GetTargetedScale(0);
		standees.Add(playerStandee.transform);

		// enemies
		for (int i = 0; i < enemyList.Length; ++i)
		{
			EnemyAnimationSet anims = enemyList[i].animationSet;
			MobAnimation standee = Instantiate(standeePrefab, GetTargetedPosition(i + 1), Quaternion.identity);
			standee.UpdateSpriteIndex(anims.idle, _animSpeed: anims.idleSpeed);
			standee.GetShadowRenderer().transform.localScale = new Vector3(anims.shadow.x, anims.shadow.y, 1);
			standee.SetFlipX(Vector2.right * (anims.isFacingRight ? 1 : -1));
			standee.transform.localScale = GetTargetedScale(i + 1);
			if (!SaveManager.Instance.CurrentSaveData.unlockedMonsters[i]) standee.SetColour(Color.black);
			standees.Add(standee.transform);
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
		// controls
		float horizontalInput = moveAction.ReadValue<Vector2>().x;
		if (horizontalInput > threshold && !isPressedRight)
		{
			// right pressed
			isPressedRight = true;
			currentSelection = Mathf.Min(currentSelection + 1, standees.Count - 1);
			UpdateName();
		}
		else if (horizontalInput <= threshold && isPressedRight)
		{
			// right released
			isPressedRight = false;
		}
		if (horizontalInput < -threshold && !isPressedLeft)
		{
			// left pressed
			isPressedLeft = true;
			currentSelection = Mathf.Max(currentSelection - 1, 0);
			UpdateName();
		}
		else if (horizontalInput >= -threshold && isPressedLeft)
		{
			// left released
			isPressedLeft = false;
		}

		// standee positioning
		for (int i = 0; i < standees.Count; ++i)
		{
			Transform standee = standees[i];
			standee.localScale = Vector3.Lerp(
				standee.localScale, GetTargetedScale(i),
				1f - Mathf.Exp(-lerpScaleSpeed * Time.deltaTime));

			standee.localPosition = Vector3.Lerp(
				standee.localPosition, GetTargetedPosition(i),
				1f - Mathf.Exp(-lerpMoveSpeed * Time.deltaTime));
		}
	}

	Vector3 GetTargetedScale(int i)
	{
		return currentSelection == i ? new Vector3(selectedStandeeScale.x, selectedStandeeScale.y, 1) : new Vector3(standeeScale.x, standeeScale.y, 1);
	}

	Vector3 GetTargetedPosition(int i)
	{
		return new Vector3((i - currentSelection) * spacingApart.x, Mathf.Abs(i - currentSelection) * spacingApart.y, 1);
	}

	void UpdateName()
	{
		nameText.text = currentSelection == 0 ? playerName :
			SaveManager.Instance.CurrentSaveData.unlockedMonsters[currentSelection - 1] ? enemyList[currentSelection - 1].enemyName :
			"???";
	}


	public void SelectCharacter()
	{
		if (currentSelection <= 0 || SaveManager.Instance.CurrentSaveData.unlockedMonsters[currentSelection - 1])
		{
			SaveManager.Instance.CurrentMiscData.selectedCharacter = currentSelection;
			ChangeScene(gameplaySceneIndex);
		}
	}
}