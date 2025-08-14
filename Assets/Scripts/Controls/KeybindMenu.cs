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

	[Header("Attack Key")] // left mouse, space
	[SerializeField] Button attackKeyButton;
	Text attackKeyText;
	[SerializeField] Button attackKeyButton2;
	Text attackKeyText2;

	[Header("Eject Key")] // Q, E
	[SerializeField] Button ejectKeyButton;
	Text ejectKeyText;
	[SerializeField] Button ejectKeyButton2;
	Text ejectKeyText2;

	[Header("Camera Drag Key")] // right mouse, middle mouse
	[SerializeField] Button dragKeyButton;
	Text dragKeyText;
	[SerializeField] Button dragKeyButton2;
	Text dragKeyText2;

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
			Rebind(inputActions.Gameplay.Attack, attackKeyText, 0);
		});
		attackKeyText2 = attackKeyButton2.GetComponentInChildren<Text>();
		attackKeyButton2.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.Attack, attackKeyText2, 1);
		});

		// eject
		ejectKeyText = ejectKeyButton.GetComponentInChildren<Text>();
		ejectKeyButton.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.Eject, ejectKeyText, 0);
		});
		ejectKeyText2 = ejectKeyButton2.GetComponentInChildren<Text>();
		ejectKeyButton2.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.Eject, ejectKeyText2, 1);
		});

		// camera drag
		dragKeyText = dragKeyButton.GetComponentInChildren<Text>();
		dragKeyButton.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.DragMap, dragKeyText, 0);
		});
		dragKeyText2 = dragKeyButton2.GetComponentInChildren<Text>();
		dragKeyButton2.onClick.AddListener(() =>
		{
			Rebind(inputActions.Gameplay.DragMap, dragKeyText2, 1);
		});

		UpdateKeybindUI();
	}

	void UpdateKeybindUI()
	{
		upKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(inputActions.Gameplay.Move, "up"));
		downKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(inputActions.Gameplay.Move, "down"));
		leftKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(inputActions.Gameplay.Move, "left"));
		rightKeyText.text = inputActions.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(inputActions.Gameplay.Move, "right"));

		attackKeyText.text = inputActions.Gameplay.Attack.GetBindingDisplayString(0);
		attackKeyText2.text = inputActions.Gameplay.Attack.GetBindingDisplayString(1);
		ejectKeyText.text = inputActions.Gameplay.Eject.GetBindingDisplayString(0);
		ejectKeyText2.text = inputActions.Gameplay.Eject.GetBindingDisplayString(1);
		dragKeyText.text = inputActions.Gameplay.DragMap.GetBindingDisplayString(0);
		dragKeyText2.text = inputActions.Gameplay.DragMap.GetBindingDisplayString(1);
	}

	void Rebind(InputAction action, Text displayText, int bindingIndex = -1)
	{
		displayText.text = "Press a key...";
		action.Disable();

		if (rebindingOperation != null) rebindingOperation.Dispose();
		rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
			//.WithControlsExcluding("Mouse")
			//.WithCancelingThrough("<Keyboard>/escape")
			.OnPotentialMatch(operation => { // https://discussions.unity.com/t/withcancelingthrough-keyboard-escape-also-cancels-with-keyboard-e/870292/16
				if (operation.selectedControl.path is "/Keyboard/escape")
				{
					operation.Cancel();
					return;
				}
			})
			.WithCancelingThrough("an enormous string of absolute gibberish which overrides the default which is escape and causes the above bug")
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

	public void ResetSettings()
	{
		SaveManager.Instance.ResetSettings();
	}

	public void DeleteSaveData()
	{
		SaveManager.Instance.ResetData();
		ResetBindings();
	}

	public void PlaySFX(AudioClip _clip)
	{
		SoundManager.Instance.Play(_clip);
	}
}