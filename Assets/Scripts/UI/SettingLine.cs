using UnityEngine;
using UnityEngine.UI;

public class SettingLine : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Text text;
	[SerializeField] Slider slider;
	[SerializeField] InputField input;

	[Header("Properties")]
	[SerializeField] string settingName = "Music Volume";
	[SerializeField] SettingType settingType = SettingType.musicVolume;
	[SerializeField] float value = 100;
	[SerializeField] float valueMin = 0;
	[SerializeField] float valueMax = 100;
	[SerializeField] bool shouldFloor = false;

	public enum SettingType
	{
		musicVolume,
		sfxVolume,
		feedbackDuration,
		screenshake,
		damageInflation,
		difficulty
	}

	void Start()
	{
		UpdateSliderInputValues();
	}

	void SetValue(float newValue)
	{
		value = newValue;
		if (shouldFloor)
			value = Mathf.FloorToInt(value);

		switch (settingType)
		{
			case SettingType.musicVolume:
				SaveManager.Instance.CurrentSaveData.musicVolume = value;
				SoundManager.Instance.SetMusicVolume(value);
				break;
			case SettingType.sfxVolume:
				SaveManager.Instance.CurrentSaveData.sfxVolume = value;
				SoundManager.Instance.SetSFXVolume(value);
				break;
			case SettingType.feedbackDuration:
				SaveManager.Instance.CurrentSaveData.feedbackDuration = value;
				break;
			case SettingType.screenshake:
				SaveManager.Instance.CurrentSaveData.screenshake = value;
				break;
			case SettingType.damageInflation:
				SaveManager.Instance.CurrentSaveData.damageInflation = value;
				break;
			case SettingType.difficulty:
				SaveManager.Instance.CurrentMiscData.difficulty = (int)value;
				SetDifficultyText();
				break;
		}
	}

	void OnEnable()
	{
		slider.onValueChanged.AddListener(SliderChanged);
		input.onValueChanged.AddListener(InputChanged);
	}

	void OnDisable()
	{
		slider.onValueChanged.RemoveListener(SliderChanged);
		input.onValueChanged.RemoveListener(InputChanged);
	}

	void SliderChanged(float arg0)
	{
		SetValue(arg0);

		if (input != null)
			input.text = value.ToString();
	}

	void InputChanged(string arg0)
	{
		float floatValue;
		if (float.TryParse(arg0, out floatValue))
		{
			SetValue(floatValue);

			if (slider != null)
				slider.value = value;
		}
		else
		{
			if (input != null)
				input.text = value.ToString();
		}
	}

	void OnValidate()
	{
		if (valueMin > valueMax) valueMin = valueMax;
		value = Mathf.Clamp(value, valueMin, valueMax);

		if (shouldFloor) value = Mathf.FloorToInt(value);

		if (text != null)
			text.text = settingName;

		if (slider != null)
		{
			slider.minValue = valueMin;
			slider.maxValue = valueMax;
			slider.value = value;
		}

		if (input != null)
			input.text = value.ToString();
	}

	public void UpdateSliderInputValues()
	{
		switch (settingType)
		{
			case SettingType.musicVolume:
				value = SaveManager.Instance.CurrentSaveData.musicVolume;
				break;
			case SettingType.sfxVolume:
				value = SaveManager.Instance.CurrentSaveData.sfxVolume;
				break;
			case SettingType.feedbackDuration:
				value = SaveManager.Instance.CurrentSaveData.feedbackDuration;
				break;
			case SettingType.screenshake:
				value = SaveManager.Instance.CurrentSaveData.screenshake;
				break;
			case SettingType.damageInflation:
				value = SaveManager.Instance.CurrentSaveData.damageInflation;
				break;
			case SettingType.difficulty:
				value = SaveManager.Instance.CurrentMiscData.difficulty;
				SetDifficultyText();
				break;
		}

		if (slider != null)
			slider.value = value;
		if (input != null)
			input.text = value.ToString();
	}

	void SetDifficultyText()
	{
		string difficultyText;
		switch (SaveManager.Instance.CurrentMiscData.difficulty)
		{
			case 1: difficultyText = "Very Easy"; break;
			case 2: difficultyText = "Easy"; break;
			default:
			case 3: difficultyText = "Normal"; break;
			case 4: difficultyText = "Hard"; break;
			case 5: difficultyText = "Very Hard"; break;
		}
		text.text = settingName + " (" + difficultyText + ")";
	}
}