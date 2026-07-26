using UnityEngine;
using TMPro;
using Micwu.UI.Utility;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Micwu.UI.InputFields.Keybind
{
    /// <summary>
    /// A selection field shown as a horizontal row of buttons; click a button to pick its value.
    /// </summary>
    public class KeybindField : AbstractKeybindField
    {
        [Header("Refs")]
        [SerializeField] private HoldableButton button; // button to use for buttons. assumes a TMP_Text child. will be disabled and used for reference.
        [SerializeField] private Image keyBackground;
        [SerializeField] private TMP_Text displayText;

        [Header("Colors")]
        public float unselectedAlpha = 0.5f;

        //

        /** Class Methods **/

        protected override void OnInit()
        {
            button.OnHold += OnRebind;
        }

        protected override void OnRefresh(bool immediate)
        {
            displayText.text = inputAction.GetBindingDisplayString(ResolveBindingIndex());
        }

        /** Event Handlers **/

        private void OnRebind()
        {
            inputAction.Disable();
            keyBackground.enabled = false;
            displayText.text = "...";
            inputAction.PerformInteractiveRebinding(ResolveBindingIndex())
                .WithActionEventNotificationsBeingSuppressed()
                .OnComplete(EndRebind)
                .OnCancel(EndRebind)
                .Start();
        }

        private void EndRebind(InputActionRebindingExtensions.RebindingOperation op)
        {
            op.Dispose();
            keyBackground.enabled = true;
            inputAction.Enable();
            OnRefresh(false);
            InvokeValueChanged();
        }
    }
}
