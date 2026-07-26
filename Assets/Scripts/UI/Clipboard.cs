using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

// fills in clipboard data

public class Clipboard : MonoBehaviour
{
	public RectTransform OwnTransform;
	public HoverTarget HoverTarget;
	public SwayingUI SwayingUI;

	public TMP_Text WorkersText;
	public TMP_Text PsychologistsText;
	public TMP_Text GuardsText;

	[Header("Options")]
	public Vector2 DefaultPosition;
	public Vector2 HiddenPosition;
	public Vector2 OutPosition;

	public float MoveSharpness = 4f;
	public float HideDuration;
	public float HideReturnDuration;

	private Sequence _sequence;

	private void OnEnable()
	{
		GameManager.Instance.OnNpcUpdate += UpdateInfo;
		GameManager.Instance.OnStartDescent += StartSway;
		GameManager.Instance.OnNewFloor += StopSway;
		
		HoverTarget.OnHover += HandleHover;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnNpcUpdate -= UpdateInfo;
			GameManager.Instance.OnStartDescent -= StartSway;
			GameManager.Instance.OnNewFloor -= StopSway;

			_sequence.Stop();
		}

		HoverTarget.OnHover -= HandleHover;
	}

	private void HandleHover()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Clipboard_Sfx);
	}

	private void StartSway() => SwayingUI.StartTween();

	private void StopSway() => SwayingUI.TweenBackToDefault();

	private void Update()
	{
		// if not doing the number-update animation, exp lerp to the right position.
		if (!_sequence.isAlive)
		{
			Vector2 targetPos = HoverTarget.Hovered ? OutPosition : DefaultPosition;
			var nextPos = Vector2.Lerp(
				targetPos,
				OwnTransform.anchoredPosition,
				Mathf.Exp(-MoveSharpness * Time.deltaTime)
			);
			OwnTransform.anchoredPosition = nextPos;
		}
	}

	private void UpdateInfo()
	{
		PlayNumberUpdateAnim();
	}

	private void PlayNumberUpdateAnim()
	{
		RectTransform self = GetComponent<RectTransform>();

		_sequence = Sequence
			.Create()
			.Chain(Tween.UIAnchoredPosition(OwnTransform, endValue: HiddenPosition, HideDuration))
			.ChainCallback(SetNumbers)
			.Chain(Tween.UIAnchoredPosition(OwnTransform, endValue: DefaultPosition, HideReturnDuration));
	}

	private void SetNumbers()
	{
		WorkersText.text = GameManager.Instance.CountNPCs(NpcRoles.Worker).ToString("D2");
		PsychologistsText.text = GameManager.Instance.CountNPCs(NpcRoles.Psychologist).ToString("D2");
		GuardsText.text = GameManager.Instance.CountNPCs(NpcRoles.Guard).ToString("D2");
	}
}
