using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoSingleton<CameraManager>
{
	[Header("References")]
	public GameObject MainCameraGameObject;

	[SerializeField]
	private GameObject _cinemachine;

	[Header("FPS Camera Settings")]
	[SerializeField]
	private float _defaultCameraSensitivity = 1.125f;

	[Header("Focus Cameras")]
	[SerializeField]
	private CinemachineCamera _playerCamera;

	[SerializeField]
	private CinemachinePanTilt _panTilt;

	[SerializeField]
	private CinemachineInputAxisController _inputAxisController;

	[Header("Focus Settings")]
	[SerializeField]
	private float _focusTweenDuration = 0.5f;

	[Header("Descent Screen Shake Settings")]
	[SerializeField]
	private float _descentShakeAmplitude = 1.0f;

	[SerializeField]
	private float _descentShakeFrequency = 1.0f;

	[Header("Skinwalker Screen Shake Settings")]
	[SerializeField]
	private float _skinwalkerShakeDelay = 1.0f;

	[SerializeField]
	private float _skinwalkerShakeAmplitude = 3.0f;

	[SerializeField]
	private float _skinwalkerShakeFrequency = 3.0f;

	[SerializeField]
	private Vector3 _skinwalkerShakePositionStrength = new Vector3(0.25f, 0.25f, 0.25f);

	[SerializeField]
	private float _shakeResetDuration = 0.3f;

	private Sequence _focusSequence;
	private CinemachineBasicMultiChannelPerlin _perlin;
	private Tween _shakeTween;
	private Sequence _shakeSequence;
	private Tween _skinwalkerDelayTween;
	private Vector3 _initialCameraLocalPos;

	private CinemachineInputAxisController _cinemachineInputAxisController;
	public float CameraSensitivity { get; private set; }

	protected override void OnInitialized()
	{
		base.OnInitialized();

		CameraSensitivity = 1f;
		_cinemachineInputAxisController = _cinemachine.GetComponent<CinemachineInputAxisController>();
	}

	private void Start()
	{
		GameManager.Instance.OnStartDescent += StartDescentShake;
		GameManager.Instance.OnSkinWalkersAct += StartSkinwalkerShake;
		GameManager.Instance.OnSkinWalkersActEnd += StartDescentShake;
		GameManager.Instance.OnNewFloor += StopDescentShake;
		GameManager.Instance.OnGameLose += StopDescentShake;
		GameManager.Instance.OnGameWin += StopDescentShake;

		_perlin = _playerCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
		_initialCameraLocalPos = MainCameraGameObject.transform.localPosition;
	}

	private void OnDisable()
	{
		ClearFocus();
		StopDescentShake();
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStartDescent -= StartDescentShake;
			GameManager.Instance.OnSkinWalkersAct -= StartSkinwalkerShake;
			GameManager.Instance.OnSkinWalkersActEnd -= StartDescentShake;
			GameManager.Instance.OnNewFloor -= StopDescentShake;
			GameManager.Instance.OnGameLose -= StopDescentShake;
			GameManager.Instance.OnGameWin -= StopDescentShake;
		}
	}

	public void StartDescentShake()
	{
		_skinwalkerDelayTween.Stop();
		_shakeTween.Stop();
		_shakeSequence.Stop();

		if (_perlin != null)
		{
			_perlin.AmplitudeGain = _descentShakeAmplitude;
			_perlin.FrequencyGain = _descentShakeFrequency;
		}
		else
		{
			_initialCameraLocalPos = MainCameraGameObject.transform.localPosition;
			_shakeSequence = Sequence
				.Create(-1)
				.Chain(
					Tween.ShakeLocalPosition(
						MainCameraGameObject.transform,
						new Vector3(0.08f, 0.08f, 0.08f),
						0.5f,
						15f,
						enableFalloff: false
					)
				);
		}
	}

	public void StartSkinwalkerShake()
	{
		_skinwalkerDelayTween.Stop();
		_shakeTween.Stop();
		_shakeSequence.Stop();

		_skinwalkerDelayTween = Tween.Delay(
			this,
			_skinwalkerShakeDelay,
			() =>
			{
				if (_perlin != null)
				{
					_perlin.AmplitudeGain = _skinwalkerShakeAmplitude;
					_perlin.FrequencyGain = _skinwalkerShakeFrequency;
				}
				else
				{
					_initialCameraLocalPos = MainCameraGameObject.transform.localPosition;
					_shakeSequence = Sequence
						.Create(-1)
						.Chain(
							Tween.ShakeLocalPosition(
								MainCameraGameObject.transform,
								_skinwalkerShakePositionStrength,
								0.5f,
								25f,
								enableFalloff: false
							)
						);
				}
			}
		);
	}

	public void StopDescentShake()
	{
		_skinwalkerDelayTween.Stop();
		_shakeSequence.Stop();
		_shakeTween.Stop();

		if (_perlin != null)
		{
			_shakeTween = Tween.Custom(
				_perlin.AmplitudeGain,
				0f,
				_shakeResetDuration,
				onValueChange: val => _perlin.AmplitudeGain = val
			);
		}
		else
		{
			Tween.LocalPosition(MainCameraGameObject.transform, _initialCameraLocalPos, _shakeResetDuration);
		}
	}

	private void LockCamera()
	{
		foreach (
			InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in _cinemachineInputAxisController.Controllers
		)
		{
			if (c.Name == "Look X (Pan)")
			{
				c.Input.Gain = 0;
			}
			else if (c.Name == "Look Y (Tilt)")
			{
				c.Input.Gain = 0;
			}
		}
	}

	private void UnlockCamera()
	{
		foreach (
			InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in _cinemachineInputAxisController.Controllers
		)
		{
			if (c.Name == "Look X (Pan)")
			{
				c.Input.Gain = _defaultCameraSensitivity * CameraSensitivity;
			}
			else if (c.Name == "Look Y (Tilt)")
			{
				c.Input.Gain = -_defaultCameraSensitivity * CameraSensitivity;
			}
		}
	}

	public void ChangeSensitivity(float newSens)
	{
		foreach (
			InputAxisControllerBase<CinemachineInputAxisController.Reader>.Controller c in _cinemachineInputAxisController.Controllers
		)
		{
			if (c.Name == "Look X (Pan)")
			{
				c.Input.Gain = _defaultCameraSensitivity * newSens;
				CameraSensitivity = newSens;
			}
			else if (c.Name == "Look Y (Tilt)")
			{
				c.Input.Gain = -_defaultCameraSensitivity * newSens;
				CameraSensitivity = newSens;
			}
		}
	}

	public void FocusOn(Transform target)
	{
		// Disable Camera Movement
		if (_inputAxisController != null)
		{
			_inputAxisController.enabled = false;
		}

		Vector3 direction = target.position - _playerCamera.transform.position;
		if (direction == Vector3.zero)
		{
			return;
		}

		var targetRotation = Quaternion.LookRotation(direction);
		Vector3 targetEuler = targetRotation.eulerAngles;

		float targetPan = targetEuler.y;
		float targetTilt = targetEuler.x;
		if (targetTilt > 180f)
		{
			targetTilt -= 360f;
		}

		// Calculate shortest path for pan
		float startPan = _panTilt.PanAxis.Value;
		float deltaPan = Mathf.DeltaAngle(startPan, targetPan);
		float finalTargetPan = startPan + deltaPan;

		_focusSequence.Stop();
		_focusSequence = Sequence.Create();

		_focusSequence.Group(
			Tween.Custom(
				startPan,
				finalTargetPan,
				_focusTweenDuration,
				onValueChange: newVal => _panTilt.PanAxis.Value = newVal,
				ease: Ease.InOutSine
			)
		);

		_focusSequence.Group(
			Tween.Custom(
				_panTilt.TiltAxis.Value,
				targetTilt,
				_focusTweenDuration,
				onValueChange: newVal => _panTilt.TiltAxis.Value = newVal,
				ease: Ease.InOutSine
			)
		);
	}

	public void ClearFocus()
	{
		_focusSequence.Stop();
		if (_inputAxisController != null)
		{
			_inputAxisController.enabled = true;
		}
	}
}
