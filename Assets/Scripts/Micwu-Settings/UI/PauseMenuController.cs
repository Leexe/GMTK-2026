using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
	public bool ControlTimescale = true;
	public bool ControlCursorLock = true;

	[Space]
	[SerializeField] private CanvasGroup _canvasGroup;
	[SerializeField] private Button _resumeButton;
	[SerializeField] private Button _exitButton;
	[SerializeField] private InputActionReference _menuAction;

	public event Action OnOpen;
	public event Action OnClose;

	private float prevTimescale;
	private bool prevCursorVisible;
	private CursorLockMode prevCursorLockMode;

	private void OnEnable()
	{
#if UNITY_WEBGL
		_exitButton.gameObject.SetActive(false);
#endif

		if (_menuAction == null)
		{
			Debug.LogError("Pause menu doesn't have an input action set");
			return;
		}

		_menuAction.action.actionMap.Enable();

		_menuAction.action.performed += OnMenuActionPerformed;
		_resumeButton.onClick.AddListener(ToggleMenu);
		_exitButton.onClick.AddListener(ExitToTitle);
	}

	private void OnDisable()
	{
		_menuAction.action.performed -= OnMenuActionPerformed;

		if (_resumeButton != null)
		{
			_resumeButton.onClick.RemoveListener(ToggleMenu);
		}
		if (_exitButton != null)
		{
			_exitButton.onClick.RemoveListener(ExitToTitle);
		}
	}

	private void OnMenuActionPerformed(InputAction.CallbackContext ctx)
	{
		ToggleMenu();
	}

	private void ToggleMenu()
	{
		bool wasOpen = _canvasGroup.alpha > 0.5f;
		SetOpen(!wasOpen);
	}

	public void SetOpen(bool open)
	{
		if (open)
		{
			_canvasGroup.alpha = 1f;
			_canvasGroup.interactable = true;
			_canvasGroup.blocksRaycasts = true;

			if (ControlCursorLock)
			{
				prevCursorLockMode = Cursor.lockState;
				prevCursorVisible = Cursor.visible;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			if (ControlTimescale)
			{
				prevTimescale = Time.timeScale;
				Time.timeScale = 0f;
			}
			OnOpen?.Invoke();
		}
		else
		{
			_canvasGroup.alpha = 0f;
			_canvasGroup.interactable = false;
			_canvasGroup.blocksRaycasts = false;
			if (ControlCursorLock)
			{
				Cursor.lockState = prevCursorLockMode;
				Cursor.visible = prevCursorVisible;
			}
			if (ControlTimescale) Time.timeScale = prevTimescale;
			OnClose?.Invoke();
		}
	}

	private void ExitToTitle()
	{
		Application.Quit();
	}
}
