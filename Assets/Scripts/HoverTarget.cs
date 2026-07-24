using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private bool _hovered = false;

    public bool Hovered => _hovered;
    public event Action OnHover;
    public event Action OnUnhover;
    public event Action OnClick;

	public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }

	public void OnPointerEnter(PointerEventData eventData)
	{
        _hovered = true;
        OnHover?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
        _hovered = false;
        OnHover?.Invoke();
	}
}