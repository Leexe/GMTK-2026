using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonGenInfoSO", menuName = "ScriptableObjects/PersonGenInfoSO", order = 0)]
public class PersonGenInfoSO : ScriptableObject
{
    [TextArea, Tooltip("Space-separated")]
    public string FirstNameSourceList;

    [TextArea, Tooltip("Space-separated")]
    public string LastNameSourceList;

    public float HeightAverage = 70f;
    public float HeightDeviation = 3f;

    [TextArea]
    public string QNASourceList;

    // 

    private List<string> _firstNames = null;
    private List<string> _lastNames = null;
    private List<QnA> _qnas;

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

    public List<QnA> GetQNAs()
    {
        if (_qnas == null)
        {
            _qnas = new();
            var allLines = QNASourceList.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            for (int i = 0; i < allLines.Count; i += 3)
            {
                _qnas.Add(new()
                {
                    Question = ParseQNAEntry(allLines[i]),
                    Response = ParseQNAEntry(allLines[i+1]),
                    BadResponse = ParseQNAEntry(allLines[i+2])
                });
            }
        }

        return _qnas;
    }

    // format for a QNA is:
    // q: option1|option2|option3...
    // n: option1|option2|option3...
    // a: option1|option2|option3...
    private string ParseQNAEntry(string line)
    {
        string afterColon = line.Split(':')[1];
        string[] options = afterColon.Split('|');
        return options[Random.Range(0, options.Length)];
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

    public List<QnA> RandomQnA(int count)
    {
        List<QnA> qnas = GetQNAs();
        count = Mathf.Min(count, qnas.Count());

        List<QnA> picks = new();

        for (int i = 0; i < count; i ++)
        {
            int idx  = Random.Range(0, qnas.Count());
            while (picks.Contains(qnas[idx]))
            {
                idx = Random.Range(0, qnas.Count());
            }
            picks.Add(qnas[idx]);
        }

        return picks;
    }
}