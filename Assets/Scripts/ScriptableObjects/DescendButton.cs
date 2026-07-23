using UnityEngine;

public class DescendButton : MonoBehaviour
{
	public void ButtonClicked()
	{
		GameManager.Instance.ContinueToNextFloor();
	}
}
