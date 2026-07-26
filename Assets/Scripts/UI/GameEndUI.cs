using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class GameEndUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private CanvasGroup _loseCanvasGroup;

	[SerializeField]
	private Image _loseImage;

	[SerializeField]
	private CanvasGroup _winCanvasGroup;

	[Header("Tween Settings")]
	[SerializeField]
	private float _tweenDuration = 1.5f;

	[Header("Color Fade Settings")]
	[SerializeField]
	private Color _startColor = Color.clear;

	[SerializeField]
	private Color _intermediateColor = Color.red;

	[SerializeField]
	private Color _finalColor = Color.black;

	[SerializeField]
	private float _fadeToIntermediateDuration = 0.75f;

	[SerializeField]
	private float _fadeToFinalDuration = 0.75f;

	private Tween _tween;
	private Sequence _colorSequence;

	private void OnEnable()
	{
		GameManager.Instance.OnGameLose += ShowLoseUI;
		GameManager.Instance.OnGameWin += ShowWinUI;
	}

	private void OnDisable()
	{
		_tween.Stop();
		_colorSequence.Stop();
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

		_colorSequence.Stop();
		_loseImage.color = _startColor;
		_colorSequence = Sequence
			.Create()
			.Chain(Tween.Color(_loseImage, _intermediateColor, _fadeToIntermediateDuration))
			.Chain(Tween.Color(_loseImage, _finalColor, _fadeToFinalDuration));
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
