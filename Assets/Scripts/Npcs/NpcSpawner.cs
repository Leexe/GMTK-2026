using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;
using Random = UnityEngine.Random;

public class NpcSpawner : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private GameObject _npcPrefab;

	[SerializeField]
	private List<Transform> _spawnPoints;

	[SerializeField]
	private List<Transform> _restPoints;

	[SerializeField]
	private List<Transform> _goalPoints;

	[SerializeField]
	private List<Transform> _repairPoints;

	[SerializeField]
	private List<Transform> _guardPoints;

	private List<NpcController> _npcPool = new();
	private List<NpcController> _activeNpcs = new();
	private int _arrivedNpcCount;
	private Tween _repairDelayTween;
	private Tween _guardDelayTween;
	private bool _hasGreeted;

	// Events
	public Action OnAllNpcsArrived;
	public event Action<NpcController> OnNpcClicked;

	private void Awake()
	{
		InitializePool();
	}

	private void InitializePool()
	{
		for (int i = 0; i < 10; i++)
		{
			GameObject npc = Instantiate(_npcPrefab, transform);
			NpcController controller = npc.GetComponent<NpcController>();
			controller.DisableVisuals();
			_npcPool.Add(controller);
		}
	}

	private void OnEnable()
	{
		GameManager.Instance.OnNpcUpdate += HandleNpcUpdate;
		GameManager.Instance.OnNewFloor += HandleNewFloor;
		GameManager.Instance.OnStartDoorOpen += HandleDoorOpenDialogue;
		GameManager.Instance.OnStartDescent += HandleStartDescent;
		GameManager.Instance.OnEngineDamage += HandleWorkerRepair;
		GameManager.Instance.OnEngineFix += HandleEngineRepair;
		GameManager.Instance.OnGuardsMove += HandleGuards;
		OnAllNpcsArrived += HandleGreetingDialogue;
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnNpcUpdate -= HandleNpcUpdate;
			GameManager.Instance.OnNewFloor -= HandleNewFloor;
			GameManager.Instance.OnStartDoorOpen -= HandleDoorOpenDialogue;
			GameManager.Instance.OnEngineFix -= HandleEngineRepair;
			GameManager.Instance.OnStartDescent -= HandleStartDescent;
			GameManager.Instance.OnEngineDamage -= HandleWorkerRepair;
			GameManager.Instance.OnGuardsMove -= HandleGuards;
		}
		OnAllNpcsArrived -= HandleGreetingDialogue;
		_repairDelayTween.Stop();
		UnsubscribeFromActiveNpcs();
	}

	public void SetSelectedNpc(NpcController npc)
	{
		foreach (NpcController n in _activeNpcs)
		{
			n.SetSelected(n == npc);
		}
	}

	// Called when OnNewFloor fires: spawns current floor's NPCs at spawn points and immediately moves them to rest points
	private void HandleNewFloor()
	{
		// Clean Up
		ResetNpcs();

		int currentFloor = GameManager.Instance.CurrentFloor;
		if (currentFloor >= GameManager.Instance.WorldState.Floors.Count)
		{
			return;
		}
		List<Person> people = GameManager.Instance.WorldState.Floors[currentFloor].People;

		List<int> availableSpawnIndices = GetShuffledIndices(_spawnPoints.Count);
		List<int> availableRestIndices = GetShuffledIndices(_restPoints.Count);

		for (int i = 0; i < people.Count; i++)
		{
			int spawnIndex = availableSpawnIndices[i % availableSpawnIndices.Count];

			NpcController npc = _npcPool[i];
			Vector3 spawnPos = _spawnPoints[spawnIndex].position;

			npc.Initialize(people[i], spawnPos);
			npc.OnClicked += HandleNpcClicked;
			npc.OnArrivedAtPosition += HandleNpcArrived;
			_activeNpcs.Add(npc);

			int restIndex = availableRestIndices[i % availableRestIndices.Count];
			Vector3 restPos = _restPoints[restIndex].position;
			npc.LerpToPosition(restPos);
		}
	}

	private void ResetNpcs()
	{
		_hasGreeted = false;
		_repairDelayTween.Complete();
		_guardDelayTween.Complete();
		UnsubscribeFromActiveNpcs();
		foreach (NpcController npc in _activeNpcs)
		{
			npc.DisableVisuals();
		}
		_activeNpcs.Clear();
		_arrivedNpcCount = 0;
	}

	// Called when door open button is pressed (OnNpcUpdate), moves active NPCs from rest points to goal points
	private void HandleNpcUpdate()
	{
		if (_activeNpcs.Count == 0)
		{
			HandleAllNpcsArrived();
			return;
		}

		_arrivedNpcCount = 0;
		List<int> availableGoalIndices = GetShuffledIndices(_goalPoints.Count);

		for (int i = 0; i < _activeNpcs.Count; i++)
		{
			NpcController npc = _activeNpcs[i];
			int goalIndex = availableGoalIndices[i % availableGoalIndices.Count];
			Vector3 goalPos = _goalPoints[goalIndex].position;
			npc.LerpToPosition(goalPos, playFootsteps: true);
		}
	}

	private void HandleEngineRepair() { }

	private void HandleNpcArrived(NpcController npc)
	{
		_arrivedNpcCount++;
		if (_arrivedNpcCount >= _activeNpcs.Count)
		{
			HandleAllNpcsArrived();
		}
	}

	private void HandleAllNpcsArrived()
	{
		GameManager.Instance.SetNpcsFinishedMoving(true);
		OnAllNpcsArrived?.Invoke();
	}

	private void UnsubscribeFromActiveNpcs()
	{
		foreach (NpcController npc in _activeNpcs)
		{
			npc.OnClicked -= HandleNpcClicked;
			npc.OnArrivedAtPosition -= HandleNpcArrived;
		}
	}

	private List<int> GetShuffledIndices(int count)
	{
		List<int> indices = new(count);
		for (int i = 0; i < count; i++)
		{
			indices.Add(i);
		}

		for (int i = 0; i < count; i++)
		{
			int temp = indices[i];
			int randomIndex = Random.Range(i, count);
			indices[i] = indices[randomIndex];
			indices[randomIndex] = temp;
		}

		return indices;
	}

	// Moves a specific NPC to a rest point by index.
	public void MoveNpcToRestPoint(int npcIndex, int restPointIndex)
	{
		if (npcIndex < 0 || npcIndex >= _activeNpcs.Count)
		{
			return;
		}
		if (restPointIndex < 0 || restPointIndex >= _restPoints.Count)
		{
			return;
		}
		_activeNpcs[npcIndex].LerpToPosition(_restPoints[restPointIndex].position);
	}

	// Moves a specific NPC to a goal point by index.
	public void MoveNpcToGoalPoint(int npcIndex, int goalPointIndex)
	{
		if (npcIndex < 0 || npcIndex >= _activeNpcs.Count)
		{
			return;
		}
		if (goalPointIndex < 0 || goalPointIndex >= _goalPoints.Count)
		{
			return;
		}
		_activeNpcs[npcIndex].LerpToPosition(_goalPoints[goalPointIndex].position, playFootsteps: true);
	}

	private void HandleNpcClicked(NpcController npc)
	{
		OnNpcClicked?.Invoke(npc);
	}

	private void HandleGreetingDialogue()
	{
		TriggerGroupDialogue(n => n.TrySayGreetingDialogue(), n => n.SayMeetDialogue());
		_hasGreeted = true;
	}

	private void HandleDoorOpenDialogue()
	{
		TriggerGroupDialogue(n => n.TrySayAcceptDialogue(), n => n.SayAcceptDialogue());
	}

	private void HandleStartDescent()
	{
		HandleDescentDialogue();
	}

	private void HandleWorkerRepair()
	{
		var workers = GameManager.Instance.PeopleOnElevator.Where(npc => npc.Role == NpcRoles.Worker).ToList();
		int workersCount = Mathf.Clamp(workers.Count, 0, _repairPoints.Count);

		List<int> availableSpawnIndices = GetShuffledIndices(_goalPoints.Count);
		for (int i = 0; i < workersCount; i++)
		{
			NpcController npc = _npcPool[i];
			int spawnIndex = availableSpawnIndices[i % availableSpawnIndices.Count];
			Vector3 spawnPos = _goalPoints[spawnIndex].position;
			npc.Initialize(workers[i], spawnPos);
		}

		List<int> shuffledRepairIndices = GetShuffledIndices(_repairPoints.Count);
		List<(NpcController npc, Vector3 originalPos)> repairAssignments = new();
		float engineRepairDelay = GameManager.Instance.EngineRepairDelay;
		for (int i = 0; i < workersCount; i++)
		{
			NpcController worker = _npcPool[i];
			Vector3 originalPos = worker.transform.position;
			repairAssignments.Add((worker, originalPos));

			Vector3 repairPos = _repairPoints[shuffledRepairIndices[i]].position;
			worker.LerpToPosition(repairPos, (engineRepairDelay / 2) - 1, playFootsteps: true);
		}

		_repairDelayTween.Stop();
		_repairDelayTween = Tween.Delay(
			this,
			engineRepairDelay / 2,
			_ =>
			{
				foreach (var (npc, originalPos) in repairAssignments)
				{
					npc.LerpToPosition(originalPos, engineRepairDelay / 2, playFootsteps: true);
				}
			}
		);
	}

	private void HandleGuards(bool skinWalkerAppears)
	{
		var guards = GameManager.Instance.PeopleOnElevator.Where(npc => npc.Role == NpcRoles.Guard).ToList();

		int guardsCount = Mathf.Clamp(guards.Count, 0, _guardPoints.Count);
		if (guardsCount == 0)
		{
			return;
		}

		List<int> availableSpawnIndices = GetShuffledIndices(_goalPoints.Count);
		List<int> shuffledGuardIndices = GetShuffledIndices(_guardPoints.Count);
		List<(NpcController npc, Vector3 originalPos)> guardAssignments = new();

		float guardDelay = GameManager.Instance.GuardDelay;

		for (int i = 0; i < guardsCount; i++)
		{
			// Get NPC from the end of pool to prevent overlap with standard indices
			int poolIndex = _npcPool.Count - 1 - i;

			NpcController guardNpc = _npcPool[poolIndex];

			// 1. Spawn at goal point
			int spawnIndex = availableSpawnIndices[i % availableSpawnIndices.Count];
			Vector3 spawnPos = _goalPoints[spawnIndex].position;

			guardNpc.Initialize(guards[i], spawnPos);
			_activeNpcs.Add(guardNpc);

			// 2. Track original position & send to guard point
			Vector3 originalPos = guardNpc.transform.position;
			guardAssignments.Add((guardNpc, originalPos));

			Vector3 guardPos = _guardPoints[shuffledGuardIndices[i]].position;
			float moveDuration = Mathf.Max(0.1f, (guardDelay / 2f) - 2f);

			guardNpc.LerpToPosition(guardPos, moveDuration, playFootsteps: true);
		}

		// 3. Return to position delay
		_guardDelayTween.Stop();
		_guardDelayTween = Tween.Delay(
			this,
			guardDelay / 2f,
			_ =>
			{
				foreach (var (npc, originalPos) in guardAssignments)
				{
					npc.LerpToPosition(originalPos, guardDelay / 2f, playFootsteps: true);
				}
			}
		);
	}

	private void HandleDescentDialogue()
	{
		if (GameManager.Instance.OpenedDoor)
		{
			return;
		}
		TriggerGroupDialogue(n => n.TrySayRejectDialogue(), n => n.SayRejectDialogue());
	}

	private void TriggerGroupDialogue(Func<NpcController, bool> trySpeak, Action<NpcController> forceSpeak)
	{
		List<NpcController> active = _activeNpcs.FindAll(n => n.IsActive);
		if (active.Count == 0)
		{
			return;
		}

		bool anySpoke = false;
		foreach (NpcController npc in active)
		{
			if (trySpeak(npc))
			{
				anySpoke = true;
			}
		}

		if (!anySpoke)
		{
			forceSpeak(active[Random.Range(0, active.Count)]);
		}
	}
}
