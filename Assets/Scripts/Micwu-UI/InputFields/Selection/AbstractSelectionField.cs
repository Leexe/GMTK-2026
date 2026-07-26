using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Micwu.UI.InputFields.Selection
{
    /// <summary>
    /// A selection field is a UI element that lets you pick between a set of fixed (string) values.
    /// Classes that inherit this aim to implement a UI that provides that underlying functionality.
    /// </summary>
    public abstract class AbstractSelectionField : MonoBehaviour
    {
        public bool UseInspectorValues = false;
        public string InspectorValue = "";
        public LabelValue[] InspectorPossibleValues = new LabelValue[0];

        //

        public event Action<string> OnValueChanged;

        protected string[] possibleValues;
        protected string[] labels;
        protected int selection;

        /** Unity Messages **/

        private void Awake()
        {
            if (UseInspectorValues)
            {
                string[] valList = InspectorPossibleValues.Select(e => e.Value).ToArray();
                string[] labelList = InspectorPossibleValues.Select(e => string.IsNullOrEmpty(e.Label) ? e.Value : e.Label).ToArray();
                Init(valList, InspectorValue, labelList);
            }
        }

        /** Public Methods **/

        public IReadOnlyList<string> PossibleValues => possibleValues;
        public IReadOnlyList<string> Labels => labels;
        public int Selection => selection;
        public string Value => possibleValues[selection];

        public void Init(string[] possibleValues, string initialValue, string[] labels = null)
        {
            if (labels != null && labels.Length != possibleValues.Length) throw new ArgumentException("value/label array length mismatch");
            selection = Array.IndexOf(possibleValues, initialValue);
            if (selection < 0) throw new ArgumentException("possible values does not contain initial value");
            this.possibleValues = possibleValues;
            this.labels = labels ?? possibleValues;
            OnInit();
            OnRefresh(true);
        }

        public void SetValue(string value, bool immediate = false)
        {
            SetValueWithoutNotify(value, immediate);
            InvokeValueChanged(Value);
        }

        public void SetValueWithoutNotify(string value, bool immediate)
        {
            int index = Array.IndexOf(possibleValues, value);
            if (index < 0) throw new ArgumentException($"possible values does not contain '{value}'");
            selection = index;
            OnRefresh(immediate);
        }

        /** Class Methods **/

        protected abstract void OnInit();
        protected abstract void OnRefresh(bool immediate);
        protected void InvokeValueChanged(string value) => OnValueChanged?.Invoke(value);

        /** Types **/

        [Serializable]
        public struct LabelValue
        {
            public string Value;
            public string Label;
        }
    }
}