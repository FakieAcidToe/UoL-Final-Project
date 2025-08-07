using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeybindMenu : MonoBehaviour
{
	PlayerInputActions inputActions;
	InputActionRebindingExtensions.RebindingOperation rebindingOperation;

	[Header("Movement Keys")] // WASD
	[SerializeField] Button upKeyButton;
	Text upKeyText;
	[SerializeField] Button downKeyButton;
	Text downKeyText;
	[SerializeField] Button leftKeyButton;
	Text leftKeyText;
	[SerializeField] Button rightKeyButton;
	Text rightKeyText;

	[Header("Attack Key")] // left mouse
	[SerializeField] Button attackKeyButton;
	Text attackKeyText;

	[Header("Eject Key")] // Q
	[SerializeField] Button ejectKeyButton;
	Text ejectKeyText;

	[Header("Camera Drag Key")] // right mouse
	[SerializeField] Button dragKeyButton;
	Text dragKeyText;

	[Header("Reset Key")]
	[SerializeField] Button resetButton;

	void Awake()
	{
		inputActions = KeybindLoader.GetNewInputActions();

		// wasd
		upKeyText = upKeyButton.GetComponentInChildren<Text>();
		upKeyButton.onClick.AddListener(() =>
		{
			RebindCompositePart(inputActions.Gameplay.Move, "up", upKeyText);
		});
		downKeyText = downKeyButton.GetComponentInChildren<Text>();
		downKeyButton.onClick.AddListener(() =>
		{
			RebindCompositePart(inputActions.Gameplay.Move, "down", downKeyText);
		});
		leftKeyText = leftKeyButton.GetComponentInChildren<Text>();
		leftKeyButton.onClick.AddListener(() =>
		{
			RebindCompositePart(inputActions.Gameplay.Move, "left", leftKeyText);
		});
		rightKeyText = rightKeyButton.GetComponentInChildren<Text>();
		rightKeyButton.onClick.AddListener(() =>
		{
			RebindCompositePart(inputActions.Gameplay.Move, "right", rightKeyText);
		});

		// attack
		attackKeyText = attackKeyButton.GetComponentInChildren<Text>();
		attackKeyButton.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.Attack, attackKeyText);
		});

		// eject
		ejectKeyText = ejectKeyButton.GetComponentInChildren<Text>();
		ejectKeyButton.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.Eject, ejectKeyText);
		});

		// camera drag
		dragKeyText = dragKeyButton.GetComponentInChildren<Text>();
		dragKeyButton.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.DragMap, dragKeyText);
		});

		// reset
		resetButton.onClick.AddListener(() =>
		{
			ResetBindings();
		});

		UpdateKeybindUI();
	}

	void UpdateKeybindUI()
	{
		upKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(inputActions.Gameplay.Move, "up"));
		downKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(inputActions.Gameplay.Move, "down"));
		leftKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(inputActions.Gameplay.Move, "left"));
		rightKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(inputActions.Gameplay.Move, "right"));

		attackKeyText.text = inputActions.Gameplay.Attack.GetBindingDisplayString();
		ejectKeyText.text = inputActions.Gameplay.Eject.GetBindingDisplayString();
		dragKeyText.text = inputActions.Gameplay.DragMap.GetBindingDisplayString();
	}

	void Rebind(InputAction action, Text displayText, int bindingIndex = -1)
	{
		displayText.text = "Press a key...";
		action.Disable();

		if (rebindingOperation != null) rebindingOperation.Dispose();
		rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
			//.WithControlsExcluding("Mouse")
			.WithCancelingThrough("<Keyboard>/escape")
			.OnMatchWaitForAnother(0.1f)
			.OnCancel(operation =>
			{
				action.Enable();
				operation.Dispose();
				rebindingOperation = null;
				displayText.text = bindingIndex == -1 ? action.GetBindingDisplayString() : action.GetBindingDisplayString(bindingIndex);
			})
			.OnComplete(operation =>
			{
				action.Enable();
				operation.Dispose();
				rebindingOperation = null;
				displayText.text = bindingIndex == -1 ? action.GetBindingDisplayString() : action.GetBindingDisplayString(bindingIndex);
				SaveBindings();
			})
			.Start();
	}

	void RebindCompositePart(InputAction action, string partName, Text displayText)
	{
		int bindingIndex = GetBindingIndex(action, partName);
		if (bindingIndex == -1)
		{
			Debug.LogWarning($"Could not find composite part '{partName}'");
			return;
		}

		Rebind(action, displayText, bindingIndex);
	}

	int GetBindingIndex(InputAction action, string compositePart)
	{
		for (int i = 0; i < action.bindings.Count; ++i)
		{
			if (action.bindings[i].isPartOfComposite &&
				action.bindings[i].name == compositePart)
				return i;
		}
		return -1;
	}

	void SaveBindings()
	{
		string rebinds = inputActions.SaveBindingOverridesAsJson();
		PlayerPrefs.SetString("rebinds", rebinds);
	}

	void OnEnable()
	{
		inputActions.Enable();
		var rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
		if (!string.IsNullOrEmpty(rebinds))
			inputActions.LoadBindingOverridesFromJson(rebinds);
	}

	void OnDisable()
	{
		inputActions.Disable();
	}

	public void ResetBindings()
	{
		inputActions.RemoveAllBindingOverrides();
		PlayerPrefs.DeleteKey("rebinds");
		UpdateKeybindUI();
	}
}