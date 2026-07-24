using UnityEngine;

public class GameSfxController : MonoBehaviour
{
	private void Start()
	{
		AudioManager.Instance.PlayAmbience("Ambience", FMODEvents.Instance.Ambience_Amb);
	}

	private void OnEnable()
	{
		GameManager.Instance.OnStartDoorClose += HandleStartDoorClose;
		GameManager.Instance.OnStartDescent += HandleStartDescent;
		GameManager.Instance.OnStartDoorOpen += HandleStartDoorOpen;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStartDoorClose -= HandleStartDoorClose;
			GameManager.Instance.OnStartDescent -= HandleStartDescent;
			GameManager.Instance.OnStartDoorOpen -= HandleStartDoorOpen;
		}
	}

	private void HandleStartDoorClose()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorClose_Sfx);
	}

	private void HandleStartDescent()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorDescend_Sfx);
	}

	private void HandleStartDoorOpen()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorOpen_Sfx);
	}
}
