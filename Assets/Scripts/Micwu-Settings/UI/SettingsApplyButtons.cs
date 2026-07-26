using System.Collections.Generic;
using System.Linq;
using Micwu.UI.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Micwu.Settings.UI
{
    public class SettingsApplyButtons : MonoBehaviour
    {
        public bool UseInspectorValues = false;
        public SettingsEntry[] InspectorSettingsEntries;

        [SerializeField] private HoldableButton applyButton;
        [SerializeField] private HoldableButton revertButton;

        private readonly Dictionary<string, SettingsEntry> entryByKey = new();
        private readonly Dictionary<string, string> originalSet = new();
        private readonly Dictionary<string, string> changeSet = new();

        /** Unity Messages **/

        private void Start()
        {
            if (UseInspectorValues) Init(InspectorSettingsEntries);
        }

        /** Public Methods **/

        public void Init(IEnumerable<SettingsEntry> entries)
        {
            foreach (var entry in entries)
            {
                entryByKey[entry.SettingKey] = entry;
                originalSet[entry.SettingKey] = entry.GetValue();
                entry.AutoUpdateSettings = false;
                entry.OnValueChanged += OnEntryValueChanged;
            }

            if (applyButton) applyButton.OnHold += ApplySettings;
            if (revertButton) revertButton.OnHold += () => RevertChanges(true);
            RefreshButtons();
        }

        public void ApplySettings()
        {
            if (changeSet.Count == 0) return;

            List<(string, string)> changes = changeSet.Select((k) => (k.Key, k.Value)).ToList();
            Settings.ChangeSettings(changes);

            foreach (var k in changeSet.Keys)
            {
                originalSet[k] = changeSet[k];
            }
            changeSet.Clear();
            RefreshButtons();
        }

        public void RevertChanges(bool immediate = false)
        {
            foreach (var key in changeSet.Keys)
            {
                entryByKey[key].SetValueWithoutNotify(originalSet[key], immediate);
            }
            changeSet.Clear();
            RefreshButtons();
        }

        /** Event Handlers **/

        private void OnEntryValueChanged(SettingsEntry entry, string value)
        {
            string key = entry.SettingKey;

            if (value == originalSet[key])
            {
                changeSet.Remove(key);
            }
            else
            {
                changeSet[key] = value;
            }

            RefreshButtons();
        }

        /** Event Handlers **/

        private void RefreshButtons()
        {
            bool interactable = changeSet.Count > 0;
            applyButton.Interactable = interactable;
            revertButton.Interactable = interactable;
            if (applyButton.TryGetComponent(out Button applyBtn)) applyBtn.interactable = interactable;
            if (revertButton.TryGetComponent(out Button revertBtn)) revertBtn.interactable = interactable;
        }
    }
}
