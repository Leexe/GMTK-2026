// settings entry. for use in a settings entry prefab.
// takes:
// - setting key to reference (set via init func?)
// - prefab mappings for each setting type
// - whether to auto apply on modify
// - condition for hiding self.
// offers:
// - public method to apply setting
// - label, desc, visuals + correct input field

using System;
using System.Collections.Generic;
using Micwu.UI.InputFields.Keybind;
using Micwu.UI.InputFields.Selection;
using Micwu.UI.InputFields.Slider;
using TMPro;
using UnityEngine;

namespace Micwu.Settings.UI
{
    public class SettingsEntry : MonoBehaviour
    {
        public bool UseInspectorValues = false;
        public string InspectorSettingKey = "";

        [Header("Refs")]
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private RectTransform inputFieldParent;

        [Tooltip("If set at Init time, component uses the set input field instead of creating its own.")]
        public GameObject InputField;

        [Header("Options")]
        public bool AutoUpdateSettings = true;
        public AbstractSelectionField SelectionFieldPrefab;
        public AbstractSliderField SliderFieldPrefab;
        public AbstractKeybindField KeybindFieldPrefab;

        private SettingDefinition settingDef;
        private string fieldValue;
        public string SettingKey => settingDef.Key;

        public event Action<SettingsEntry, string> OnValueChanged;

        /** Unity Messages **/

        private void Awake()
        {
            if (UseInspectorValues) Init(InspectorSettingKey);
        }

        private void OnDestroy()
        {
            Settings.OnSettingsChanged -= OnSettingsChangedExternally;
        }

        /** Public Methods **/

        public void Init(string settingKey)
        {
            if (!Settings.IsASetting(settingKey))
            {
                Debug.LogError("SettingsEntry.Init: invalid setting key");
                return;
            }
            settingDef = Settings.Definitions[settingKey];

            labelText.text = settingDef.Label;
            if (descriptionText != null && !string.IsNullOrWhiteSpace(settingDef.Description))
            {
                descriptionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(settingDef.Description));
                descriptionText.text = settingDef.Description;
            }
            InitInputField();
            Settings.OnSettingsChanged += OnSettingsChangedExternally;
        }

        public void SyncSettings()
        {
            Settings.ChangeSetting(SettingKey, fieldValue);
        }

        public string GetValue()
        {
            if (TryMapDefToComponent(out SliderSettingDef slDef, out AbstractSliderField slComp, SliderFieldPrefab))
            {
                return slComp.Value.ToString();
            }
            else if (TryMapDefToComponent(out SelectionSettingDef scDef, out AbstractSelectionField scComp, SliderFieldPrefab))
            {
                return scComp.Value;
            }
            else if (TryMapDefToComponent(out KeybindSettingDef kbDef, out AbstractKeybindField kbComp, KeybindFieldPrefab))
            {
                return kbComp.Value;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SetValue(string value, bool immediate = false)
        {
            SetValueWithoutNotify(value, immediate);
            OnValueChanged?.Invoke(this, value);
        }

        public void SetValueWithoutNotify(string value, bool immediate)
        {
            if (TryMapDefToComponent(out SliderSettingDef slDef, out AbstractSliderField slComp, SliderFieldPrefab))
            {
                slComp.SetValueWithoutNotify(slDef.ParseValue(value), immediate);
            }
            else if (TryMapDefToComponent(out SelectionSettingDef scDef, out AbstractSelectionField scComp, SelectionFieldPrefab))
            {
                scComp.SetValueWithoutNotify(value, immediate);
            }
            else if (TryMapDefToComponent(out KeybindSettingDef kbDef, out AbstractKeybindField kbComp, KeybindFieldPrefab))
            {
                kbComp.SetValueWithoutNotify(value, immediate);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /** Private Helpers **/

        private void InitInputField()
        {
            string currentSettingVal = Settings.Values[SettingKey];
            
            if (TryMapDefToComponent(out SliderSettingDef slDef, out AbstractSliderField slComp, SliderFieldPrefab))
            {
                slComp.Init(slDef.Range, slDef.ParseValue(currentSettingVal), slDef.Steps);
                slComp.OnValueChanged += OnSliderValueChanged;
            }
            else if (TryMapDefToComponent(out SelectionSettingDef scDef, out AbstractSelectionField scComp, SelectionFieldPrefab))
            {
                scComp.Init(scDef.PossibleValues, currentSettingVal);
                scComp.OnValueChanged += OnSelectionValueChanged;
            }
            else if (TryMapDefToComponent(out KeybindSettingDef kbDef, out AbstractKeybindField kbComp, KeybindFieldPrefab))
            {
                kbComp.Init(kbDef.Action, kbDef.BindingId);
                kbComp.OnValueChanged += OnKeybindValueChanged;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        // a bit of unfortunate fuckery used to handle the "different setting type, different input field" mapping.
        // returns true if current settingDef is of specified type and corresponding component exists in InputField.
        // accepts a prefab to instantiate in case InputField is null.
        // will log errors - not just fail - if there's a component mismatch or missing prefab when needed.
        private bool TryMapDefToComponent<T, U>(out T def, out U component, Component fallbackPrefab = null) where T : SettingDefinition where U : Component
        {
            def = null;
            component = null;
            if (settingDef is not T matchedDef) return false;

            if (InputField == null)
            {
                if (fallbackPrefab == null)
                {
                    Debug.LogError("failed to instantiate input field");
                    return false;
                }
                InputField = Instantiate(fallbackPrefab.gameObject, inputFieldParent);
            }

            if (!InputField.TryGetComponent(out U comp))
            {
                Debug.LogError("bad input field");
                return false;
            }

            def = matchedDef;
            component = comp;
            return true;
        }

        /** Event Handlers **/

        private void OnSliderValueChanged(float val)
        {
            fieldValue = val.ToString();
            OnValueChanged?.Invoke(this, fieldValue);
            if (AutoUpdateSettings) SyncSettings();
        }

        private void OnSelectionValueChanged(string val)
        {
            fieldValue = val;
            OnValueChanged?.Invoke(this, fieldValue);
            if (AutoUpdateSettings) SyncSettings();
        }

        private void OnKeybindValueChanged(string val)
        {
            fieldValue = val;
            OnValueChanged?.Invoke(this, fieldValue);
            if (!AutoUpdateSettings) return;

            KeybindSettingDef.SwapConflicts(SettingKey, val, Settings.Values[SettingKey]);
            SyncSettings();
        }

        private void OnSettingsChangedExternally(IReadOnlyList<(string key, string val)> changes)
        {
            foreach (var (key, val) in changes)
                if (key == SettingKey) SetValueWithoutNotify(val, false);
        }
    }
}