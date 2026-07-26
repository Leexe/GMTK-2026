using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Controls the appearance and disappearance of the dialogue continue icon,
/// listening to typewriter and dialogue state events.
/// </summary>
public class DialogueContinueIcon : MonoBehaviour
{
	[FoldoutGroup("References")]
	[SerializeField]
	private CanvasGroup _canvasGroup;

	[FoldoutGroup("Animation Settings")]
	[SerializeField]
	private float _appearDuration = 0.25f;

	[FoldoutGroup("Animation Settings")]
	[SerializeField]
	private float _disappearDuration = 0.15f;

	private Tween _fadeTween;
	private DialogueState _dialogueState;

	private void Awake()
	{
		SetHiddenImmediate();
	}

	private void OnEnable()
	{
		_dialogueState = DialogueManager.Instance.DialogueState;
		_dialogueState.OnTypewriterFinish += ShowIcon;
		_dialogueState.OnStartDialogue += HideIcon;
		_dialogueState.OnDisplayDialogue += HideIcon;
		_dialogueState.OnEndStory += HideIcon;
	}

	private void OnDisable()
	{
		_dialogueState.OnTypewriterFinish -= ShowIcon;
		_dialogueState.OnStartDialogue -= HideIcon;
		_dialogueState.OnDisplayDialogue -= HideIcon;
		_dialogueState.OnEndStory -= HideIcon;
		StopAllTweens();
	}

	private void HideIcon(string speakerName, string text) => HideIcon();

	[Button]
	public void ShowIcon()
	{
		StopAllTweens();
		_fadeTween = Tween.Alpha(_canvasGroup, 1f, _appearDuration, Ease.Linear);
	}

	[Button]
	public void HideIcon()
	{
		StopAllTweens();
		_fadeTween = Tween.Alpha(_canvasGroup, 0f, _disappearDuration, Ease.Linear);
	}

	private void SetHiddenImmediate()
	{
		StopAllTweens();
		_canvasGroup.alpha = 0f;
	}

	private void StopAllTweens()
	{
		_fadeTween.Stop();
	}
}
