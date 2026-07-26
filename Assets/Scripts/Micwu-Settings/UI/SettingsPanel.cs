using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Micwu.Settings.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        public bool UseAllSettingsInOrder = false;
        public string AllSettingsTitle = "All Settings"; 
        public SettingsSection[] InspectorSettingsList = new SettingsSection[0];

        [SerializeField] private TMP_Text headingPrefab;
        [SerializeField] private SettingsApplyButtons applyButtonPrefab;
        [SerializeField] private SettingsEntry settingsEntryPrefab;
        [SerializeField] private RectTransform entryLayout;

        private void Awake()
        {
            if (UseAllSettingsInOrder)
            {
                SettingsSection oneSection = new()
                {
                    Title = AllSettingsTitle,
                    SettingsKeys = Settings.DefinitionList.Select(def => def.Key).ToArray()
                };
                InitPage(new SettingsSection[] { oneSection });
            }
            else
            {
                InitPage(InspectorSettingsList);
            }
        }

        private void InitPage(IEnumerable<SettingsSection> sections)
        {
            for (int i = entryLayout.childCount - 1; i >= 0; i--)
            {
                Destroy(entryLayout.GetChild(i).gameObject);
            }
            foreach (SettingsSection section in sections)
            {
                InitSection(section);
            }
        }

        private void InitSection(SettingsSection section)
        {
            if (section.SettingsKeys.Any(key => !Settings.IsASetting(key)))
            {
                Debug.LogError("SettingsPanel.BuildSettingsEntries: key list has invalid key(s)");
                return;
            }

            TMP_Text text = Instantiate(headingPrefab, entryLayout);
            text.text = section.Title;

            List<SettingsEntry> entries = new();
            foreach (var key in section.SettingsKeys)
            {
                SettingsEntry entry = Instantiate(settingsEntryPrefab, entryLayout);
                entry.Init(key);
                entries.Add(entry);
            }

            if (section.HasApplyButton)
            {
                SettingsApplyButtons btn = Instantiate(applyButtonPrefab, entryLayout);
                btn.Init(entries);
            }
        }

        /** Types **/

        [Serializable]
        public struct SettingsSection
        {
            public string Title;
            public bool HasApplyButton;
            public string[] SettingsKeys;
        }
    }
}
