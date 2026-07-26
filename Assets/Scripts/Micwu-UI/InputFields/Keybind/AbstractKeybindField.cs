using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Micwu.UI.InputFields.Keybind
{
    /// <summary>
    /// A keybind field is a UI element that lets you pick an input key.
    /// The "value" of the field is the override path (ex. "<Keyboard>/Space") or null/empty.
    /// </summary>
    public abstract class AbstractKeybindField : MonoBehaviour
    {
        public bool UseInspectorValues = false;
        public string InspectorInputActionPath;
        public string InspectorBindingGuid;

        [SerializeField] private InputActionAsset inputAsset;

        //

        public event Action<string> OnValueChanged;

        protected InputAction inputAction;
        protected Guid bindingGuid;
        public string Value => inputAction.bindings[ResolveBindingIndex()].effectivePath;

        /** Unity Messages **/

        private void Awake()
        {
            if (UseInspectorValues)
            {
                InputAction action = inputAsset.FindAction(InspectorInputActionPath);
                Init(action, new Guid(InspectorBindingGuid));
            }
        }

        /** Public Methods **/

        public virtual void Init(InputAction inputAction, Guid bindingGuid)
        {
            this.inputAction = inputAction;
            this.bindingGuid = bindingGuid;
            OnInit();
            OnRefresh(true);
        }

        public void SetValue(string value, bool immediate = false)
        {
            SetValueWithoutNotify(value, immediate);
            InvokeValueChanged();
        }

        public void SetValueWithoutNotify(string path, bool immediate)
        {
            inputAction.ApplyBindingOverride(ResolveBindingIndex(), path);
            OnRefresh(immediate);
        }

        /** Class Methods **/

        protected abstract void OnInit();
        protected abstract void OnRefresh(bool immediate);
        protected void InvokeValueChanged() => OnValueChanged?.Invoke(Value);

        protected int ResolveBindingIndex()
        {
            return inputAction.bindings.IndexOf(b => b.id == bindingGuid);
        }
    }
}
