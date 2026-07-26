using UnityEngine;

public class ControlPanel : MonoBehaviour
{
	public EmissiveLight CircleButtonLight;
	public HoverTarget CircleButton;
	public EmissiveLight ArrowButtonLight;
	public HoverTarget ArrowButton;

	private bool _circleButtonEnabled = true;
	private bool _arrowButtonEnabled = true;

	private void OnEnable()
	{
		ArrowButton.OnClick += HandleArrowClick;
		CircleButton.OnClick += HandleCircleClick;
		GameManager.Instance.OnNpcsArrived += HandleNpcsArrived;
		GameManager.Instance.OnStartDoorClose += HandleStartDoorClose;

		SetCircleButtonEnabled(false);
		SetArrowButtonEnabled(false);
	}

	private void OnDisable()
	{
		if (ArrowButton != null)
		{
			ArrowButton.OnClick -= HandleArrowClick;
		}
		if (CircleButton != null)
		{
			CircleButton.OnClick -= HandleCircleClick;
		}
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnNpcsArrived -= HandleNpcsArrived;
			GameManager.Instance.OnStartDoorClose -= HandleStartDoorClose;
		}
	}

	private void HandleStartDoorClose()
	{
		SetCircleButtonEnabled(false);
		SetArrowButtonEnabled(false);
	}

	private void HandleCircleClick()
	{
		if (_circleButtonEnabled)
		{
			GameManager.Instance.AcceptNpcs();
			SetCircleButtonEnabled(false);
			SetArrowButtonEnabled(false);
		}
	}

	private void HandleArrowClick()
	{
		if (_arrowButtonEnabled)
		{
			GameManager.Instance.ContinueToNextFloor();
			SetCircleButtonEnabled(false);
			SetArrowButtonEnabled(false);
		}
	}

	private void HandleNpcsArrived()
	{
		if (GameManager.Instance.DescendButtonPressed)
		{
			return;
		}

		if (!GameManager.Instance.OpenedDoor)
		{
			SetCircleButtonEnabled(true);
		}
		SetArrowButtonEnabled(true);
	}

	private void SetArrowButtonEnabled(bool enabled)
	{
		_arrowButtonEnabled = enabled;
		ArrowButtonLight.Brightness = enabled ? 1f : 0f;
	}

	private void SetCircleButtonEnabled(bool enabled)
	{
		_circleButtonEnabled = enabled;
		CircleButtonLight.Brightness = enabled ? 1f : 0f;
	}
}
