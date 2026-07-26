using PrimeTween;
using UnityEngine;

public class DialogueUIController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private RectTransform _dialogueBoxTransform;

	[SerializeField]
	private HoverTarget _hoverTarget;

	[Header("Tween Data")]
	[SerializeField]
	private float _yOffset = 100;

	[SerializeField]
	private float _activeYOffset = 100;

	[SerializeField]
	private float _tweenDuration;

	private Sequence _sequence;
	private Vector2 _startPosition;
	private Vector2 _endPosition;
	private Quaternion _startRotation;
	private Quaternion _endRotation;
	private bool _isActive;

	private void Start()
	{
		_startPosition = _dialogueBoxTransform.anchoredPosition;
		_endPosition = _dialogueBoxTransform.anchoredPosition + new Vector2(0, _yOffset);
		_startRotation = Quaternion.Euler(_dialogueBoxTransform.eulerAngles);
		_endRotation = Quaternion.Euler(Vector3.zero);
		OnUIOpen();
	}

	private void OnEnable()
	{
		DialogueManager.Instance.DialogueState.OnStartDialogue += OnUIOpen;
		DialogueManager.Instance.DialogueState.OnEndStory += OnUIClose;
		_hoverTarget.OnHover += OnHover;
		_hoverTarget.OnUnhover += OnUnhover;
	}

	private void OnDisable()
	{
		if (DialogueManager.Instance != null)
		{
			DialogueManager.Instance.DialogueState.OnStartDialogue -= OnUIOpen;
			DialogueManager.Instance.DialogueState.OnEndStory -= OnUIClose;
		}

		_hoverTarget.OnHover -= OnHover;
		_hoverTarget.OnUnhover -= OnUnhover;
	}

	private void OnHover()
	{
		if (!_isActive)
		{
			return;
		}

		Vector2 targetPos = _startPosition + new Vector2(0, _activeYOffset);
		AnimateTo(targetPos, _endRotation);
	}

	private void OnUnhover()
	{
		if (!_isActive)
		{
			return;
		}

		AnimateTo(_startPosition, _startRotation);
	}

	private void OnUIOpen()
	{
		_isActive = true;
		bool isHovered = _hoverTarget.Hovered;
		Vector2 targetPos = isHovered ? _startPosition + new Vector2(0, _activeYOffset) : _startPosition;
		Quaternion targetRot = isHovered ? _endRotation : _startRotation;
		AnimateTo(targetPos, targetRot);
	}

	private void OnUIClose()
	{
		_isActive = false;
		AnimateTo(_endPosition, _endRotation);
	}

	private void AnimateTo(Vector2 targetPosition, Quaternion targetRotation)
	{
		_sequence.Stop();
		_sequence = Sequence
			.Create()
			.Chain(Tween.UIAnchoredPosition(_dialogueBoxTransform, targetPosition, _tweenDuration))
			.Group(Tween.Rotation(_dialogueBoxTransform, targetRotation, _tweenDuration));
	}
}
