using UnityEngine;
using UnityEngine.InputSystem;

public static class KeybindLoader 
{
	public static PlayerInputActions GetNewInputActions()
    {
		PlayerInputActions inputActions = new PlayerInputActions();

		// load overrides
		string rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
		if (!string.IsNullOrEmpty(rebinds)) inputActions.LoadBindingOverridesFromJson(rebinds);

		return inputActions;
	}
}
