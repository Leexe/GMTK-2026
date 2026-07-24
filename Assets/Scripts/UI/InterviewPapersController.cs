using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEditor.Overlays;
using UnityEngine;

// handles showing/hiding of interview paper

public class InterviewPapersController : MonoBehaviour
{
	public InterviewPaper PaperPrefab;

	public InterviewPaper Paper;
	public NpcSpawner NpcSpawner; // for click info

	[Header("Options")]
	public Vector2 ShownPosition;
	public Vector2 HiddenPosition;
	public float Spacing = 300f;

	public float AnimDuration = 1f;
	public float FastAnimDuration = 0.2f;

	private Sequence _activeSequence;

	private readonly Dictionary<Person, InterviewResponses> _interviewResponses = new();

	private List<PaperItem> _paperItems = new();

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

	/** Public Methods **/

	// public void ShowInfo(InterviewResponses info)
	// {
	// 	_infoToShow = info;
	// 	if (!_activeSequence.isAlive)
	// 	{
	// 		Sync();
	// 	}
	// }

	// public void HideInfo()
	// {
	// 	ShowInfo(null);
	// }

	public void ClearResponses()
	{
		// HideInfo();
		// _interviewResponses.Clear();

        foreach (PaperItem item in _paperItems)
        {
            item.ShouldDelete = true;
        }
        Layout();
	}

	/** Private Helpers **/

	private void HandleNpcClicked(NpcController npc)
	{
		Person person = npc.Person;

        Debug.Log(npc.transform.GetSiblingIndex());

		if (!_paperItems.Any(p => p.Data.Source == person))
		{
            Debug.Log("adding");
			AddPaperItem(npc);
		}

		foreach (PaperItem item in _paperItems)
		{
			bool isMatch = item.NpcController.Person == person;
			item.IsLastClicked = isMatch;
			item.IsOut = isMatch;
			// item.IsOut = isMatch ? !item.IsOut : item.IsOut;
		}

		Layout();
	}

	private void AddPaperItem(NpcController npc)
	{
		int numPsychologists = GameManager.Instance.NpcCount[NpcRoles.Psychologist];
		var responses = InterviewResponses.FromPerson(npc.Person, numPsychologists);

		InterviewPaper go = Instantiate(PaperPrefab, transform);
		go.SetInfo(responses);
		go.OwnTransform.anchoredPosition = HiddenPosition;

		_paperItems.Add(
			new()
			{
				Data = responses,
				NpcController = npc,
				PaperGO = go,
			}
		);
	}

	private void Layout()
	{
		// first - delete papers that should be deleted!
		for (int i = _paperItems.Count - 1; i >= 0; i--)
		{
			if (_paperItems[i].ShouldDelete && !_paperItems[i].IsOut && !_paperItems[i].Sequence.isAlive)
			{
				Destroy(_paperItems[i].PaperGO.gameObject);
				_paperItems.RemoveAt(i);
			}
		}

		// sort paperitems (and order their gameobjects)
		_paperItems.Sort(
			(a, b) => (int)Mathf.Sign(a.NpcController.transform.position.x - b.NpcController.transform.position.x)
		);
        for (int i = 0; i < _paperItems.Count; i++)
        {
            _paperItems[i].PaperGO.transform.SetSiblingIndex(i);
        }

		// figure out how many papers are out
		int numpapersOut = _paperItems.Count(p => p.IsOut && !p.ShouldDelete);
		float offset = -0.5f * Spacing * (numpapersOut - 1);
		int paperIdx = 0;

		for (int i = 0; i < _paperItems.Count; i++)
		{
			if (_paperItems[i].IsOut && !_paperItems[i].ShouldDelete)
			{
				float xPos = offset + (paperIdx * Spacing);
				float rotation = -5f * (paperIdx - ((numpapersOut - 1) * 0.5f));
				paperIdx++;
				_paperItems[i].TargetPosition = new Vector2(xPos, ShownPosition.y);
				_paperItems[i].TargetRotation = rotation;
			}
			else
			{
				_paperItems[i].TargetPosition = HiddenPosition;
				_paperItems[i].TargetRotation = 0f;
			}
		}

		for (int i = 0; i < _paperItems.Count; i++)
		{
			_paperItems[i].Sequence.Stop();
			_paperItems[i].Sequence = Sequence
				.Create()
				.Chain(
					Tween.UIAnchoredPosition(_paperItems[i].PaperGO.OwnTransform, _paperItems[i].TargetPosition, 0.3f)
				)
				.Group(
					Tween.Rotation(
						_paperItems[i].PaperGO.OwnTransform,
						Quaternion.Euler(0f, 0f, _paperItems[i].TargetRotation),
						0.3f
					)
				);
		}
	}

	private class PaperItem
	{
		public NpcController NpcController;
		public InterviewResponses Data;
		public InterviewPaper PaperGO;
		public Sequence Sequence;

		public bool IsLastClicked = false;
		public bool IsOut = false;
		public bool ShouldDelete = false;
		public Vector2 TargetPosition;
		public float TargetRotation;
	}
}
