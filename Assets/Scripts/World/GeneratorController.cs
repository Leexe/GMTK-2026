using UnityEngine;

public class GeneratorController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private DynamicLight _light;

	[Header("Custom Settings")]
	[SerializeField]
	private float _damageLightIntensity = 40f;

	[SerializeField]
	private float _damageLightDuration = 0.3f;

	[SerializeField]
	private float _repairLightIntensity = 40f;

	[SerializeField]
	private Color _repairColor;

	[SerializeField]
	private float _repairLightDuration = 0.3f;

	private void OnEnable()
	{
		GameManager.Instance.OnEngineDamage += EngineDamage;
		GameManager.Instance.OnEngineFix += EngineRepair;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnEngineDamage -= EngineDamage;
			GameManager.Instance.OnEngineFix -= EngineRepair;
		}
	}

	private void EngineDamage()
	{
		_light.FlashIntensity(_damageLightIntensity, _damageLightDuration);
		_particleSystem.Play();
	}

	private void EngineRepair()
	{
		_light.FlashIntensity(_repairLightIntensity, _repairColor, _repairLightDuration);
	}
}
