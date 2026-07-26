using UnityEngine;
using TMPro;
using Micwu.UI.Utility;

namespace Micwu.UI.InputFields.Selection
{
    /// <summary>
    /// A selection field with left/right buttons as well as dots showing your selection.
    /// </summary>
    public class SelectionSwitcherField : AbstractSelectionField
    {
        [Header("Refs")]
        [SerializeField] private HoldableButton leftButton;
        [SerializeField] private HoldableButton rightButton;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private RectTransform dotsLayout;
        [SerializeField] private TMP_Text dot; // dot to use for dots. will be disabled and used for reference.

        [Header("Colors")]
        public float unselectedAlpha = 0.5f;

        //

        private TMP_Text[] dots;

        /** Unity Messages **/

        private void OnEnable()
        {
            leftButton.OnHold += OnLeftPressed;
            rightButton.OnHold += OnRightPressed;
        }

        private void OnDisable()
        {
            leftButton.OnHold -= OnLeftPressed;
            rightButton.OnHold -= OnRightPressed;
        }

        /** Class Methods **/

        protected override void OnInit()
        {
            leftButton.Interactable = possibleValues.Length > 1;
            rightButton.Interactable = possibleValues.Length > 1;
            InitDots();
        }

        protected override void OnRefresh(bool immediate)
        {
            valueText.text = labels[selection];

            if (dots == null) return;
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].alpha = i == selection ? 1f : unselectedAlpha;
            }
        }

        /** Event Handlers **/

        private void OnLeftPressed() => Step(-1);
        private void OnRightPressed() => Step(1);

        /** Private Helpers **/

        private void Step(int delta)
        {
            if (possibleValues == null || possibleValues.Length < 1) return;
            int ct = possibleValues.Length;
            string newVal = possibleValues[((selection + delta) % ct + ct) % ct];
            SetValue(newVal);
        }

        private void InitDots()
        {
            dot.gameObject.SetActive(false);

            for (int i = dotsLayout.childCount - 1; i >= 0; i--)
            {
                Transform child = dotsLayout.GetChild(i);
                if (child != dot.transform) Destroy(child.gameObject);
            }

            dots = new TMP_Text[possibleValues.Length];

            for (int i = 0; i < possibleValues.Length; i++)
            {
                dots[i] = Instantiate(dot, dotsLayout);
                dots[i].gameObject.SetActive(true);
            }
        }

    }
}