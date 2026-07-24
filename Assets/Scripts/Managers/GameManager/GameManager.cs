using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using PrimeTween;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
	[Header("References")]
	public LevelSO LevelsData;
	public RolesSO RolesData;
	public PersonGenInfoSO PersonData;

	[Header("Worker Data")]
	[SerializeField]
	private float _workerEngineMult = 1.5f;

	[Header("SkinWalker Data")]
	[SerializeField]
	private int _skinWalkerKillCount = 2;

	[SerializeField]
	[Range(0f, 1f)]
	private float _skinWalkerActChance = 0.5f;

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
	private float _elevatorOpenDelay = 2f;

	[SerializeField]
	private float _elevatorDoorCloseDelay = 1f;

	[SerializeField]
	private float _elevatorDescendDelay = 3f;

	[SerializeField]
	private float _transitionDelay = 1f;

	private static float _effectTimeScale = 1f; // temp effects

	public static float BaseTimeScale { get; private set; } = 1f;

	public static float SimulationTimeScale { get; private set; } = 1f;

	public static bool IsPaused { get; private set; }

	public Dictionary<NpcRoles, int> NpcCount { private set; get; }

	// public List<LevelInstance> LevelInstances { private set; get; }
	public World WorldState { get; private set; } = new();

	public bool NpcsFinishedMoving { get; private set; } = true;
	public float EngineIntegrity { private set; get; }
	public float EngineIntegrityNormalized => EngineIntegrity / _maxEngineIntegrity;
	public int CurrentFloor => _currentFloor;

	private float _runTime;
	private Sequence _timeSlowSequence;
	private Sequence _descentSequence;
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
	public Action OnStartDoorOpen;

	[HideInInspector]
	public Action OnFinishedDoorOpen;

	[HideInInspector]
	public Action OnStartDoorClose;

	[HideInInspector]
	public Action OnStartDescent;

	[HideInInspector]
	public Action OnEngineUpdate;

	[HideInInspector]
	public Action OnSkinWalkersAct;

	// Unity Events

	protected override void OnInitialized()
	{
		base.OnInitialized();
		InitializeWorld();

		NpcCount = new();
		foreach (NpcRoles role in Enum.GetValues(typeof(NpcRoles)))
		{
			NpcCount[role] = 0;
		}
	}

	private void Start()
	{
		EngineIntegrity = _maxEngineIntegrity;
		PrimeTweenConfig.warnZeroDuration = false;
		OnNewFloor?.Invoke();
	}

	private void InitializeWorld()
	{
		WorldState.Generate(LevelsData, PersonData, RolesData);
	}

	private void OnDisable()
	{
		_descentSequence.Stop();
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

		float closeDelay = 0f;
		if (_openedDoor)
		{
			closeDelay = _elevatorDoorCloseDelay;
			OnStartDoorClose?.Invoke();
		}

		_descentSequence.Stop();
		_descentSequence = Sequence.Create();

		// Door close Sfx
		_descentSequence.Chain(Tween.Delay(closeDelay, () => OnStartDescent?.Invoke()));

		// Skinwalker Acts
		if (DoesSkinWalkerAct())
		{
			_descentSequence.Chain(Tween.Delay(_transitionDelay, () => SkinWalkersActs()));
		}
		if (_gameOver)
		{
			return;
		}

		// Workers Repair Engine
		if (NpcCount[NpcRoles.Worker] > 0)
		{
			_descentSequence.Chain(Tween.Delay(_transitionDelay, () => HandleWorkers()));
		}

		// Engine Deteriorates
		_descentSequence.Chain(Tween.Delay(_transitionDelay, () => EngineDeteriorate()));
		if (_gameOver)
		{
			return;
		}

		// Arrive At Next Floor
		_descentSequence.Chain(Tween.Delay(_transitionDelay, () => ArriveAtNextFloor()));
	}

	private void ArriveAtNextFloor()
	{
		_currentFloor++;
		_openedDoor = false;
		NpcsFinishedMoving = true;
		OnNewFloor?.Invoke();
		if (CheckWinCondition())
		{
			return;
		}
	}

	public void AcceptNpcs()
	{
		if (_currentFloor < LevelsData.LevelsList.Count && !_openedDoor)
		{
			_openedDoor = true;
			NpcsFinishedMoving = false;

			foreach (Person p in WorldState.Floors[_currentFloor].People)
			{
				if (p.IsSkinwalker)
				{
					NpcCount[NpcRoles.Skinwalker]++;
				}
				else
				{
					NpcCount[p.Role]++;
				}
			}

			OnStartDoorOpen?.Invoke();

			Tween.Delay(
				_elevatorOpenDelay,
				() =>
				{
					OnFinishedDoorOpen?.Invoke();
					OnNpcUpdate?.Invoke();
				}
			);
		}
	}

	private void HandleWorkers()
	{
		float workerGain = NpcCount[NpcRoles.Worker] * _workerEngineMult;
		Debug.Log($"Engine Repaired +{workerGain}");
		EngineIntegrity = Mathf.Clamp(EngineIntegrity + workerGain, 0, _maxEngineIntegrity);
		OnEngineUpdate?.Invoke();
	}

	private bool DoesSkinWalkerAct()
	{
		int skinWalkerCount = NpcCount[NpcRoles.Skinwalker];
		float actChance = _skinWalkerActChance * skinWalkerCount;
		return UnityEngine.Random.value <= actChance;
	}

	private void SkinWalkersActs()
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
			Debug.Log("Killed Guard");
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
				Debug.Log($"Killed {victimRole}");
				NpcCount[victimRole]--;
				totalKillsNeeded--;
			}
		}

		// Clear Skin Walkers
		NpcCount[NpcRoles.Skinwalker] = 0;

		OnNpcUpdate?.Invoke();
		OnSkinWalkersAct?.Invoke();
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
		if (_currentFloor >= LevelsData.LevelsList.Count && !_gameOver)
		{
			_gameOver = true;
			_descentSequence.Stop();
			OnGameWin?.Invoke();
			return true;
		}

		return false;
	}

	private bool CheckLoseCondition()
	{
		if (EngineIntegrity <= 0 && !_gameOver)
		{
			_gameOver = true;
			_descentSequence.Stop();
			OnGameLose?.Invoke();
			return true;
		}

		return false;
	}

	public void RestartGame()
	{
		_descentSequence.Stop();
		_timeSlowSequence.Stop();

		UnityEngine.SceneManagement.SceneManager.LoadScene(
			UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
		);
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
