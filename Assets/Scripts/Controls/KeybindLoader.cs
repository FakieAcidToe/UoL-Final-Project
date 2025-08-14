using UnityEngine;
using UnityEngine.InputSystem;

public static class KeybindLoader 
{
	public static PlayerInputActions GetNewInputActions()
	{
		PlayerInputActions inputActions = new PlayerInputActions();
		UpdateBindingOverrides(inputActions);

		return inputActions;
	}

	public static void UpdateBindingOverrides(PlayerInputActions inputActions)
	{
		// load overrides
		string rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
		if (!string.IsNullOrEmpty(rebinds)) inputActions.LoadBindingOverridesFromJson(rebinds);
	}
}
