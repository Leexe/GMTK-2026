using System;
using PrimeTween;
using UnityEngine;

public class GameEndUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private CanvasGroup _loseCanvasGroup;

	[SerializeField]
	private CanvasGroup _winCanvasGroup;

	[Header("Tween Settings")]
	[SerializeField]
	private float _tweenDuration = 1.5f;

	private Tween _tween;

	private void OnEnable()
	{
		GameManager.Instance.OnGameLose += ShowLoseUI;
		GameManager.Instance.OnGameWin += ShowWinUI;
	}

	private void OnDisable()
	{
		_tween.Stop();
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGameLose -= ShowLoseUI;
			GameManager.Instance.OnGameWin -= ShowWinUI;
		}
	}

	private void ShowLoseUI()
	{
		_loseCanvasGroup.blocksRaycasts = true;
		_loseCanvasGroup.interactable = true;
		_tween = Tween.Alpha(_loseCanvasGroup, 0, 1, _tweenDuration);
	}

	private void ShowWinUI()
	{
		_winCanvasGroup.blocksRaycasts = true;
		_winCanvasGroup.interactable = true;
		_tween = Tween.Alpha(_winCanvasGroup, 0, 1, _tweenDuration);
	}

	public void OnRetryButton()
	{
		GameManager.Instance.RestartGame();
	}
}
