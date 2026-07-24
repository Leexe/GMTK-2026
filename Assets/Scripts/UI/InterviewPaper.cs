using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// fills in interview results

public class InterviewPaper : MonoBehaviour
{
    public TMP_Text NameText;
    public TMP_Text RoleText;
    public TMP_Text HeightText;
    public TMP_Text NotesText;
    public Image Mugshot;

    public RectTransform ZonesVisual;
    public Image ZoneItem; // will be disabled and then cloned.

    /** Public Methods **/

    public void SetInfo(InterviewResponses info)
    {
        NameText.text = info.Name;
        RoleText.text = info.Role.ToString();
        HeightText.text = InchesToString(info.HeightInches);
        NotesText.text = QuestionsToString(info.QnA);
        BuildZoneList(info.FloorsTheyveBeen);
    }

    /** Private Helpers **/

    private void BuildZoneList(List<int> visited)
    {
        // clear existing gameobjects
        for (int i = ZonesVisual.childCount - 1; i >= 0; i--)
        {
            Transform tr = ZonesVisual.GetChild(i);
            if (tr != ZoneItem.transform)
            {
                Destroy(tr.gameObject);
            }
        }

        ZoneItem.gameObject.SetActive(false);

        if (visited != null)
        {
            for (int i = 1; i <= 10; i++)
            {
                Image img = Instantiate(ZoneItem);
                if (visited.Contains(i))
                {
                    img.color = Color.red;
                }
                img.transform.SetParent(ZonesVisual);
                img.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString();
                img.gameObject.SetActive(true);
            }
        }
    }

    private static string InchesToString(int inches)
    {
        return $"{inches / 12}' {inches % 12}\"";
    }

    private static string QuestionsToString(List<QnA> questions)
    {
        return string.Join("\n\n\n", questions.Select(q => $"Q: {q.Question}\n\nA: {q.Response}"));
    }
}
