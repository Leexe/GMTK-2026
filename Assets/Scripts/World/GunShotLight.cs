using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

public class GunShotLight : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private DynamicLight _light;

	[Header("Gunshot Light Settings")]
	[SerializeField]
	private float _flashIntensity = 160f;

	[SerializeField]
	private float _flashDuration = 0.15f;

	[SerializeField]
	private bool _customColor;

	[SerializeField]
	[ShowIf("@_customColor")]
	private Color _flashColor = new Color(1f, 0.85f, 0.4f);

	private void OnEnable()
	{
		GameManager.Instance.OnGunShot += TriggerGunShot;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGunShot -= TriggerGunShot;
		}
	}

	[Button]
	public void TriggerGunShot()
	{
		_light.SetBaseIntensity(0f);
		if (_customColor)
		{
			_light.FlashIntensity(_flashIntensity, _flashColor, _flashDuration);
		}
		else
		{
			_light.FlashIntensity(_flashIntensity, _flashDuration);
		}
	}
}
