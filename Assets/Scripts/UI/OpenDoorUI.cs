using UnityEngine;

public class OpenDoorUI : MonoBehaviour
{
	public void ButtonClicked()
	{
		GameManager.Instance.AcceptNpcs();
	}
}
