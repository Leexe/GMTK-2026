using System;
using UnityEngine;

namespace Micwu.UI.InputFields.Slider
{
    /// <summary>
    /// A slider field is a UI element that lets you pick a (float) value within a range,
    /// optionally snapped to a fixed number of evenly-spaced steps.
    /// Classes that inherit this aim to implement a UI that provides that underlying functionality.
    /// </summary>
    public abstract class AbstractSliderField : MonoBehaviour
    {
        public bool UseInspectorValues = false;
        public float InspectorValue = 0f;
        public Vector2 InspectorRange = new(0f, 1f); // x = min, y = max
        public int InspectorSteps = 0; // number of selectable values; <= 1 means continuous.

        //

        public event Action<float> OnValueChanged;

        protected Vector2 range;
        protected int steps;
        protected float currentValue;

        /** Unity Messages **/

        private void Awake()
        {
            if (UseInspectorValues) Init(InspectorRange, InspectorValue, InspectorSteps);
        }

        /** Public Methods **/

        public Vector2 Range => range;
        public int Steps => steps;
        public float Value => currentValue;
        public bool HasSteps => steps > 1;
        public float Normalized => Mathf.InverseLerp(range.x, range.y, currentValue);

        public virtual void Init(Vector2 range, float initialValue, int steps = 0)
        {
            this.range = range;
            this.steps = steps;
            currentValue = Snap(Mathf.Clamp(initialValue, range.x, range.y));
            OnInit();
            OnRefresh(true);
        }

        public void SetValue(float value, bool immediate = false)
        {
            SetValueWithoutNotify(value, immediate);
            InvokeValueChanged(currentValue);
        }

        public virtual void SetValueWithoutNotify(float value, bool immediate)
        {
            currentValue = Snap(Mathf.Clamp(value, range.x, range.y));
            OnRefresh(immediate);
        }

        /** Class Methods **/

        protected abstract void OnInit();
        protected abstract void OnRefresh(bool immediate);
        protected void InvokeValueChanged(float value) => OnValueChanged?.Invoke(value);

        /** Private Helpers **/

        private float Snap(float value)
        {
            if (!HasSteps) return value;
            float t = Mathf.InverseLerp(range.x, range.y, value);
            t = Mathf.Round(t * (steps - 1)) / (steps - 1);
            return Mathf.Lerp(range.x, range.y, t);
        }
    }
}
