using UnityEngine;
using UnityEngine.InputSystem;
using System;

public static class KeybindLoader 
{
	public static PlayerInputActions GetNewInputActions(Action onChangeBindingsCallback = null)
	{
		PlayerInputActions inputActions = new PlayerInputActions();
		UpdateBindingOverrides(inputActions);
		SaveManager.Instance.onChangeBindings.AddListener(() => UpdateBindingOverrides(inputActions, onChangeBindingsCallback));
		return inputActions;
	}

	public static void UpdateBindingOverrides(PlayerInputActions inputActions, Action onChangeBindingsCallback = null)
	{
		// load overrides
		string rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
		if (string.IsNullOrEmpty(rebinds))
			inputActions.RemoveAllBindingOverrides();
		else
			inputActions.LoadBindingOverridesFromJson(rebinds);

		onChangeBindingsCallback?.Invoke();
	}
}