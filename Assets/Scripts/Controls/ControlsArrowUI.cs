using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsArrowUI : MonoBehaviour
{
	[SerializeField] GameObject upArrow;
	[SerializeField] GameObject downArrow;
	[SerializeField] GameObject leftArrow;
	[SerializeField] GameObject rightArrow;

	[SerializeField] bool shouldDeleteWhenPressed = true;

	TextMesh upText;
	TextMesh downText;
	TextMesh leftText;
	TextMesh rightText;

	PlayerInputActions controls;
	const float threshold = 0.5f;

	void Awake()
	{
		upText = upArrow.GetComponentInChildren<TextMesh>();
		downText = downArrow.GetComponentInChildren<TextMesh>();
		leftText = leftArrow.GetComponentInChildren<TextMesh>();
		rightText = rightArrow.GetComponentInChildren<TextMesh>();

		controls = KeybindLoader.GetNewInputActions(UpdateKeybindUI);
		UpdateKeybindUI();
	}

	void OnEnable()
	{
		controls.Gameplay.Enable();
	}

	void OnDisable()
	{
		controls.Gameplay.Disable();
		if (shouldDeleteWhenPressed)
			gameObject.SetActive(false);
	}

	void Update()
	{
		if (shouldDeleteWhenPressed)
		{
			Vector2 input = controls.Gameplay.Move.ReadValue<Vector2>();
			if (input.x > threshold && rightArrow.gameObject.activeSelf)
			{
				rightArrow.gameObject.SetActive(false);
				DisableSelfIfAllDisabled();
			}
			else if (input.x < -threshold && leftArrow.gameObject.activeSelf)
			{
				leftArrow.gameObject.SetActive(false);
				DisableSelfIfAllDisabled();
			}
			if (input.y > threshold && upArrow.gameObject.activeSelf)
			{
				upArrow.gameObject.SetActive(false);
				DisableSelfIfAllDisabled();
			}
			else if (input.y < -threshold && downArrow.gameObject.activeSelf)
			{
				downArrow.gameObject.SetActive(false);
				DisableSelfIfAllDisabled();
			}
		}
	}

	void DisableSelfIfAllDisabled()
	{
		if (!rightArrow.gameObject.activeSelf && !leftArrow.gameObject.activeSelf && !upArrow.gameObject.activeSelf && !downArrow.gameObject.activeSelf)
			gameObject.SetActive(false);
	}

	void UpdateKeybindUI()
	{
		if (upText != null && downText != null && leftText != null && rightText != null)
		{
			upText.text = controls.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(controls.Gameplay.Move, "up"));
			downText.text = controls.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(controls.Gameplay.Move, "down"));
			leftText.text = controls.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(controls.Gameplay.Move, "left"));
			rightText.text = controls.Gameplay.Move.GetBindingDisplayString(GetBindingIndex(controls.Gameplay.Move, "right"));
		}
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
}