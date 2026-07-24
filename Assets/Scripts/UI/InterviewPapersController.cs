using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

// handles showing/hiding of interview paper

public class InterviewPapersController : MonoBehaviour
{
	public InterviewPaper Paper;
	public NpcSpawner NpcSpawner; // for click info

	[Header("Options")]
	public Vector2 ShownPosition;
	public Vector2 HiddenPosition;

	public float AnimDuration = 1f;
	public float FastAnimDuration = 0.2f;

	private Sequence _activeSequence;

	private readonly Dictionary<Person, InterviewResponses> _interviewResponses = new();

	private InterviewResponses _currentShownInfo = null;
	private InterviewResponses _infoToShow = null;

	/** Unity Messages **/

	public void OnEnable()
	{
		NpcSpawner.OnNpcClicked += HandleNpcClicked;
		GameManager.Instance.OnStartDoorOpen += ClearResponses;
		GameManager.Instance.OnStartDescent += ClearResponses;
	}

	public void OnDisable()
	{
		if (NpcSpawner != null)
		{
			NpcSpawner.OnNpcClicked -= HandleNpcClicked;
		}
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStartDoorOpen -= ClearResponses;
			GameManager.Instance.OnStartDescent -= ClearResponses;
		}

		_activeSequence.Stop();
	}

	private readonly InterviewResponses _a = new()
	{
		Name = "Victor Wembanyama",
		Role = NpcRoles.Psychologist,
		HeightInches = 90,
		FloorsTheyveBeen = new() { 10, 11 },
		QnA = new()
		{
			new() { Question = "What is Obama's last name?", Response = "Barack" },
			new() { Question = "What day is it?", Response = "Tuesday probably" },
		},
	};

	private readonly InterviewResponses _b = new()
	{
		Name = "Bob Wolfeschlegelsteinhausenbergerdorff",
		Role = NpcRoles.Worker,
		HeightInches = 67,
		FloorsTheyveBeen = new() { 5 },
		QnA = new()
		{
			new() { Question = "What is your last name?", Response = "Wolfeschlegelsteinhausenbergerdorff" },
			new() { Question = "How are you doing?", Response = "I'm doing alright-ish. I appreciate you asking!" },
		},
	};

	[Button]
	public void ShowA() => ShowInfo(_a);

	[Button]
	public void ShowB() => ShowInfo(_b);

	[Button]
	public void Clear() => HideInfo();

	/** Public Methods **/

	public void ShowInfo(InterviewResponses info)
	{
		_infoToShow = info;
		if (!_activeSequence.isAlive)
		{
			Sync();
		}
	}

	public void HideInfo()
	{
		ShowInfo(null);
	}

	public void ClearResponses()
	{
		HideInfo();
		_interviewResponses.Clear();
	}

	/** Private Helpers **/

	private void HandleNpcClicked(NpcController npc)
	{
		if (!_interviewResponses.ContainsKey(npc.Person))
		{
			int numPsychologists = GameManager.Instance.NpcCount[NpcRoles.Psychologist];
			_interviewResponses[npc.Person] = InterviewResponses.FromPerson(npc.Person, numPsychologists);
		}

		if (_interviewResponses[npc.Person] != _currentShownInfo)
		{
			ShowInfo(_interviewResponses[npc.Person]);
		}
		else
		{
			HideInfo();
		}
	}

	public void Sync()
	{
		if (_infoToShow != _currentShownInfo)
		{
			if (_currentShownInfo != null)
			{
				HideTween(fast: _infoToShow != null);
			}
			else
			{
				ShowTween(_infoToShow);
			}
		}
	}

	private void ShowTween(InterviewResponses info)
	{
		// set all info - just text for now
		Paper.SetInfo(info);
		_currentShownInfo = info;

		float rot = Random.Range(-2f, 2f);

		_activeSequence = Sequence
			.Create()
			.Chain(
				Tween.UIAnchoredPosition(
					Paper.GetComponent<RectTransform>(),
					HiddenPosition,
					ShownPosition,
					AnimDuration,
					Ease.OutCubic
				)
			)
			.Group(Tween.Rotation(Paper.transform, Quaternion.identity, Quaternion.Euler(0f, 0f, rot), AnimDuration))
			.ChainCallback(Sync);
	}

	private void HideTween(bool fast = false)
	{
		float duration = fast ? FastAnimDuration : AnimDuration;

		_currentShownInfo = null;
		_activeSequence = Sequence
			.Create()
			.Chain(
				Tween.UIAnchoredPosition(
					Paper.GetComponent<RectTransform>(),
					ShownPosition,
					HiddenPosition,
					duration,
					Ease.OutCubic
				)
			)
			.ChainCallback(Sync);
	}
}
