using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;

public class NpcController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private GameObject _visuals;

	[SerializeField]
	private GameObject _meshGameObject;

	[SerializeField]
	private HoverTarget _hoverTarget;

	[SerializeField]
	private TextMeshProUGUI _dialogueText;

	[SerializeField]
	private CanvasGroup _dialogueCanvasGroup;

	[Header("Dialogue Settings")]
	[SerializeField]
	private float _greetingDialogueChance = 0.5f;

	[SerializeField]
	private float _acceptDialogueChance = 0.5f;

	[SerializeField]
	private float _rejectDialogueChance = 0.75f;

	[SerializeField]
	private float _dialogueDuration = 2f;

	[SerializeField]
	private float _dialogueFadeDuration = 0.2f;

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

	[Header("Footstep Settings")]
	[SerializeField]
	private float _stepInterval = 0.4f;

	[Header("Material Animation Settings")]
	[SerializeField]
	private NpcMaterialMapSO _materialMap;

	[SerializeField]
	private float _walkMaterialInterval = 0.15f;

	public System.Action<NpcController> OnArrivedAtPosition;
	public System.Action<NpcController> OnClicked;

	public Person Person => _person;
	public NpcRoles Role => _person.Role;
	public bool IsActive => _visuals.activeSelf;

	private Person _person;
	private Sequence _bounceSequence;
	private Sequence _footstepSequence;
	private Sequence _dialogueSequence;
	private Sequence _materialSequence;
	private Renderer _meshRenderer;
	private int _currentWalkFrameIndex;
	private Tween _lerpTween;
	private Tween _delayTween;
	private Vector3 _basePosition;

	private bool _hasClickListener = false;
	private bool _isSelected = false;
	private bool _isMoving = false;

	private Renderer MeshRenderer => _meshRenderer ??= _meshGameObject.GetComponent<Renderer>();

	private void OnEnable()
	{
		StartIdleBounce();
	}

	private void OnDisable()
	{
		StopBounce();
		_lerpTween.Stop();
		_dialogueSequence.Stop();
		StopWalkMaterialAnim();
	}

	public void Initialize(Person person, Vector3 position)
	{
		string warningText = person.IsSkinwalker ? " (SKINWALKER!)" : "";
		Debug.Log($"{person.Name} ({person.Role}) Initializing!" + warningText);
		_person = person;
		transform.position = position;
		_basePosition = position;
		HideDialogue();
		EnableVisuals();
		SetSelected(false);
		SetIdleMaterial();
	}

	public void SetSelected(bool selected)
	{
		_isSelected = selected;
		UpdateHighlight();
	}

	private void UpdateHighlight()
	{
		bool shouldHighlight = (_isSelected || _hoverTarget.Hovered) && !_isMoving;
		_meshGameObject.layer = shouldHighlight ? LayerMask.NameToLayer("Outlined") : LayerMask.NameToLayer("Default");
	}

	private void HandleClick()
	{
		OnClicked?.Invoke(this);
	}

	public void SetPerson(Person person)
	{
		_person = person;
	}

	public void LerpToPosition(Vector3 targetPosition, bool playFootsteps = false)
	{
		StopBounce();
		_lerpTween.Stop();

		_isMoving = true;
		UpdateHighlight();

		float delay = Random.Range(0f, _lerpDelay);

		if (playFootsteps)
		{
			_delayTween = Tween.Delay(this, delay, target => target.StartMoveBounce());
		}
		else
		{
			_delayTween = Tween.Delay(this, delay, target => target.StartMoveBounceNoSound());
		}

		_lerpTween = Tween.Position(transform, targetPosition, _lerpDuration, _lerpEase, startDelay: delay);
		_lerpTween.OnComplete(
			target: this,
			target =>
			{
				target._basePosition = targetPosition;
				target.StartIdleBounce();
				target.OnArrivedAtPosition?.Invoke(target);
				target._isMoving = false;
				target.UpdateHighlight();
			}
		);
	}

	public bool TrySayGreetingDialogue()
	{
		if (Random.value <= _greetingDialogueChance)
		{
			SayMeetDialogue();
			return true;
		}
		return false;
	}

	public bool TrySayAcceptDialogue()
	{
		if (Random.value <= _acceptDialogueChance)
		{
			SayAcceptDialogue();
			return true;
		}
		return false;
	}

	public bool TrySayRejectDialogue()
	{
		if (Random.value <= _rejectDialogueChance)
		{
			SayRejectDialogue();
			return true;
		}
		return false;
	}

	public void SayMeetDialogue()
	{
		ShowDialogue(GameManager.Instance.NpcDialogueData.GetRandomMeetText());
	}

	public void SayAcceptDialogue()
	{
		ShowDialogue(GameManager.Instance.NpcDialogueData.GetRandomAcceptText());
	}

	public void SayRejectDialogue()
	{
		ShowDialogue(GameManager.Instance.NpcDialogueData.GetRandomRejectText());
	}

	private void ShowDialogue(string text, float duration = -1f)
	{
		_dialogueSequence.Stop();

		_dialogueText.text = text;
		_dialogueCanvasGroup.alpha = 0f;
		Transform canvasTransform = _dialogueCanvasGroup.transform;
		canvasTransform.localScale = Vector3.one * 0.85f;
		_dialogueCanvasGroup.gameObject.SetActive(true);

		float showDuration = duration > 0f ? duration : _dialogueDuration;

		_dialogueSequence = Sequence
			.Create()
			.Chain(
				Tween.Alpha(
					_dialogueCanvasGroup,
					startValue: 0f,
					endValue: 1f,
					duration: _dialogueFadeDuration,
					ease: Ease.OutQuad
				)
			)
			.Group(
				Tween.Scale(
					canvasTransform,
					startValue: Vector3.one * 0.85f,
					endValue: Vector3.one,
					duration: _dialogueFadeDuration,
					ease: Ease.OutBack
				)
			)
			.Chain(Tween.Delay(showDuration))
			.Chain(
				Tween.Alpha(
					_dialogueCanvasGroup,
					startValue: 1f,
					endValue: 0f,
					duration: _dialogueFadeDuration,
					ease: Ease.InQuad
				)
			)
			.Group(
				Tween.Scale(
					canvasTransform,
					startValue: Vector3.one,
					endValue: Vector3.one * 0.85f,
					duration: _dialogueFadeDuration,
					ease: Ease.InQuad
				)
			)
			.ChainCallback(this, target => target._dialogueCanvasGroup.gameObject.SetActive(false));
	}

	public void HideDialogue()
	{
		_dialogueSequence.Stop();
		_dialogueCanvasGroup.alpha = 0f;
		_dialogueCanvasGroup.gameObject.SetActive(false);
	}

	public void EnableVisuals()
	{
		_visuals.SetActive(true);
		if (!_hasClickListener)
		{
			_hoverTarget.OnClick += HandleClick;
			_hoverTarget.OnHover += UpdateHighlight;
			_hoverTarget.OnUnhover += UpdateHighlight;
			_hasClickListener = true;
		}
		StartIdleBounce();
	}

	public void DisableVisuals()
	{
		StopBounce();
		_lerpTween.Stop();
		HideDialogue();

		_visuals.SetActive(false);
		if (_hasClickListener)
		{
			_hoverTarget.OnClick -= HandleClick;
			_hoverTarget.OnHover -= UpdateHighlight;
			_hoverTarget.OnUnhover -= UpdateHighlight;
			_hasClickListener = false;
		}
	}

	public void StartMoveBounce()
	{
		StartBounce(_bounceHeight, _bounceDuration, _bounceEase);
		StartFootsteps();
		StartWalkMaterialAnim();
	}

	public void StartMoveBounceNoSound()
	{
		StartBounce(_bounceHeight, _bounceDuration, _bounceEase);
		StartWalkMaterialAnim();
	}

	public void StartIdleBounce()
	{
		StartBounce(_idleBounceHeight, _idleBounceDuration, _idleBounceEase);
		SetIdleMaterial();
	}

	private void StartFootsteps()
	{
		_footstepSequence.Stop();

		_footstepSequence = Sequence
			.Create(-1)
			.ChainCallback(this, target => target.PlayFootstepSound())
			.Chain(Tween.Delay(_stepInterval));
	}

	private void PlayFootstepSound()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Footsteps_Sfx, gameObject);
	}

	private void StartBounce(float height, float duration, Ease ease)
	{
		_bounceSequence.Stop();
		_footstepSequence.Stop();

		Transform targetTransform = _visuals.transform;
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
		_footstepSequence.Stop();
		StopWalkMaterialAnim();
		_visuals.transform.localPosition = Vector3.zero;
	}

	private List<Material> GetMaterialsForCurrentRole()
	{
		if (_person == null)
		{
			return null;
		}

		NpcRoles role = _person.Role;
		if (role == NpcRoles.Skinwalker)
		{
			role = _person is Skinwalker skinwalker ? skinwalker.FakeRole : NpcRoles.Worker;
		}
		return _materialMap.GetMaterialsForRole(role);
	}

	public void SetIdleMaterial()
	{
		StopWalkMaterialAnim();
		List<Material> materials = GetMaterialsForCurrentRole();
		if (materials == null)
		{
			return;
		}

		MeshRenderer.sharedMaterial = materials[2]; // Index 2 is the idle
	}

	public void StartWalkMaterialAnim()
	{
		_materialSequence.Stop();
		List<Material> materials = GetMaterialsForCurrentRole();
		if (materials == null)
		{
			return;
		}

		_currentWalkFrameIndex = 0;
		MeshRenderer.sharedMaterial = materials[_currentWalkFrameIndex];

		_materialSequence = Sequence
			.Create(-1)
			.Chain(Tween.Delay(_walkMaterialInterval))
			.ChainCallback(this, target => target.AdvanceWalkMaterialFrame());
	}

	private void AdvanceWalkMaterialFrame()
	{
		List<Material> materials = GetMaterialsForCurrentRole();
		if (materials == null)
		{
			return;
		}

		_currentWalkFrameIndex = (_currentWalkFrameIndex + 1) % materials.Count;
		MeshRenderer.sharedMaterial = materials[_currentWalkFrameIndex];
	}

	public void StopWalkMaterialAnim()
	{
		_materialSequence.Stop();
	}
}
