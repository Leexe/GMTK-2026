using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonGenInfoSO", menuName = "ScriptableObjects/PersonGenInfoSO", order = 0)]
public class PersonGenInfoSO : ScriptableObject
{
	[TextArea(20, 40), Tooltip("Space-separated")]
	public string FirstNameSourceList;

	[TextArea(20, 40), Tooltip("Space-separated")]
	public string LastNameSourceList;

	public float HeightAverage = 70f;
	public float HeightDeviation = 3f;

	[TextArea(40, 300)]
	public string QNASourceList;

	//

	private List<string> _firstNames = null;
	private List<string> _lastNames = null;
	private List<QnAOption> _qnas;

	//

	public List<string> GetFirstNames()
	{
		if (_firstNames == null || _firstNames.Count == 0)
		{
			_firstNames = new List<string>(FirstNameSourceList.Split(' '));
		}
		return _firstNames;
	}

	public List<string> GetLastNames()
	{
		if (_lastNames == null || _lastNames.Count == 0)
		{
			_lastNames = new List<string>(LastNameSourceList.Split(' '));
		}
		return _lastNames;
	}

	public List<QnAOption> GetQNAs()
	{
		if (_qnas == null)
		{
			_qnas = new();
			var allLines = QNASourceList.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
			for (int i = 0; i < allLines.Count; i += 4)
			{
				_qnas.Add(
					new()
					{
						Role = ParseFilter(allLines[i]),
						Questions = ParseQNAEntry(allLines[i + 1]),
						Responses = ParseQNAEntry(allLines[i + 2]),
						BadResponses = ParseQNAEntry(allLines[i + 3]),
					}
				);
			}
		}

		return _qnas;
	}

	// format for a QNA is:
	// q: option1|option2|option3...
	// n: option1|option2|option3...
	// a: option1|option2|option3...
	private string[] ParseQNAEntry(string line)
	{
		return line.Split(':')[1].Split('|');
	}

	private NpcRoles? ParseFilter(string line)
	{
		// Debug.Log(line);
		return line switch
		{
			"W" => NpcRoles.Worker,
			"P" => NpcRoles.Psychologist,
			"G" => NpcRoles.Guard,
			_ => null,
		};
	}

	//

	public string RandomName()
	{
		List<string> firstNames = GetFirstNames();
		List<string> lastNames = GetLastNames();

		string firstName = firstNames[Random.Range(0, firstNames.Count)];
		string lastName = lastNames[Random.Range(0, lastNames.Count)];

		return $"{firstName} {lastName}";
	}

	public int RandomHeight()
	{
		// normal distribution
		float u1 = Random.value;
		float u2 = Random.value;
		if (u1 == 0)
		{
			u1 = 0.69f;
		}
		if (u2 == 0)
		{
			u2 = 0.69f;
		}
		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
		randStdNormal = Mathf.Clamp(randStdNormal, -5f, 5f);
		float randNormal = HeightAverage + (HeightDeviation * randStdNormal);

		return Mathf.RoundToInt(randNormal);
	}

	public List<QnA> RandomQnA(int count, NpcRoles? role)
	{
		List<QnAOption> qnas = GetQNAs();
		var relevantQNAs = qnas.Where(q => !q.Role.HasValue || !role.HasValue || q.Role.Value == role.Value).ToList();
		count = Mathf.Min(count, qnas.Count());

		List<QnAOption> picks = new();

		for (int i = 0; i < count; i++)
		{
			int idx = Random.Range(0, relevantQNAs.Count());
			while (picks.Contains(relevantQNAs[idx]))
			{
				idx = Random.Range(0, relevantQNAs.Count());
			}
			picks.Add(relevantQNAs[idx]);
		}

		return picks
			.Select(p => new QnA()
			{
				Question = p.Questions[Random.Range(0, p.Questions.Length)],
				Response = p.Responses[Random.Range(0, p.Responses.Length)],
				BadResponse = p.BadResponses[Random.Range(0, p.BadResponses.Length)],
			})
			.ToList();
	}

	public struct QnAOption
	{
		public NpcRoles? Role;
		public string[] Questions;
		public string[] Responses;
		public string[] BadResponses;
	}
}
