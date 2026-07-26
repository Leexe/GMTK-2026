using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Micwu.Settings
{
    public abstract class SettingDefinition
    {
        public string Key;
        public string Label;
        public string Description;
        public string DefaultVal;
        public Action<string> OnApply;

        public void Validate()
        {
            if (string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(Label)) throw new ArgumentException("bad key/label");
            ValidateSelf();
            if (!IsValidVal(DefaultVal)) throw new ArgumentException("bad defaultValue");
        }

        public void Apply(string val)
        {
            ApplySelf(val);
            OnApply?.Invoke(val);
        }

        public abstract bool IsValidVal(string val);
        protected abstract void ValidateSelf();
        protected virtual void ApplySelf(string val) { }
    }

    //

    public sealed class SelectionSettingDef : SettingDefinition
    {
        public string[] PossibleValues;

        public override bool IsValidVal(string val)
        {
            return PossibleValues.Contains(val);
        }

        protected override void ValidateSelf()
        {
            if (PossibleValues.Length == 0f) throw new ArgumentException("empty possibleValues array");
            if (PossibleValues.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("bad value in possibleValues");
        }
    }


    public sealed class SliderSettingDef : SettingDefinition
    {
        private const float Epsilon = 1e-4f;
        public Vector2 Range;
        public int Steps;

        public override bool IsValidVal(string val)
        {
            if (!TryParse(val, out float n)) return false;
            return TryGetNearestStep(n, out float _);
        }

        public float ParseValue(string val)
        {
            if (!TryParse(val, out float n)) throw new FormatException($"slider '{Key}': '{val}' is not numeric");
            return n;
        }

        protected override void ValidateSelf()
        {
            if (Range[0] > Range[1]) throw new ArgumentException("bad range");
            if (Steps < 0) throw new ArgumentException("invalid step count (should be positive or 0 for no steps)");
        }

        // returns whether the input is acceptably close to a step, and outputs the nearest valid step.
        public bool TryGetNearestStep(float value, out float validStep)
        {
            if (Steps == 0)
            {
                validStep = Mathf.Clamp(value, Range[0], Range[1]);
                return value > Range[0] - Epsilon && value < Range[1] + Epsilon;
            }
            else
            {
                float stepNum = (value - Range[0]) / (Range[1] - Range[0]) * (Steps - 1);
                int nearestStepNum = Mathf.Clamp(Mathf.RoundToInt(stepNum), 0, Steps - 1);
                validStep = nearestStepNum * (Range[1] - Range[0]) + Range[0];
                return Mathf.Abs(nearestStepNum - stepNum) < Epsilon;
            }
        }

        public static bool TryParse(string val, out float result)
        {
            return float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }
    }

    public sealed class KeybindSettingDef : SettingDefinition
    {
        public InputAction Action;
        public Guid BindingId;

        private int BindingIndex => Action.bindings.IndexOf(b => b.id == BindingId);

        // blank string means unbound; null means use default
        public override bool IsValidVal(string val) =>
            string.IsNullOrEmpty(val) || InputControlPath.TryGetDeviceLayout(val) != null;

        protected override void ValidateSelf()
        {
            if (Action == null) throw new ArgumentException($"keybind '{Key}': no InputAction");
            if (BindingIndex < 0) throw new ArgumentException($"keybind '{Key}': binding id not found on action");
            if (string.IsNullOrEmpty(DefaultVal)) DefaultVal = Action.bindings[BindingIndex].path;
        }

        protected override void ApplySelf(string val) => Action.ApplyBindingOverride(BindingIndex, val);

        public static void SwapConflicts(string changedKey, string path, string replacementPath)
        {
            if (string.IsNullOrEmpty(path)) return;
            foreach (var def in Settings.Definitions.Values)
            {
                if (def.Key == changedKey || def is not KeybindSettingDef) continue;
                if (Settings.Values[def.Key] == path) Settings.ChangeSetting(def.Key, replacementPath);
            }
        }

        // note: bindings can have the same name. this function returns the first one in the list.
        public static KeybindSettingDef FromBindingName(string key, string label, string actionPath, string bindName)
        {
            InputAction action = InputSystem.actions.FindAction(actionPath);
            InputBinding binding = action.bindings.First(b => b.name == bindName);

            return new()
            {
                    Key=key, Label = label,
                    DefaultVal = "",
                    Action = action,
                    BindingId = binding.id
            };
        }
    }
}

