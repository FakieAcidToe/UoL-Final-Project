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
	
	public enum SettingType
	{
		musicVolume,
		sfxVolume
	}

	void Start()
	{
		switch (settingType)
		{
			case SettingType.musicVolume:
				value = SaveManager.Instance.CurrentSaveData.musicVolume;
				break;
			case SettingType.sfxVolume:
				value = SaveManager.Instance.CurrentSaveData.sfxVolume;
				break;
		}

		if (slider != null)
			slider.value = value;
		if (input != null)
			input.text = value.ToString();
	}

	void SetValue(float newValue)
	{
		value = newValue;

		switch (settingType)
		{
			case SettingType.musicVolume:
				SaveManager.Instance.CurrentSaveData.musicVolume = value;
				break;
			case SettingType.sfxVolume:
				SaveManager.Instance.CurrentSaveData.sfxVolume = value;
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
}
