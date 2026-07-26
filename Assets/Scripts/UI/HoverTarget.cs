using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public bool OutlineOnHover = false;
	public GameObject OutlineTarget;

	public bool FadeCanvasOnHover = false;
	public CanvasGroup CanvasGroup;
	public float ShownOpacity = 0.4f;
	public float FadeDuration = 0.3f;

    private bool _forceHovered = false;
	private bool _hovered = false;
	private Tween _opacityTween;

	public bool Hovered => _hovered;
	public event Action OnHover;
	public event Action OnUnhover;
	public event Action OnClick;

	public void Start()
	{
		if (FadeCanvasOnHover)
		{
			CanvasGroup.alpha = 0f;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OnClick?.Invoke();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_hovered = true;
		UpdateHoverVisuals();
		OnHover?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_hovered = false;
		UpdateHoverVisuals();
		OnUnhover?.Invoke();
	}

    public void SetForceHovered(bool val)
    {
        _forceHovered = val;
        UpdateHoverVisuals();
    }

	private void UpdateHoverVisuals()
	{
		if (_hovered || _forceHovered)
		{
			if (OutlineOnHover)
			{
				OutlineTarget.layer = LayerMask.NameToLayer("Outlined");
			}
			if (FadeCanvasOnHover)
			{
				_opacityTween.Stop();
				_opacityTween = Tween.Alpha(CanvasGroup, ShownOpacity, FadeDuration);
			}
		}
		else
		{
			if (OutlineOnHover)
			{
				OutlineTarget.layer = LayerMask.NameToLayer("Default");
			}
			if (FadeCanvasOnHover)
			{
				_opacityTween.Stop();
				_opacityTween = Tween.Alpha(CanvasGroup, 0f, FadeDuration);
			}
		}
	}
}
