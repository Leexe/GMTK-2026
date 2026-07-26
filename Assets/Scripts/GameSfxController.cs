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

	[Header("Creepy Sound Settings")]
	[SerializeField]
	private float _minCreepySoundInterval = 10f;

	[SerializeField]
	private float _maxCreepySoundInterval = 30f;

	private EventInstance _elevatorDescendInstance;
	private EventInstance _engineRunningInstance;
	private EventInstance _engineLowInstance;
	private Tween _elevatorBeepDelayTween;
	private Tween _creepySoundTween;
	private Sequence _skinWalkerActSequence;
	private Sequence _gameLoseSequence;

	private void Start()
	{
		AudioManager.Instance.StopMusic(false);
		AudioManager.Instance.PlayAmbience("Ambience", FMODEvents.Instance.Ambience_Amb);
		InitLoopInstances();
		HandleEngineUpdate();
		ScheduleNextCreepySound();
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
		GameManager.Instance.OnGameLose += HandleGameLose;
		GameManager.Instance.OnGameWin += HandleGameWin;
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
			GameManager.Instance.OnGameLose -= HandleGameLose;
			GameManager.Instance.OnGameWin -= HandleGameWin;
		}

		_creepySoundTween.Stop();
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
		_skinWalkerActSequence = Sequence.Create();
		_skinWalkerActSequence.ChainCallback(() =>
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorLightsOut_Sfx)
		);
		_skinWalkerActSequence.ChainCallback(() =>
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CreepySound_Sfx)
		);
		_skinWalkerActSequence.ChainDelay(duration / 4f);
		_skinWalkerActSequence.ChainCallback(() =>
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SkinwalkerEncounter_Sfx)
		);
		_skinWalkerActSequence.ChainDelay(duration / 4f);
		_skinWalkerActSequence.ChainCallback(() => AudioManager.Instance.PlayOneShot(FMODEvents.Instance.NpcDeath_Sfx));
		_skinWalkerActSequence.ChainDelay(duration / 4f);
		if (GameManager.Instance.CountNPCs(NpcRoles.Guard) > 0)
		{
			_skinWalkerActSequence.ChainCallback(() =>
			{
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.SoldierShot_Sfx);
				GameManager.Instance.OnGunShot?.Invoke();
			});
		}
	}

	private void HandleGameLose()
	{
		_skinWalkerActSequence.Stop();
		_elevatorBeepDelayTween.Stop();
		_gameLoseSequence.Stop();
		_creepySoundTween.Stop();
		StopLoopInstances();
		AudioManager.Instance.StopAmbience();
		AudioManager.Instance.PlayMusic(FMODEvents.Instance.Lose_Bgm);

		if (GameManager.Instance.EngineIntegrity <= 0)
		{
			_gameLoseSequence = Sequence
				.Create()
				.ChainCallback(() => AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorLightsOut_Sfx))
				.ChainDelay(0.5f)
				.ChainCallback(() => AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorFalling_Sfx));
		}
		else
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.PlayerDeath_Sfx);
		}
	}

	private void HandleGameWin()
	{
		_skinWalkerActSequence.Stop();
		_elevatorBeepDelayTween.Stop();
		_gameLoseSequence.Stop();
		_creepySoundTween.Stop();
		StopLoopInstances();
		AudioManager.Instance.StopAmbience();
		AudioManager.Instance.PlayMusic(FMODEvents.Instance.Win_Bgm);
	}

	private void ScheduleNextCreepySound()
	{
		_creepySoundTween.Stop();
		float delay = Random.Range(_minCreepySoundInterval, _maxCreepySoundInterval);
		_creepySoundTween = Tween.Delay(
			this,
			delay,
			_ =>
			{
				AudioManager.Instance.PlayOneShot(FMODEvents.Instance.CreepySound_Sfx);
				ScheduleNextCreepySound();
			}
		);
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
