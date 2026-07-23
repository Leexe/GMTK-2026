using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NpcCountUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _workerCount;

	[SerializeField]
	private TextMeshProUGUI _physCount;

	[SerializeField]
	private TextMeshProUGUI _guardCount;

	private void OnEnable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnNpcUpdate += UpdateUI;
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnNpcUpdate -= UpdateUI;
		}
	}

	private void Start()
	{
		UpdateUI();
	}

	private void UpdateUI()
	{
		Dictionary<NpcRoles, int> npcCount = GameManager.Instance.NpcCount;
		Debug.Log(npcCount[NpcRoles.Worker]);
		_workerCount.text = npcCount[NpcRoles.Worker].ToString();
		_physCount.text = npcCount[NpcRoles.Psychologist].ToString();
		_guardCount.text = npcCount[NpcRoles.Guard].ToString();
	}
}
