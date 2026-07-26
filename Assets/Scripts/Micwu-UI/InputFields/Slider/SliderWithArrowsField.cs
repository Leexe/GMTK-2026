using Micwu.UI.Utility;
using TMPro;
using UnityEngine;

namespace Micwu.UI.InputFields.Slider
{
    /// <summary>
    /// A slider field with left/right arrow buttons. With steps, arrows move one step; without steps,
    /// holding an arrow changes the value at a constant rate. The slider can also be dragged directly.
    /// </summary>
    public class SliderWithArrowsField : AbstractSliderField
    {
        [Header("Refs")]
        [SerializeField] private HoldableButton leftButton;
        [SerializeField] private HoldableButton rightButton;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private UnityEngine.UI.Slider slider;

        [Header("Params")]
        public float ChangeRate = 0.5f; // (normalized) fraction of the range per second, while a button is held without steps.
        public string ValueFormat = "0.##"; // numeric format for valueText.

		private bool initialized = false;

        /** Unity Messages **/

        private void OnEnable()
        {
            leftButton.OnHold += OnLeftPressed;
            rightButton.OnHold += OnRightPressed;
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        private void OnDisable()
        {
            leftButton.OnHold -= OnLeftPressed;
            rightButton.OnHold -= OnRightPressed;
            slider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        private void Update()
        {
            if (HasSteps) return; // held-to-change is for continuous sliders only

            float dir = (leftButton.IsHeld ? -1f : 0f) + (rightButton.IsHeld ? 1f : 0f);
            if (dir == 0f) return;

            float delta = dir * ChangeRate * (Range.y - Range.x) * Time.unscaledDeltaTime;
            SetValue(Value + delta);
        }

        /** Class Methods **/

        protected override void OnInit()
        {
			initialized = false;
            if (HasSteps)
            {
                slider.minValue = 0;
                slider.maxValue = Steps - 1;
                slider.wholeNumbers = true;
            }
            else
            {
                slider.minValue = Range.x;
                slider.maxValue = Range.y;
            }
			initialized = true;
        }

        protected override void OnRefresh(bool immediate)
        {
            if (HasSteps)
            {
				int step = Mathf.RoundToInt(Mathf.InverseLerp(Range[0], Range[1], Value) * (Steps - 1));
                slider.SetValueWithoutNotify(step);
            }
            else
            {
                slider.SetValueWithoutNotify(Value);
            }
            valueText.text = Value.ToString(ValueFormat);
        }

        /** Event Handlers **/

        private void OnLeftPressed() { if (HasSteps) StepBy(-1); }
        private void OnRightPressed() { if (HasSteps) StepBy(1); }

        private void OnSliderChanged(float value)
        {
			if (!initialized) return;

            if (HasSteps)
            {
                SetValue(Mathf.Lerp(Range[0], Range[1], value / (Steps - 1)));
            }
            else
            {
                SetValue(value);
            }
        }

        /** Private Helpers **/

        private void StepBy(int dir)
        {
            if (HasSteps) slider.value += dir;
        }
    }
}
