using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class GameSfxController : MonoBehaviour
{
	[SerializeField]
	private float _engineLowThreshold = 0.25f;

	private EventInstance _elevatorDescendInstance;
	private EventInstance _engineRunningInstance;
	private EventInstance _engineLowInstance;

	private void Start()
	{
		AudioManager.Instance.PlayAmbience("Ambience", FMODEvents.Instance.Ambience_Amb);
		InitLoopInstances();
		HandleEngineUpdate();
	}

	private void OnEnable()
	{
		GameManager.Instance.OnStartDoorClose += HandleStartDoorClose;
		GameManager.Instance.OnStartDescent += HandleStartDescent;
		GameManager.Instance.OnStartDoorOpen += HandleStartDoorOpen;
		GameManager.Instance.OnNewFloor += HandleNewFloor;
		GameManager.Instance.OnEngineUpdate += HandleEngineUpdate;
		GameManager.Instance.OnSkinWalkersAct += HandleSkinWalkersAct;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStartDoorClose -= HandleStartDoorClose;
			GameManager.Instance.OnStartDescent -= HandleStartDescent;
			GameManager.Instance.OnStartDoorOpen -= HandleStartDoorOpen;
			GameManager.Instance.OnNewFloor -= HandleNewFloor;
			GameManager.Instance.OnEngineUpdate -= HandleEngineUpdate;
			GameManager.Instance.OnSkinWalkersAct -= HandleSkinWalkersAct;
		}

		StopLoopInstances();
	}

	private void OnDestroy()
	{
		CleanUpLoopInstances();
	}

	private void InitLoopInstances()
	{
		_elevatorDescendInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.ElevatorDescend_LoopSfx);
		_engineRunningInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.EngineRunning_LoopSfx);
		_engineLowInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.EngineLow_LoopSfx);
	}

	private void HandleEngineUpdate()
	{
		float health = GameManager.Instance.EngineIntegrityNormalized;

		if (health <= _engineLowThreshold && health > 0f)
		{
			StopInstance(ref _engineRunningInstance);
			PlayInstance(ref _engineLowInstance);
		}
		else if (health > _engineLowThreshold)
		{
			StopInstance(ref _engineLowInstance);
			PlayInstance(ref _engineRunningInstance);
		}
		else
		{
			StopInstance(ref _engineRunningInstance);
			StopInstance(ref _engineLowInstance);
		}
	}

	private void HandleStartDoorClose()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorClose_Sfx);
	}

	private void HandleStartDescent()
	{
		PlayInstance(ref _elevatorDescendInstance);
	}

	private void HandleNewFloor()
	{
		StopInstance(ref _elevatorDescendInstance);
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorArrive_Sfx);
	}

	private void HandleStartDoorOpen()
	{
		StopInstance(ref _elevatorDescendInstance);
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorOpen_Sfx);
	}

	private void HandleSkinWalkersAct()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SkinwalkerEncounter_Sfx);
	}

	private void PlayInstance(ref EventInstance instance)
	{
		if (!instance.isValid()) return;

		instance.getPlaybackState(out PLAYBACK_STATE state);
		if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
		{
			instance.start();
		}
	}

	private void StopInstance(ref EventInstance instance, bool allowFadeOut = true)
	{
		if (!instance.isValid()) return;

		instance.getPlaybackState(out PLAYBACK_STATE state);
		if (state != PLAYBACK_STATE.STOPPED && state != PLAYBACK_STATE.STOPPING)
		{
			instance.stop(allowFadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
		}
	}

	private void StopLoopInstances()
	{
		StopInstance(ref _elevatorDescendInstance);
		StopInstance(ref _engineRunningInstance);
		StopInstance(ref _engineLowInstance);
	}

	private void CleanUpLoopInstances()
	{
		DestroyInstance(ref _elevatorDescendInstance);
		DestroyInstance(ref _engineRunningInstance);
		DestroyInstance(ref _engineLowInstance);
	}

	private void DestroyInstance(ref EventInstance instance)
	{
		if (!instance.isValid()) return;

		instance.stop(STOP_MODE.IMMEDIATE);
		instance.release();
		instance = default;
	}
}
