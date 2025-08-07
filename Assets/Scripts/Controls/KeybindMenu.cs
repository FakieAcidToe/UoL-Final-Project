using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeybindMenu : MonoBehaviour
{
	PlayerInputActions inputActions;
	InputActionRebindingExtensions.RebindingOperation rebindingOperation;

	[Header("Movement Keys")] // WASD
	[SerializeField] Text upKeyText;
	[SerializeField] Button upKeyButton;
	[SerializeField] Text downKeyText;
	[SerializeField] Button downKeyButton;
	[SerializeField] Text leftKeyText;
	[SerializeField] Button leftKeyButton;
	[SerializeField] Text rightKeyText;
	[SerializeField] Button rightKeyButton;

	[Header("Attack Key")] // left mouse, spacebar
	[SerializeField] Text attackKeyText;
	[SerializeField] Button attackKeyButton;

	[Header("Eject Key")] // Q, E, middle mouse
	[SerializeField] Text ejectKeyText;
	[SerializeField] Button ejectKeyButton;

	[Header("Camera Drag Key")] // right mouse
	[SerializeField] Text dragKeyText;
	[SerializeField] Button dragKeyButton;

	void Start()
	{
		inputActions = new PlayerInputActions();
		inputActions.Enable();

		UpdateKeybindUI();

		// wasd
		upKeyButton.onClick.AddListener(() =>
		{
			RebindCompositePart(inputActions.Gameplay.Move, "up", upKeyText);
		});
		downKeyButton.onClick.AddListener(() =>
		{
			RebindCompositePart(inputActions.Gameplay.Move, "down", downKeyText);
		});
		leftKeyButton.onClick.AddListener(() =>
		{
			RebindCompositePart(inputActions.Gameplay.Move, "left", leftKeyText);
		});
		rightKeyButton.onClick.AddListener(() =>
		{
			RebindCompositePart(inputActions.Gameplay.Move, "right", rightKeyText);
		});

		// attack
		attackKeyButton.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.Attack, attackKeyText);
		});

		// eject
		ejectKeyButton.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.Attack, ejectKeyText);
		});

		// camera drag
		dragKeyButton.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.Attack, dragKeyText);
		});
	}

	void UpdateKeybindUI()
	{
		upKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex("up"));
		downKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex("down"));
		leftKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex("left"));
		rightKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex("right"));

		attackKeyText.text = inputActions.Gameplay.Attack.GetBindingDisplayString();
		ejectKeyText.text = inputActions.Gameplay.Eject.GetBindingDisplayString();
		dragKeyText.text = inputActions.Gameplay.DragMap.GetBindingDisplayString();
	}

	void Rebind(InputAction action, Text displayText, int bindingIndex = -1)
	{
		displayText.text = "Press a key...";
		action.Disable();

		action.PerformInteractiveRebinding(bindingIndex)
			//.WithControlsExcluding("Mouse")
			.OnMatchWaitForAnother(0.1f)
			.OnComplete(operation =>
			{
				action.Enable();
				operation.Dispose();
				displayText.text = action.GetBindingDisplayString();
				SaveBindings();
			})
			.Start();
	}

	void RebindCompositePart(InputAction action, string partName, Text displayText)
	{
		int bindingIndex = GetBindingIndex(partName);
		if (bindingIndex == -1)
		{
			Debug.LogWarning($"Could not find composite part '{partName}'");
			return;
		}

		Rebind(action, displayText, bindingIndex);
	}

	int GetBindingIndex(string compositePart)
	{
		for (int i = 0; i < inputActions.Gameplay.Move.bindings.Count; ++i)
		{
			if (inputActions.Gameplay.Move.bindings[i].isPartOfComposite &&
				inputActions.Gameplay.Move.bindings[i].name == compositePart)
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