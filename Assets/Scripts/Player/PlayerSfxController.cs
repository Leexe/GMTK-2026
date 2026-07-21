using FMOD.Studio;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerSfxController : MonoBehaviour
{
	[SerializeField]
	private MyCharacterController _characterController;

	[Header("Customizations")]
	[SerializeField]
	[Tooltip("Time it takes the player to fall before the falling sfx plays")]
	[SuffixLabel("s")]
	private float _fallingSfxThreshold = 0.75f;

	[SerializeField]
	[Tooltip("Distance between footsteps before playing a sound")]
	private float _footstepDistance = 10f;

	private EventInstance _fallingSfxInstance;
	private bool _isFallingSfxPlaying;
	private float _fallingTimer;
	private float _distanceSinceLastFootstep;

	private void Start()
	{
		_fallingSfxInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.Falling_LoopSfx);
	}

	private void OnEnable()
	{
		if (_characterController != null)
		{
			_characterController.OnGroundJump.AddListener(PlayJumpSfx);
			_characterController.OnAirJump.AddListener(PlayJumpSfx);
		}
	}

	private void OnDisable()
	{
		if (_characterController != null)
		{
			_characterController.OnGroundJump.RemoveListener(PlayJumpSfx);
			_characterController.OnAirJump.RemoveListener(PlayJumpSfx);
		}
	}

	private void OnDestroy()
	{
		if (AudioManager.Instance)
		{
			AudioManager.Instance.DestroyInstance(_fallingSfxInstance);
		}
	}

	private void Update()
	{
		HandleFallingSfx();
		HandleFootstepSfx(Time.deltaTime);
	}

	private void HandleFallingSfx()
	{
		if (_characterController.IsFalling)
		{
			_fallingTimer += Time.deltaTime;
			if (_fallingTimer >= _fallingSfxThreshold && !_isFallingSfxPlaying)
			{
				AudioManager.Instance.PlayInstance(_fallingSfxInstance);
				_isFallingSfxPlaying = true;
			}
		}
		else
		{
			_fallingTimer = 0f;
			if (_isFallingSfxPlaying)
			{
				AudioManager.Instance.StopInstance(_fallingSfxInstance);
				_isFallingSfxPlaying = false;
			}
		}
	}

	private void HandleFootstepSfx(float deltaTime)
	{
		if (_characterController.IsGrounded && _characterController.CurrentHorVelocity.magnitude > 0.1f)
		{
			_distanceSinceLastFootstep += _characterController.CurrentHorVelocity.magnitude * deltaTime;
			if (_distanceSinceLastFootstep >= _footstepDistance)
			{
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Footsteps_Sfx);
				_distanceSinceLastFootstep = 0f;
			}
		}
		else
		{
			_distanceSinceLastFootstep = _footstepDistance;
		}
	}

	private void PlayJumpSfx()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Jump_Sfx);
	}
}
