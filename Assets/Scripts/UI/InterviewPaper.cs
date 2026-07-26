using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// fills in interview results

public class InterviewPaper : MonoBehaviour
{
	[Header("Sprites")]
	[SerializeField]
	private Sprite WorkerIconSprite;

	[SerializeField]
	private Sprite PsychologistIconSprite;

	[SerializeField]
	private Sprite GuardIconSprite;

	[SerializeField]
	private Sprite WorkerMugSprite;

	[SerializeField]
	private Sprite PsychologistMugSprite;

	[SerializeField]
	private Sprite GuardMugSprite;

	[Header("References")]
	public TMP_Text NameText;
	public TMP_Text RoleText;
	public Image RoleImage;
	public TMP_Text HeightText;
	public TMP_Text NotesText;
	public Image Mugshot;

	public RectTransform ZonesVisual;
	public Image ZoneItem; // will be disabled and then cloned.

	private InterviewResponses _storedInfo;
	public InterviewResponses StoredInfo => _storedInfo;

	/** Public Methods **/

	public void SetInfo(InterviewResponses info)
	{
		_storedInfo = info;

		NameText.text = info.Name;
		RoleText.text = info.Role.ToString();
		RoleImage.sprite = GetSprite(info.Role);
		Mugshot.sprite = GetMugSprite(info.Role);
		HeightText.text = InchesToString(info.HeightInches);
		NotesText.text = QuestionsToString(info.QnA);
	}

	/** Private Helpers **/

	private Sprite GetSprite(NpcRoles role) =>
		role switch
		{
			NpcRoles.Worker => WorkerIconSprite,
			NpcRoles.Psychologist => PsychologistIconSprite,
			NpcRoles.Guard => GuardIconSprite,
			_ => WorkerIconSprite,
		};

	private Sprite GetMugSprite(NpcRoles role) =>
		role switch
		{
			NpcRoles.Worker => WorkerMugSprite,
			NpcRoles.Psychologist => PsychologistMugSprite,
			NpcRoles.Guard => GuardMugSprite,
			_ => WorkerMugSprite,
		};

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
					img.color = Color.white;
				}
				img.transform.SetParent(ZonesVisual);
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
		return string.Join("\n\n", questions.Select(q => $"Q: {q.Question}\nA: {q.Response}"));
	}
}
