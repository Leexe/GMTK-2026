using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class EngineHealthDisplay : MonoBehaviour
{
	public List<EmissiveLight> Lights;
	public CanvasGroup CanvasGroup;
	public HoverTarget HoverTarget;

	[Header("Options")]
	public float FadeDuration = 0.3f;
	public float ShownAlpha = 0.4f;

	private Tween _alphaTween;

	private void OnEnable()
	{
		GameManager.Instance.OnEngineUpdate += HandleEngineUpdate;
		HoverTarget.OnHover += OnHover;
		HoverTarget.OnUnhover += OnUnhover;

		CanvasGroup.alpha = 0f;
		HandleEngineUpdate();
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnEngineUpdate -= HandleEngineUpdate;
		}
		if (HoverTarget != null)
		{
			HoverTarget.OnHover -= OnHover;
			HoverTarget.OnUnhover -= OnUnhover;
		}
	}

	private void OnHover()
	{
		_alphaTween.Stop();
		_alphaTween = Tween.Alpha(CanvasGroup, ShownAlpha, FadeDuration);
	}

	private void OnUnhover()
	{
		_alphaTween.Stop();
		_alphaTween = Tween.Alpha(CanvasGroup, 0f, FadeDuration);
	}

	private void HandleEngineUpdate()
	{
		UpdateLights(GameManager.Instance.EngineIntegrityNormalized);
	}

	private void UpdateLights(float normalizedHealth)
	{
		var baseColor = Color.HSVToRGB(0.32f * normalizedHealth, 1f, 0.5f);
		for (int i = 0; i < Lights.Count; i++)
		{
			Lights[i].BaseColor = baseColor;
			Lights[i].Brightness = Mathf.Clamp01((normalizedHealth * Lights.Count) - i);
		}
	}
}
