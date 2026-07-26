using UnityEngine;
using TMPro;
using Micwu.UI.Utility;

namespace Micwu.UI.InputFields.Selection
{
    /// <summary>
    /// A selection field shown as a horizontal row of buttons; click a button to pick its value.
    /// </summary>
    public class SelectionButtonsField : AbstractSelectionField
    {
        [Header("Refs")]
        [SerializeField] private RectTransform buttonsLayout;
        [SerializeField] private HoldableButton button; // button to use for buttons. assumes a TMP_Text child. will be disabled and used for reference.

        [Header("Colors")]
        public float unselectedAlpha = 0.5f;

        //

        private TMP_Text[] buttonTexts;

        /** Class Methods **/

        protected override void OnInit()
        {
            InitButtons();
        }

        protected override void OnRefresh(bool immediate)
        {
            if (buttonTexts == null) return;
            for (int i = 0; i < buttonTexts.Length; i++)
            {
                buttonTexts[i].alpha = i == selection ? 1f : unselectedAlpha;
            }
        }

        /** Event Handlers **/

        private void OnButtonPressed(int index)
        {
            SetValue(possibleValues[index]);
        }

        /** Private Helpers **/

        private void InitButtons()
        {
            button.gameObject.SetActive(false);

            for (int i = buttonsLayout.childCount - 1; i >= 0; i--)
            {
                Transform child = buttonsLayout.GetChild(i);
                if (child != button.transform) Destroy(child.gameObject);
            }

            buttonTexts = new TMP_Text[possibleValues.Length];

            for (int i = 0; i < possibleValues.Length; i++)
            {
                HoldableButton instance = Instantiate(button, buttonsLayout);
                instance.gameObject.SetActive(true);

                TMP_Text text = instance.GetComponentInChildren<TMP_Text>();
                text.text = labels[i];
                buttonTexts[i] = text;

                int index = i;
                instance.OnHold += () => OnButtonPressed(index);
            }
        }
    }
}
