using System.Collections.Generic;
using UnityEngine;

public class LevelDialogueTrigger : MonoBehaviour
{
	private void OnEnable()
	{
		GameManager.Instance.OnNewFloor += HandleNewFloor;
		GameManager.Instance.OnStartDescent += HandleStartDescent;
	}

	private void OnDisable()
	{
		if (GameManager.Instance)
		{
			GameManager.Instance.OnNewFloor -= HandleNewFloor;
			GameManager.Instance.OnStartDescent -= HandleStartDescent;
		}
	}

	private void HandleNewFloor()
	{
		int currentFloor = GameManager.Instance.CurrentFloor;
		List<Level> levelsList = GameManager.Instance.LevelsData.LevelsList;

		if (currentFloor < 0 || currentFloor >= levelsList.Count)
		{
			return;
		}

		Level level = levelsList[currentFloor];
		if (string.IsNullOrWhiteSpace(level.OnLoadDialogueKnot))
		{
			return;
		}

		DialogueManager.Instance.DialogueState.StartStory(level.OnLoadDialogueKnot.Trim());
	}

	private void HandleStartDescent()
	{
		DialogueManager.Instance.DialogueState.EndStory();
	}
}
