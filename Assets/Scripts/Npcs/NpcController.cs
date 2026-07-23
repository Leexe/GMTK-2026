using PrimeTween;
using UnityEngine;

public class NpcController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private GameObject _visuals;

	[Header("Bounce Settings")]
	[SerializeField]
	private float _bounceHeight = 0.15f;

	[SerializeField]
	private float _bounceDuration = 0.8f;

	[SerializeField]
	private Ease _bounceEase = Ease.InOutSine;

	[Header("Lerp Settings")]
	[SerializeField]
	private float _lerpDuration = 0.5f;

	[SerializeField]
	private Ease _lerpEase = Ease.InOutQuad;

	public System.Action<NpcController> OnArrivedAtPosition;

	public NpcRoles Role => _role;
	public bool IsActive => _visuals != null && _visuals.activeSelf;

	private NpcRoles _role;
	private Sequence _bounceSequence;
	private Tween _lerpTween;
	private Vector3 _basePosition;

	public void Initialize(NpcRoles role, Vector3 position)
	{
		_role = role;
		transform.position = position;
		_basePosition = position;
		EnableVisuals();
		StopBounce();
	}

	public void SetRole(NpcRoles role)
	{
		_role = role;
	}

	public void LerpToPosition(Vector3 targetPosition)
	{
		StopBounce();
		_lerpTween.Stop();
		StartBounce();

		_lerpTween = Tween.Position(transform, targetPosition, _lerpDuration, _lerpEase);
		_lerpTween.OnComplete(
			target: this,
			target =>
			{
				target._basePosition = targetPosition;
				target.StopBounce();
				target.OnArrivedAtPosition?.Invoke(target);
			}
		);
	}

	public void EnableVisuals()
	{
		if (_visuals != null)
		{
			_visuals.SetActive(true);
		}
		StopBounce();
	}

	public void DisableVisuals()
	{
		StopBounce();
		_lerpTween.Stop();

		if (_visuals != null)
		{
			_visuals.SetActive(false);
		}
	}

	private void StartBounce()
	{
		StopBounce();

		Transform targetTransform = _visuals != null ? _visuals.transform : transform;
		float halfDuration = _bounceDuration / 2f;
		Vector3 upPosition = Vector3.up * _bounceHeight;

		_bounceSequence = Sequence
			.Create(-1, Sequence.SequenceCycleMode.Yoyo)
			.Chain(Tween.LocalPosition(targetTransform, upPosition, halfDuration, _bounceEase))
			.Chain(Tween.LocalPosition(targetTransform, Vector3.zero, halfDuration, _bounceEase));

		_bounceSequence.elapsedTime = Random.Range(0f, _bounceDuration);
	}

	private void StopBounce()
	{
		_bounceSequence.Stop();
		_visuals.transform.localPosition = Vector3.zero;
	}

	private void OnDisable()
	{
		StopBounce();
		_lerpTween.Stop();
	}
}
