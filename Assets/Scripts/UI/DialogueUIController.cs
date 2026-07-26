using PrimeTween;
using UnityEngine;

public class DialogueUIController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private RectTransform _dialogueBoxTransform;

	[Header("Tween Data")]
	[SerializeField]
	private float _yOffset = 100;

	[SerializeField]
	private float _tweenDuration;

	private Sequence _sequence;
	private Vector2 _startPosition;
	private Vector2 _endPosition;
	private Quaternion _startRotation;
	private Quaternion _endRotation;

	private void Start()
	{
		_startPosition = _dialogueBoxTransform.anchoredPosition;
		_endPosition = _dialogueBoxTransform.anchoredPosition - new Vector2(0, _yOffset);
		_startRotation = Quaternion.Euler(_dialogueBoxTransform.eulerAngles);
		_endRotation = Quaternion.Euler(Vector3.zero);
		OnUIOpen();
	}

	private void OnEnable()
	{
		DialogueManager.Instance.DialogueState.OnStartDialogue += OnUIOpen;
		DialogueManager.Instance.DialogueState.OnEndStory += OnUIClose;
	}

	private void OnDisable()
	{
		if (DialogueManager.Instance != null)
		{
			DialogueManager.Instance.DialogueState.OnStartDialogue += OnUIOpen;
			DialogueManager.Instance.DialogueState.OnEndStory += OnUIClose;
		}
	}

	private void OnUIOpen()
	{
		_sequence.Stop();
		_sequence = Sequence.Create();
		if (_dialogueBoxTransform.anchoredPosition != _startPosition)
		{
			_sequence.Chain(Tween.UIAnchoredPosition(_dialogueBoxTransform, _startPosition, _tweenDuration));
		}

		if (_dialogueBoxTransform.rotation != _startRotation)
		{
			_sequence.Group(Tween.Rotation(_dialogueBoxTransform, _startRotation, _tweenDuration));
		}
	}

	private void OnUIClose()
	{
		_sequence.Stop();
		_sequence = Sequence.Create();
		if (_dialogueBoxTransform.anchoredPosition != _endPosition)
		{
			_sequence.Chain(Tween.UIAnchoredPosition(_dialogueBoxTransform, _endPosition, _tweenDuration));
		}

		if (_dialogueBoxTransform.rotation != _endRotation)
		{
			_sequence.Group(Tween.Rotation(_dialogueBoxTransform, _endRotation, _tweenDuration));
		}
	}
}
