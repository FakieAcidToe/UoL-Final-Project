using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarUI : MonoBehaviour
{
	[System.Serializable]
	class FillBar
	{
		[SerializeField] float lerpSpeedIncrease;
		[SerializeField] float lerpSpeedDecrease;

		float visualHealth = 0;
		bool shouldLerpHealth = false;

		[SerializeField] RectTransform fillBarMask;
		[SerializeField] RectTransform fillBar;

		public void SetVisualHealth(int _health)
		{
			visualHealth = _health;
		}

		public void SetShouldLerpHealth(bool _shouldLerp)
		{
			shouldLerpHealth = _shouldLerp;
		}

		public void Update(int _health, int _maxHealth)
		{
			if (shouldLerpHealth)
			{
				// lerp to current health
				if (Mathf.Abs(visualHealth - _health) < 0.1f)
				{
					visualHealth = _health;
					shouldLerpHealth = false;
					UpdateMaskPosition(_maxHealth);
				}
				else
				{
					visualHealth = Mathf.Lerp(visualHealth, _health, 1f - Mathf.Exp(-(visualHealth < _health ? lerpSpeedIncrease : lerpSpeedDecrease) * Time.deltaTime));
					UpdateMaskPosition(_maxHealth);
				}
			}
		}

		public void UpdateMaskPosition(int _maxHealth)
		{
			if (fillBarMask != null && fillBar != null)
			{
				float x = Mathf.Lerp(-fillBarMask.rect.width, 0, Mathf.Clamp(visualHealth / _maxHealth, 0, _maxHealth));
				fillBarMask.localPosition = new Vector3(x, 0, 0);
				fillBar.localPosition = new Vector3(-x, 0, 0);
			}
		}
	}

	[SerializeField, Min(0)] int health = 100;
	[SerializeField, Min(1)] int maxHealth = 100;

	public Image portraitImage;
	Sprite defaultSprite;

	[SerializeField] FillBar[] fillBars;
	Coroutine setHealthCoroutine;

	void Awake()
	{
		UpdateMaskPosition();
		if (portraitImage != null)
			defaultSprite = portraitImage.sprite;
	}

	void Update()
	{
		foreach (FillBar bar in fillBars)
			bar.Update(health, maxHealth);
	}

	public void SetHealthRelative(int _healthAdd)
	{
		SetHealth(health + _healthAdd);
	}

	public void SetHealth(int _health, bool _shouldLerp = true)
	{
		health = Mathf.Clamp(_health, 0, maxHealth);

		if (_shouldLerp)
			foreach (FillBar bar in fillBars)
				bar.SetShouldLerpHealth(true);
		else
			UpdateMaskPosition();
	}

	public void SetMaxHealth(int _maxHealth, bool _shouldLerp = true)
	{
		maxHealth = _maxHealth;
		health = Mathf.Clamp(health, 0, maxHealth);

		if (_shouldLerp)
			foreach (FillBar bar in fillBars)
				bar.SetShouldLerpHealth(true);
		else
			UpdateMaskPosition();
	}

	void UpdateMaskPosition()
	{
		foreach (FillBar bar in fillBars)
		{
			bar.SetVisualHealth(health);
			bar.UpdateMaskPosition(maxHealth);
		}
	}

	public void SetPortrait(Sprite newSprite = null)
	{
		if (portraitImage != null)
			portraitImage.sprite = newSprite == null ? defaultSprite : newSprite;
	}

	void OnValidate()
	{
		UpdateMaskPosition();
	}

	public void SetHealth(int _health, float _timeTaken, bool _shouldLerp = true, Action _onCompleteCallback = null)
	{
		if (setHealthCoroutine != null) StopCoroutine(setHealthCoroutine);
			setHealthCoroutine = StartCoroutine(SetHealthCoroutine(_health, _timeTaken, _shouldLerp, _onCompleteCallback));
	}

	IEnumerator SetHealthCoroutine(int _health, float _timeTaken, bool _shouldLerp = true, Action _onCompleteCallback = null)
	{
		yield return new WaitForSeconds(_timeTaken);
		SetHealth(_health, _shouldLerp);

		setHealthCoroutine = null;
		if (_onCompleteCallback != null) _onCompleteCallback();
	}
}