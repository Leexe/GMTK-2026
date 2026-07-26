using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcDialogueSO", menuName = "ScriptableObjects/NpcDialogueSO", order = 0)]
public class NpcDialogueSO : ScriptableObject
{
	[SerializeField]
	[TextArea(3, 5)]
	private List<string> _meetText;

	[SerializeField]
	[TextArea(3, 5)]
	private List<string> _acceptText;

	[SerializeField]
	[TextArea(3, 5)]
	private List<string> _rejectText;

	public IReadOnlyList<string> MeetText => _meetText;
	public IReadOnlyList<string> AcceptText => _acceptText;
	public IReadOnlyList<string> RejectText => _rejectText;

	public string GetRandomMeetText() => GetRandom(_meetText);

	public string GetRandomAcceptText() => GetRandom(_acceptText);

	public string GetRandomRejectText()
	{
		string rejectText = GetRandom(_rejectText);
		string modifiedRejectText = "<shake>" + rejectText;
		return modifiedRejectText;
	}

	private string GetRandom(List<string> list)
	{
		if (list == null || list.Count == 0)
		{
			return string.Empty;
		}
		return list[Random.Range(0, list.Count)];
	}
}
