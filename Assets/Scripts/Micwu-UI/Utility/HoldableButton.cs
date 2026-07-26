using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Micwu.UI.Utility
{
    /// <summary>
    /// A lightweight pointer-driven button. Fires OnHold the instant the pointer presses down
    /// (more responsive than Button's on-release click) and OnRelease when it lifts or leaves.
    /// Not a UnityEngine.UI.Button: needs a raycast-target Graphic (e.g. an Image) on this object
    /// or a child to receive pointer events.
    /// </summary>
    public class HoldableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool Interactable = true;

        public event Action OnHold;    // pointer pressed down
        public event Action OnRelease; // pointer lifted, or left the button while held

        public bool IsHeld { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable || IsHeld) return;
            IsHeld = true;
            OnHold?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) => Release();

        private void OnDisable() => Release(); // don't come back stuck-held after being re-enabled

        private void Release()
        {
            if (!IsHeld) return;
            IsHeld = false;
            OnRelease?.Invoke();
        }
    }
}
