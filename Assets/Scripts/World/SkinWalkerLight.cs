using PrimeTween;
using UnityEngine;

public class SkinWalkerLight : MonoBehaviour
{
	[Header("Referneces")]
	[SerializeField]
	private DynamicLight _light;

	[Header("Options")]
	[SerializeField]
	private float _targetLightIntensity;

	[SerializeField]
	private Color _targetColor;

	[SerializeField]
	private float _fadeInDuration;

	[SerializeField]
	private float _fadeOutDuration;

	private bool _skinwalkersAttacked;

	private void OnEnable()
	{
		GameManager.Instance.OnSkinWalkersAct += SkinWalkersAct;
		GameManager.Instance.OnNewFloor += TurnBackOnLights;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnSkinWalkersAct -= SkinWalkersAct;
			GameManager.Instance.OnNewFloor -= TurnBackOnLights;
		}
	}

	private void SkinWalkersAct()
	{
		_light.TweenLights(_targetLightIntensity, _targetColor, _fadeOutDuration);
		_skinwalkersAttacked = true;
	}

	private void TurnBackOnLights()
	{
		if (_skinwalkersAttacked)
		{
			_light.ResetLights(_fadeInDuration);
		}
	}
}
