using System.Collections.Generic;
using Micwu.Settings;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
	// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	// called by gamemanager instead of before scene load, since volume sliders
	// require audiomanager to be initialized
	public static void SetupSettings()
	{
		Settings.OnSettingsChanged += HandleSettingsChange;

#pragma warning disable IDE1006 // Naming Styles
		Settings.DefineSettings(
			new SettingDefinition[]
			{
				// SOUNDS
				new SliderSettingDef()
				{
					Key = "MasterVolume",
					Label = "Master",
					DefaultVal = "50",
					Range = new(0f, 100f),
					Steps = 21,
					OnApply = v => SetVolume(AudioManager.AudioBusType.Master, v),
				},
				// new SliderSettingDef()
				// {
				// 	Key = "MusicVolume",
				// 	Label = "Music",
				// 	DefaultVal = "100",
				// 	Range = new(0f, 100f),
				// 	Steps = 21,
				// 	OnApply = v => SetVolume(AudioManager.AudioBusType.Music, v),
				// },
				new SliderSettingDef()
				{
					Key = "SFXVolume",
					Label = "SFX",
					DefaultVal = "100",
					Range = new(0f, 100f),
					Steps = 21,
					OnApply = v => SetVolume(AudioManager.AudioBusType.Game, v),
				},
				new SliderSettingDef()
				{
					Key = "AmbienceVolume",
					Label = "Ambience",
					DefaultVal = "100",
					Range = new(0f, 100f),
					Steps = 21,
					OnApply = v => SetVolume(AudioManager.AudioBusType.Ambience, v),
				},

			}
		);
#pragma warning restore IDE1006 // Naming Styles
	}

	private static void HandleSettingsChange(IEnumerable<(string, string)> vals)
	{
		//
	}

	private static void SetVolume(AudioManager.AudioBusType bus, string val)
	{
		if (!FMODUnity.RuntimeManager.HaveAllBanksLoaded || AudioManager.Instance == null)
		{
			return;
		}

		if (SliderSettingDef.TryParse(val, out float v))
		{
			AudioManager.Instance.SetVolume(bus, v * 0.01f);
		}
	}
}
