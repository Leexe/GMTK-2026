using System;
using System.Collections.Generic;
using System.Linq;
using Animancer;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
	[Header("References")]
	public LevelSO LevelsData;
	public RolesSO RolesData;
	public NpcDialogueSO NpcDialogueData;
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
	private float _transitionDelay = 1f;

	[SerializeField]
	private float _engineRepairDelay = 5f;

	[SerializeField]
	private float _guardDelay = 5f;

	[SerializeField]
	private float _skinWalkerDelay = 5f;

	[Header("Elevator")]
	[SerializeField]
	private HoverTarget[] _forceHovers;

	[SerializeField]
	private AnimancerComponent _grateAnimancer;

	[SerializeField]
	private AnimancerComponent _elevatorAnimancer;

	[SerializeField]
	private AnimationClip _grateOpenAnim;

	[SerializeField]
	private AnimationClip _grateCloseAnim;

	[SerializeField]
	private AnimationClip _doorOpenAnim;

	[SerializeField]
	private AnimationClip _doorCloseAnim;

	private static float _effectTimeScale = 1f; // temp effects

	public static float BaseTimeScale { get; private set; } = 1f;
	public static float SimulationTimeScale { get; private set; } = 1f;
	public static bool IsPaused { get; private set; }

	public List<Person> PeopleOnElevator { get; private set; } = new();
	public World WorldState { get; private set; } = new();

	public bool NpcsFinishedMoving { get; private set; } = true;
	public float EngineIntegrity { private set; get; }
	public float EngineIntegrityNormalized => EngineIntegrity / _maxEngineIntegrity;
	public int CurrentFloor => _currentFloor;
	public float EngineRepairDelay => _engineRepairDelay;
	public float GuardDelay => _guardDelay;
	public float SkinWalkerDelay => _skinWalkerDelay;
	public bool OpenedDoor => _openedDoor;
	public bool DescendButtonPressed => _descendButtonPressed;

	private float _runTime;
	private Sequence _timeSlowSequence;
	private Sequence _descentSequence;
	private int _currentFloor;
	private bool _gameOver;
	private bool _openedDoor;
	private bool _descendButtonPressed;

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
	public Action<bool> OnGuardsMove;

	[HideInInspector]
	public Action OnSkinWalkersAct;

	[HideInInspector]
	public Action OnSkinWalkersActEnd;

	[HideInInspector]
	public Action OnNpcsArrived;

	[HideInInspector]
	public Action OnEngineFix;

	[HideInInspector]
	public Action OnEngineDamage;

	// Unity Events

	protected override void OnInitialized()
	{
		base.OnInitialized();
		InitializeWorld();
		GameSetup.SetupSettings();

		PeopleOnElevator = new();
		EngineIntegrity = _maxEngineIntegrity;
		PrimeTweenConfig.warnEndValueEqualsCurrent = false;
	}

	private void Start()
	{
		PrimeTweenConfig.warnZeroDuration = false;
		OnNewFloor?.Invoke();
		_elevatorAnimancer.Play(_doorOpenAnim);

		foreach (HoverTarget t in _forceHovers)
		{
			t.SetForceHovered(true);
		}
	}

	private void InitializeWorld()
	{
		WorldState.Generate(LevelsData, PersonData, RolesData);
	}

	private void OnEnable()
	{
		OnNewFloor += CheckWinCondition;
	}

	private void OnDisable()
	{
		_descentSequence.Stop();
	}

	public int CountNPCs(NpcRoles? role = null, bool includeSkinwalkers = true)
	{
		if (role == NpcRoles.Skinwalker)
		{
			return PeopleOnElevator.Count(p => p.IsSkinwalker && includeSkinwalkers);
		}
		return PeopleOnElevator.Count(p =>
			(!p.IsSkinwalker || includeSkinwalkers) && (!role.HasValue || p.Role == role.Value)
		);
	}

	public int KillRandomNpcs(int amount, NpcRoles? role = null, bool includeSkinwalkers = true)
	{
		List<Person> pool;

		if (role == NpcRoles.Skinwalker)
		{
			pool = PeopleOnElevator.Where(p => p.IsSkinwalker && includeSkinwalkers).ToList();
		}
		else
		{
			pool = PeopleOnElevator
				.Where(p => (!p.IsSkinwalker || includeSkinwalkers) && (!role.HasValue || p.Role == role.Value))
				.ToList();
		}

		for (int i = 0; i < amount; i++)
		{
			if (pool.Count == 0)
			{
				return i;
			}
			Person selection = pool[UnityEngine.Random.Range(0, pool.Count)];
			pool.Remove(selection);
			PeopleOnElevator.Remove(selection);
		}

		return amount;
	}

	// Game Logic

	public void SetNpcsFinishedMoving(bool value)
	{
		NpcsFinishedMoving = value;
		if (value)
		{
			OnNpcsArrived?.Invoke();
		}
	}

	public void ContinueToNextFloor()
	{
		if (_currentFloor >= LevelsData.LevelsList.Count || _gameOver)
		{
			return;
		}

		if (_openedDoor && !NpcsFinishedMoving && !_descendButtonPressed)
		{
			Debug.Log("Cannot descend: NPCs are still moving into position.");
			return;
		}

		if (_descendButtonPressed)
		{
			return;
		}

		// un-force-hover
		foreach (HoverTarget t in _forceHovers)
		{
			t.SetForceHovered(false);
		}

		float closeDelay = 0f;
		_descendButtonPressed = true;
		if (_openedDoor)
		{
			closeDelay = _elevatorDoorCloseDelay;
			OnStartDoorClose?.Invoke();
		}
		else
		{
			AudioManager.Instance.PlayOneShot(FMODEvents.Instance.ElevatorShortBuzz_Sfx);
		}

		_descentSequence.Stop();
		_descentSequence = Sequence.Create();

		// Trigger Start Descent Event
		if (_openedDoor)
		{
			_descentSequence.ChainCallback(() => _grateAnimancer.Play(_grateCloseAnim));
			// _descentSequence.ChainDelay(_grateCloseAnim.length + 0.2f);
		}

		_descentSequence.ChainCallback(() => _elevatorAnimancer.Play(_doorCloseAnim));
		_descentSequence.ChainDelay(_doorCloseAnim.length + 0.2f);
		_descentSequence.Chain(Tween.Delay(closeDelay, () => OnStartDescent?.Invoke()));

		// Engine Deteriorates
		_descentSequence.Chain(Tween.Delay(_transitionDelay, () => EngineDeteriorate()));
		if (_gameOver)
		{
			return;
		}

		// Workers Repair Engine
		int workerCount = CountNPCs(NpcRoles.Worker, includeSkinwalkers: false);
		if (workerCount > 0)
		{
			_descentSequence.Chain(Tween.Delay(_engineRepairDelay / 2));
			_descentSequence.ChainCallback(() => HandleWorkers());
			_descentSequence.ChainDelay(_engineRepairDelay / 2);
		}

		// Guards Move
		bool doesSkinWalkerAct = DoesSkinWalkerAct();
		int guardCount = CountNPCs(NpcRoles.Guard, includeSkinwalkers: false);
		if (guardCount > 0 && doesSkinWalkerAct)
		{
			_descentSequence.ChainCallback(() => HandleGuards(true));
			_descentSequence.ChainDelay(_guardDelay / 2);
		}
		else if (guardCount > 0 && !doesSkinWalkerAct)
		{
			_descentSequence.ChainCallback(() => HandleGuards(false));
			_descentSequence.ChainDelay(_guardDelay);
		}

		// Skinwalker Acts
		if (doesSkinWalkerAct)
		{
			_descentSequence.ChainCallback(() => OnSkinWalkersAct?.Invoke());
			_descentSequence.Chain(Tween.Delay(_skinWalkerDelay, () => SkinWalkersActs()));
		}
		if (_gameOver)
		{
			return;
		}

		// Arrive At Next Floor
		_descentSequence.Chain(Tween.Delay(_transitionDelay, () => ArriveAtNextFloor()));
	}

	private void ArriveAtNextFloor()
	{
		if (CheckLoseCondition())
		{
			return;
		}

		_currentFloor++;
		_openedDoor = false;
		NpcsFinishedMoving = true;
		OnNewFloor?.Invoke();
		_descendButtonPressed = false;

		_elevatorAnimancer.Play(_doorOpenAnim);
	}

	public void AcceptNpcs()
	{
		if (_currentFloor < LevelsData.LevelsList.Count && !_openedDoor)
		{
			_openedDoor = true;
			NpcsFinishedMoving = false;

			PeopleOnElevator.AddRange(WorldState.Floors[_currentFloor].People);

			OnStartDoorOpen?.Invoke();

			_grateAnimancer.Play(_grateOpenAnim);
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

	private void HandleGuards(bool doesSkinWalkerAct)
	{
		OnGuardsMove?.Invoke(doesSkinWalkerAct);
	}

	private void HandleWorkers()
	{
		int realWorkerCount = CountNPCs(NpcRoles.Worker, includeSkinwalkers: false);
		float workerGain = realWorkerCount * _workerEngineMult;
		Debug.Log($"Engine Repaired +{workerGain}");
		EngineIntegrity = Mathf.Clamp(EngineIntegrity + workerGain, 0, _maxEngineIntegrity);
		OnEngineFix?.Invoke();
		OnEngineUpdate?.Invoke();
	}

	private bool DoesSkinWalkerAct()
	{
		int skinWalkerCount = CountNPCs(NpcRoles.Skinwalker);
		float actChance = _skinWalkerActChance * skinWalkerCount;
		return UnityEngine.Random.value <= actChance;
	}

	private void SkinWalkersActs()
	{
		if (CountNPCs(NpcRoles.Skinwalker) <= 0)
		{
			return;
		}

		// skinwalkers kill one guard if there is one
		int guardsKilled = KillRandomNpcs(1, NpcRoles.Guard, includeSkinwalkers: false);

		// if there was no guard, more people die
		if (guardsKilled == 0)
		{
			int skinWalkers = KillRandomNpcs(67, NpcRoles.Skinwalker); // remove all skinwalkers
			int peopleKills = skinWalkers * _skinWalkerKillCount;

			KillRandomNpcs(peopleKills);

			if (PeopleOnElevator.Count == 0)
			{
				OnGameLose?.Invoke();
				Debug.Log("Game Lose");
				_gameOver = true;
				return;
			}
		}

		OnNpcUpdate?.Invoke();
		OnSkinWalkersActEnd?.Invoke();
	}

	private void EngineDeteriorate()
	{
		float maxDeterioration = _engineMinDeterioration + (_currentFloor * _engineDeteriorateScaling);
		float minDeterioration = maxDeterioration * _engineDeteriorateVariance;
		int deteriorateAmount = Mathf.RoundToInt(UnityEngine.Random.Range(minDeterioration, maxDeterioration));
		Debug.Log($"Engine Damaged -{deteriorateAmount}");
		EngineIntegrity = Mathf.Clamp(EngineIntegrity - deteriorateAmount, 0, _maxEngineIntegrity);
		OnEngineDamage?.Invoke();
		OnEngineUpdate?.Invoke();
	}

	private void CheckWinCondition()
	{
		if (_currentFloor >= LevelsData.LevelsList.Count && !_gameOver)
		{
			_gameOver = true;
			_descentSequence.Stop();
			OnGameWin?.Invoke();
		}
	}

	private bool CheckLoseCondition()
	{
		if (EngineIntegrity <= 0 && !_gameOver)
		{
			_gameOver = true;
			_descentSequence.Stop();
			OnGameLose?.Invoke();
			Debug.Log("Lost");
			return true;
		}

		return false;
	}

	public void RestartGame()
	{
		_descentSequence.Stop();
		_timeSlowSequence.Stop();
		AudioManager.Instance.StopMusic(false);

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

	// Debug

	[Button]
	private void TriggerWin()
	{
		OnGameWin?.Invoke();
	}

	[Button]
	private void TriggerLose()
	{
		OnGameLose?.Invoke();
	}
}
