using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Micwu.Settings
{
    public static class Settings
    {
        private static readonly Dictionary<string, SettingDefinition> defs = new();
        private static readonly List<SettingDefinition> defsAsList = new();
        private static readonly Dictionary<string, string> vals = new();

        public static IReadOnlyDictionary<string, SettingDefinition> Definitions => defs;
        public static IReadOnlyList<SettingDefinition> DefinitionList => defsAsList;
        public static IReadOnlyDictionary<string, string> Values => vals;
        public static event Action<IReadOnlyList<(string key, string val)>> OnSettingsChanged;

        /** Public Methods **/

        public static void DefineSettings(SettingDefinition[] definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));

            if (defs != null && defs.Count > 0)
            {
                Debug.LogWarning("Settings.DefineSettings: existing settings defined. overwriting");
                defs.Clear();
                defsAsList.Clear();
                vals.Clear();
            }

            defsAsList.AddRange(definitions);
            foreach (var def in definitions)
            {
                if (defs.ContainsKey(def.Key)) throw new ArgumentException($"duplicate setting def for {def.Key}");
                def.Validate();
                defs[def.Key] = def;
                vals[def.Key] = GetStoredSettingOrDefault(def);
                def.Apply(vals[def.Key]);
            }
        }

        public static void ChangeSettings(List<(string key, string val)> entries)
        {
            foreach (var (key, val) in entries)
            {
                if (!IsASetting(key)) throw new ArgumentException($"attempted to set nonexistent setting {key}");
                if (!defs[key].IsValidVal(val)) throw new ArgumentException($"invalid value for {key} ({val})");

                vals[key] = val;
                defs[key].Apply(val);
                SaveSetting(key);
            }
            OnSettingsChanged?.Invoke(entries);
            PlayerPrefs.Save();
        }

        public static void ChangeSetting(string setting, string value)
        {
            ChangeSettings(new() { (setting, value) });
        }

        
        public static void ResetSettings(List<string> settings)
        {
            if (settings.Any(k => !IsASetting(k))) throw new ArgumentException("attempted to reset nonexistent setting");
            ChangeSettings((List<(string key, string val)>)settings.Select(k => (k, defs[k].DefaultVal)));
        }

        public static void ResetSetting(string settingKey)
        {
            if (!IsASetting(settingKey)) throw new ArgumentException($"attempted to reset nonexistent setting {settingKey}");
            ChangeSetting(settingKey, defs[settingKey].DefaultVal);
        }

        public static bool IsASetting(string key)
        {
            return defs.ContainsKey(key) && vals.ContainsKey(key);
        }

        /** Helpers **/

        private static string GetStoredSettingOrDefault(SettingDefinition definition)
        {
            string savedVal = PlayerPrefs.GetString(definition.Key, definition.DefaultVal);

            if (definition.IsValidVal(savedVal))
            {
                return savedVal;
            }
            else
            {
                Debug.LogWarning($"Settings.GetStoredSettingOrDefault: bad setting loaded for {definition.Key}");
                return definition.DefaultVal;
            }
        }

        private static void SaveSetting(string setting)
        {
            if (!IsASetting(setting)) throw new ArgumentException("attempted to save nonexistent setting");
            PlayerPrefs.SetString(setting, vals[setting]);
        }
    }
}