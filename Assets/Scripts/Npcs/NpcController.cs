using PrimeTween;
using UnityEngine;

public class NpcController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private GameObject _visuals;

	[SerializeField]
	private HoverTarget _hoverTarget;

	[Header("Move Bounce Settings")]
	[SerializeField]
	private float _bounceHeight = 0.15f;

	[SerializeField]
	private float _bounceDuration = 0.8f;

	[SerializeField]
	private Ease _bounceEase = Ease.InOutSine;

	[Header("Idle Bounce Settings")]
	[SerializeField]
	private float _idleBounceHeight = 0.05f;

	[SerializeField]
	private float _idleBounceDuration = 1.6f;

	[SerializeField]
	private Ease _idleBounceEase = Ease.InOutSine;

	[Header("Lerp Settings")]
	[SerializeField]
	private float _lerpDuration = 0.5f;

	[SerializeField]
	private float _lerpDelay = 0.25f;

	[SerializeField]
	private Ease _lerpEase = Ease.InOutQuad;

	public System.Action<NpcController> OnArrivedAtPosition;
	public System.Action<NpcController> OnClicked;

	public Person Person => _person;
	public NpcRoles Role => _person.Role;
	public bool IsActive => _visuals != null && _visuals.activeSelf;

	private Person _person;
	private Sequence _bounceSequence;
	private Tween _lerpTween;
	private Tween _delayTween;
	private Vector3 _basePosition;


	private bool _hasClickListener = false;

	public void Initialize(Person person, Vector3 position)
	{
		Debug.Log($"{person.Name} ({person.Role}) Initializing!");
		_person = person;
		transform.position = position;
		_basePosition = position;
		EnableVisuals();
	}

	private void HandleClick()
	{
		OnClicked?.Invoke(this);
	}

	public void SetPerson(Person person)
	{
		_person = person;
	}

public void LerpToPosition(Vector3 targetPosition)
	{
		StopBounce();
		_lerpTween.Stop();

		float delay = Random.Range(0f, _lerpDelay);
		_delayTween = Tween.Delay(this, delay, target => target.StartMoveBounce());

		_lerpTween = Tween.Position(transform, targetPosition, _lerpDuration, _lerpEase, startDelay: delay);
		_lerpTween.OnComplete(
			target: this,
			target =>
			{
				target._basePosition = targetPosition;
				target.StartIdleBounce();
				target.OnArrivedAtPosition?.Invoke(target);
			}
		);
	}

	public void EnableVisuals()
	{
		if (_visuals != null)
		{
			_visuals.SetActive(true);
			if (!_hasClickListener)
			{
				_hoverTarget.OnClick += HandleClick;
				_hasClickListener = true;
			}
		}
		StartIdleBounce();
	}

	public void DisableVisuals()
	{
		StopBounce();
		_lerpTween.Stop();

		if (_visuals != null)
		{
			_visuals.SetActive(false);
			if (_hasClickListener)
			{
				_hoverTarget.OnClick -= HandleClick;	
				_hasClickListener = false;
			}
		}
	}

	public void StartMoveBounce()
	{
		StartBounce(_bounceHeight, _bounceDuration, _bounceEase);
	}

	public void StartIdleBounce()
	{
		StartBounce(_idleBounceHeight, _idleBounceDuration, _idleBounceEase);
	}

	private void StartBounce(float height, float duration, Ease ease)
	{
		_bounceSequence.Stop();

		Transform targetTransform = _visuals != null ? _visuals.transform : transform;
		float halfDuration = duration / 2f;
		Vector3 upPosition = Vector3.up * height;

		_bounceSequence = Sequence
			.Create(-1, Sequence.SequenceCycleMode.Yoyo)
			.Chain(Tween.LocalPosition(targetTransform, upPosition, halfDuration, ease))
			.Chain(Tween.LocalPosition(targetTransform, Vector3.zero, halfDuration, ease));

		_bounceSequence.elapsedTime = Random.Range(0f, duration);
	}

	private void StopBounce()
	{
		_delayTween.Stop();
		_bounceSequence.Stop();
			_visuals.transform.localPosition = Vector3.zero;
	}

	private void OnDisable()
	{
		StopBounce();
		_lerpTween.Stop();
	}
}
