using UnityEngine;
using UnityEngine.UI;

public class GameoverManager : GeneralManager
{
	[Header("Scene References")]
	[SerializeField] Text topText;
	[SerializeField] StatLine[] statLines;

	[Header("Bestiary List")]
	[SerializeField] PlayerAnimationSet playerSet;
	[SerializeField] EnemyStats[] enemyList;
	[SerializeField] PowerUpItem[] itemList;

	[Header("Prefabs")]
	[SerializeField] MobAnimation standeePrefab;
	[SerializeField] ItemPrefab itemPrefab;

	[Header("Positioning")]
	[SerializeField] Vector2 standeePosition;
	[SerializeField] Vector2 standeeScale = Vector2.one * 2;
	[SerializeField] float statLineTiming = 0.5f;

	[Header("Strings")]
	[SerializeField] string topTextWin = "You Win!!";
	[SerializeField] string topTextLose = "Game Over";

	[Header("Audio")]
	[SerializeField] AudioClip starSfx;

	float timer = 0;
	int statIndex = 0;

	protected override void Start()
	{
		base.Start();

		// disable statlines
		foreach (StatLine stat in statLines) stat.gameObject.SetActive(false);

		// testing
		//SaveManager.Instance.CurrentMiscData.win = true;
		//SaveManager.Instance.CurrentMiscData.currentPlayCharacter = 4;

		topText.text = SaveManager.Instance.CurrentMiscData.win ? topTextWin : topTextLose;

		// spawn standee
		MobAnimation standee = Instantiate(standeePrefab, standeePosition, Quaternion.identity);
		if (SaveManager.Instance.CurrentMiscData.currentPlayCharacter == 0)
		{
			// player
			standee.UpdateSpriteIndex(playerSet.idle, _animSpeed: playerSet.idleSpeed);
			standee.SetFlipX(Vector2.right * (playerSet.isFacingRight ? 1 : -1));
		}
		else
		{
			// enemy
			EnemyAnimationSet anims = enemyList[SaveManager.Instance.CurrentMiscData.currentPlayCharacter - 1].animationSet;
			standee.UpdateSpriteIndex(anims.idle, _animSpeed: anims.idleSpeed);
			standee.GetShadowRenderer().transform.localScale = new Vector3(anims.shadow.x, anims.shadow.y, 1);
			standee.SetFlipX(Vector2.right * (anims.isFacingRight ? 1 : -1));
		}
		standee.transform.localScale = new Vector2(standeeScale.x, standeeScale.y);
	}

	void Update()
	{
		if (statIndex < statLines.Length)
		{
			timer += Time.deltaTime;
			if (timer >= statLineTiming)
			{
				statLines[statIndex].gameObject.SetActive(true);
				PlaySFX(starSfx);

				timer -= statLineTiming;
				++statIndex;
			}
		}
	}
}
