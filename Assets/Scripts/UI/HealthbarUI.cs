using System;
using UnityEngine;

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

	[SerializeField] FillBar[] fillBars;

	void Awake()
	{
		foreach (FillBar bar in fillBars)
			bar.SetVisualHealth(health);
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

	public void SetHealth(int _health)
	{
		health = Mathf.Clamp(_health, 0, maxHealth);

		foreach (FillBar bar in fillBars)
			bar.SetShouldLerpHealth(true);
	}

	void OnValidate()
	{
		foreach (FillBar bar in fillBars)
		{
			bar.SetVisualHealth(health);
			bar.UpdateMaskPosition(maxHealth);
		}
	}
}