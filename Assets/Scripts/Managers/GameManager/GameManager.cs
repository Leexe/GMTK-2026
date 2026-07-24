using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
	[Header("References")]
	[SerializeField]
	public LevelSO LevelsData;

	[SerializeField]
	public RolesSO RolesData;

	[Header("Worker Data")]
	[SerializeField]
	private float _workerEngineMult = 1.5f;

	[Header("SkinWalker Data")]
	[SerializeField]
	private int _skinWalkerKillCount = 2;

	[Header("Engine Data")]
	[SerializeField]
	private float _maxEngineIntegrity = 100f;

	[SerializeField]
	private float _engineMinDeterioration = 5f;

	[SerializeField]
	private float _engineDeteriorateScaling = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _engineDeteriorateVariance = 0.9f;

	[Header("Delays")]
	[SerializeField]
	private float _elevatorDoorCloseDelay = 1f;

	[SerializeField]
	private float _elevatorDescendDelay = 3f;

	private static float _effectTimeScale = 1f; // temp effects

	public static float BaseTimeScale { get; private set; } = 1f;

	public static float SimulationTimeScale { get; private set; } = 1f;

	public static bool IsPaused { get; private set; }

	public Dictionary<NpcRoles, int> NpcCount { private set; get; }
	public List<LevelInstance> LevelInstances { private set; get; }
	public bool NpcsFinishedMoving { get; private set; } = true;
	public float EngineIntegrity { private set; get; }
	public float EngineIntegrityNormalized => EngineIntegrity / _maxEngineIntegrity;
	public int CurrentFloor => _currentFloor;
	public float RunTime;

	private Sequence _timeSlowSequence;
	private int _currentFloor;
	private bool _gameOver;
	private bool _openedDoor;

	// Events

	[HideInInspector]
	public Action OnGameLose;

	[HideInInspector]
	public Action OnGameWin;

	[HideInInspector]
	public Action OnNpcUpdate;

	[HideInInspector]
	public Action OnNewFloor;

	[HideInInspector]
	public Action OnStartDescent;

	[HideInInspector]
	public Action OnEngineUpdate;

	// Unity Events

	protected override void OnInitialized()
	{
		base.OnInitialized();
		InitializeNpcCount();
		InitializeLevelInstances();
	}

	private void Start()
	{
		EngineIntegrity = _maxEngineIntegrity;
		PrintNpcsIdentities();
		OnNewFloor?.Invoke();
		AudioManager.Instance.PlayAmbience("Ambience", FMODEvents.Instance.Ambience_Amb);
	}

	private void InitializeNpcCount()
	{
		NpcCount = new Dictionary<NpcRoles, int>();
		foreach (NpcRoles role in Enum.GetValues(typeof(NpcRoles)))
		{
			NpcCount[role] = 0;
		}
	}

	private void InitializeLevelInstances()
	{
		LevelInstances = new List<LevelInstance>();

		foreach (Level level in LevelsData.LevelsList)
		{
			LevelInstances.Add(new LevelInstance(level, RolesData));
		}
	}

	// Game Logic

	public void SetNpcsFinishedMoving(bool value)
	{
		NpcsFinishedMoving = value;
	}

	public void ContinueToNextFloor()
	{
		if (_currentFloor >= LevelsData.LevelsList.Count || _gameOver)
		{
			return;
		}

		if (_openedDoor && !NpcsFinishedMoving)
		{
			Debug.Log("Cannot descend: NPCs are still moving into position.");
			return;
		}

		AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorClose_Sfx);
		Tween.Delay(
			_elevatorDescendDelay,
			() => AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorDescend_Sfx)
		);

		_currentFloor++;
		_openedDoor = false;
		NpcsFinishedMoving = true;
		OnNewFloor?.Invoke();
		if (CheckWinCondition())
		{
			return;
		}
		HandleWorkers();
		if (EngineDeteriorate())
		{
			return;
		}
		PrintNpcsIdentities();
	}

	public void AcceptNpcs()
	{
		if (_currentFloor < LevelsData.LevelsList.Count && !_openedDoor)
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorOpen_Sfx);

			_openedDoor = true;
			NpcsFinishedMoving = false;
			foreach (KeyValuePair<NpcRoles, int> kvp in LevelInstances[_currentFloor].NpcGuaranteedSpawns)
			{
				NpcCount[kvp.Key] += kvp.Value;
				if (kvp.Value > 0)
				{
					Debug.Log($"Accepted NPC Identity: +{kvp.Value} {kvp.Key}");
				}
			}
			HandleSkinWalkers();
			OnNpcUpdate?.Invoke();
		}
	}

	private void PrintNpcsIdentities()
	{
		if (_currentFloor < 0 || _currentFloor >= LevelInstances.Count)
		{
			return;
		}

		Debug.Log($"Arriving at Floor {_currentFloor}:");
		foreach (KeyValuePair<NpcRoles, int> kvp in LevelInstances[_currentFloor].NpcGuaranteedSpawns)
		{
			for (int i = 0; i < kvp.Value; i++)
			{
				Debug.Log($"NPC Identity: {kvp.Key}");
			}
		}
	}

	private void HandleWorkers()
	{
		float workerGain = NpcCount[NpcRoles.Worker] * _workerEngineMult;
		Debug.Log($"Engine Repaired +{workerGain}");
		EngineIntegrity = Mathf.Clamp(EngineIntegrity + workerGain, 0, _maxEngineIntegrity);
		OnEngineUpdate?.Invoke();
	}

	private void HandleSkinWalkers()
	{
		int skinWalkerCount = NpcCount[NpcRoles.Skinwalker];
		if (skinWalkerCount <= 0)
		{
			return;
		}

		// Kill Guards If Any
		if (NpcCount[NpcRoles.Guard] > 0)
		{
			NpcCount[NpcRoles.Guard]--;
		}
		// Kill Other Npcs, If No Guard
		else
		{
			int totalKillsNeeded = skinWalkerCount * _skinWalkerKillCount;
			while (totalKillsNeeded > 0)
			{
				var availableVictims = new List<NpcRoles>();
				foreach (KeyValuePair<NpcRoles, int> kvp in NpcCount)
				{
					if (kvp.Key != NpcRoles.Skinwalker && kvp.Value > 0)
					{
						for (int i = 0; i < kvp.Value; i++)
						{
							availableVictims.Add(kvp.Key);
						}
					}
				}

				if (availableVictims.Count == 0)
				{
					OnGameLose?.Invoke();
					Debug.Log("Game Lose");
					_gameOver = true;
					return;
				}

				int randomIndex = UnityEngine.Random.Range(0, availableVictims.Count);
				NpcRoles victimRole = availableVictims[randomIndex];
				NpcCount[victimRole]--;
				totalKillsNeeded--;
			}
		}

		// Clear Skin Walkers
		NpcCount[NpcRoles.Skinwalker] = 0;
	}

	private bool EngineDeteriorate()
	{
		float maxDeterioration = _engineMinDeterioration + (_currentFloor * _engineDeteriorateScaling);
		float minDeterioration = maxDeterioration * _engineDeteriorateVariance;
		int deteriorateAmount = Mathf.RoundToInt(UnityEngine.Random.Range(minDeterioration, maxDeterioration));
		Debug.Log($"Engine Damaged -{deteriorateAmount}");
		EngineIntegrity = Mathf.Clamp(EngineIntegrity - deteriorateAmount, 0, _maxEngineIntegrity);
		if (CheckLoseCondition())
		{
			return true;
		}
		OnEngineUpdate?.Invoke();
		return false;
	}

	private bool CheckWinCondition()
	{
		if (_currentFloor == LevelsData.LevelsList.Count && !_gameOver)
		{
			Debug.Log("Won Game");
			OnGameWin?.Invoke();
			_gameOver = true;
			return true;
		}

		return false;
	}

	private bool CheckLoseCondition()
	{
		if (EngineIntegrity <= 0 && !_gameOver)
		{
			Debug.Log("Lost Game");
			OnGameLose?.Invoke();
			_gameOver = true;
			return true;
		}

		return false;
	}

	// Cursor

	public static void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public static void UnlockCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	// Time

	public static void SetPaused(bool val)
	{
		IsPaused = val;
		ApplyTime();
	}

	public static void SetBaseTimeScale(float val)
	{
		BaseTimeScale = val;
		ApplyTime();
	}

	public static void SetSimulationTimeScale(float val)
	{
		SimulationTimeScale = val;
		ApplyTime();
	}

	private static void ApplyTime()
	{
		Time.timeScale = IsPaused ? 0f : BaseTimeScale * SimulationTimeScale * _effectTimeScale;
	}
}
