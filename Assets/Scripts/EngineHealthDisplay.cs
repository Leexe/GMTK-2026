using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EngineHealthDisplay : MonoBehaviour
{
	public List<EmissiveLight> Lights;
    public TMP_Text PercentText;

	private void OnEnable()
	{
		GameManager.Instance.OnEngineUpdate += HandleEngineUpdate;
		HandleEngineUpdate();
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnEngineUpdate -= HandleEngineUpdate;
		}
	}

	private void HandleEngineUpdate()
	{
		UpdateLights(GameManager.Instance.EngineIntegrityNormalized);
        PercentText.text = $"{Mathf.FloorToInt(GameManager.Instance.EngineIntegrityNormalized * 100f)}%";
	}

	private void UpdateLights(float normalizedHealth)
	{
		var baseColor = Color.HSVToRGB(0.32f * normalizedHealth, 1f, 0.5f);
		for (int i = 0; i < Lights.Count; i++)
		{
			Lights[i].BaseColor = baseColor;
			Lights[i].Brightness = Mathf.Clamp01((normalizedHealth * Lights.Count) - i);
		}
	}
}
