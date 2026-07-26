using FMOD.Studio;
using FMODUnity;
using PrimeTween;
using UnityEngine;

public class GameSfxController : MonoBehaviour
{
	[SerializeField]
	private float _engineLowThreshold = 0.25f;

	[SerializeField]
	private float _elevatorBeepDelay = 0.5f;

	private EventInstance _elevatorDescendInstance;
	private EventInstance _engineRunningInstance;
	private EventInstance _engineLowInstance;
	private Tween _elevatorBeepDelayTween;
	private Sequence _skinWalkerActSequence;

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
		GameManager.Instance.OnSkinWalkersActEnd += HandleSkinWalkersAct;
		GameManager.Instance.OnEngineFix += HandleEngineFix;
		GameManager.Instance.OnEngineDamage += HandleEngineDamage;
		GameManager.Instance.OnSkinWalkersAct += HandleSkinWalkerAct;
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
			GameManager.Instance.OnSkinWalkersActEnd -= HandleSkinWalkersAct;
			GameManager.Instance.OnEngineFix -= HandleEngineFix;
			GameManager.Instance.OnEngineDamage -= HandleEngineDamage;
			GameManager.Instance.OnSkinWalkersAct -= HandleSkinWalkerAct;
		}

		StopLoopInstances();
	}

	private void InitLoopInstances()
	{
		_elevatorDescendInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.ElevatorDescend_LoopSfx);
		_engineRunningInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.EngineRunning_LoopSfx);
		_engineRunningInstance.setVolume(0.2f);
		_engineLowInstance = AudioManager.Instance.CreateInstance(FMODEvents.Instance.EngineLow_LoopSfx);
	}

	private void HandleEngineUpdate()
	{
		float health = GameManager.Instance.EngineIntegrityNormalized;

		if (health <= _engineLowThreshold && health > 0f)
		{
			AudioManager.Instance.PlayInstance(_engineLowInstance);
			AudioManager.Instance.StopInstance(_engineRunningInstance);
		}
		else
		{
			AudioManager.Instance.StopInstance(_engineLowInstance);
			AudioManager.Instance.PlayInstance(_engineRunningInstance);
		}
	}

	private void HandleStartDoorClose()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorClose_Sfx);
	}

	private void HandleStartDescent()
	{
		AudioManager.Instance.PlayInstance(_elevatorDescendInstance);
	}

	private void HandleSkinWalkerAct()
	{
		float duration = GameManager.Instance.SkinWalkerDelay;
		_skinWalkerActSequence = new Sequence();
		_skinWalkerActSequence.ChainCallback(() =>
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorLightsOut_Sfx)
		);
		_skinWalkerActSequence.ChainCallback(() =>
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CreepySound_Sfx)
		);
		_skinWalkerActSequence.ChainDelay(duration / 4);
		_skinWalkerActSequence.ChainCallback(() =>
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SkinwalkerEncounter_Sfx)
		);
		_skinWalkerActSequence.ChainDelay(duration / 4);
		_skinWalkerActSequence.ChainCallback(() => AudioManager.Instance.PlayOneShot(FMODEvents.Instance.NpcDeath_Sfx));
		_skinWalkerActSequence.ChainDelay(duration / 4);
		if (GameManager.Instance.CountNPCs(NpcRoles.Guard) > 0)
		{
			_skinWalkerActSequence.ChainCallback(() =>
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SoldierShot_Sfx)
			);
		}
	}

	private void HandleNewFloor()
	{
		AudioManager.Instance.StopInstance(_elevatorDescendInstance);
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorArrive_Sfx);

		_elevatorBeepDelayTween.Stop();
		_elevatorBeepDelayTween = Tween.Delay(
			this,
			_elevatorBeepDelay,
			_ => AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorBeep_Sfx)
		);
	}

	private void HandleStartDoorOpen()
	{
		AudioManager.Instance.StopInstance(_elevatorDescendInstance);
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorOpen_Sfx);
	}

	private void HandleSkinWalkersAct()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SkinwalkerEncounter_Sfx);
	}

	private void HandleEngineFix()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.EngineFix_Sfx);
	}

	private void HandleEngineDamage()
	{
		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.EngineDamage_Sfx);
	}

	private void StopLoopInstances()
	{
		if (AudioManager.Instance != null)
		{
			AudioManager.Instance.StopInstance(_elevatorDescendInstance);
			AudioManager.Instance.StopInstance(_engineRunningInstance);
			AudioManager.Instance.StopInstance(_engineLowInstance);
		}
	}
}
