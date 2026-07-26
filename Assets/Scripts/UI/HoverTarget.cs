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
        if (OutlineOnHover)
        {
            OutlineTarget.layer = LayerMask.NameToLayer("Outlined");
        }
        if (FadeCanvasOnHover)
        {
            _opacityTween.Stop();
            _opacityTween = Tween.Alpha(CanvasGroup, ShownOpacity, FadeDuration);
        }
        OnHover?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
        _hovered = false;
        if (OutlineOnHover)
        {
            OutlineTarget.layer = LayerMask.NameToLayer("Default");
        }
        if (FadeCanvasOnHover)
        {
            _opacityTween.Stop();
            _opacityTween = Tween.Alpha(CanvasGroup, 0f, FadeDuration);
        }
        OnUnhover?.Invoke();
	}
}