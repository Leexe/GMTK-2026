using System;
using System.Collections.Generic;
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

	private List<NpcController> _npcPool = new();
	private List<NpcController> _activeNpcs = new();
	private List<int> _occupiedSpawnIndices = new();
	private int _arrivedNpcCount;

	// Events
	public Action OnAllNpcsArrived;

	private void Awake()
	{
		InitializePool();
	}

	private void Start()
	{
		HandleNewFloor();
	}

	private void InitializePool()
	{
		for (int i = 0; i < _spawnPoints.Count; i++)
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
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnNpcUpdate -= HandleNpcUpdate;
			GameManager.Instance.OnNewFloor -= HandleNewFloor;
		}

		UnsubscribeFromActiveNpcs();
	}

	// Called when OnNewFloor fires: spawns current floor's NPCs at spawn points and immediately moves them to rest points
	private void HandleNewFloor()
	{
		// Clean Up
		UnsubscribeFromActiveNpcs();
		foreach (NpcController npc in _activeNpcs)
		{
			npc.DisableVisuals();
		}
		_activeNpcs.Clear();
		_occupiedSpawnIndices.Clear();
		_arrivedNpcCount = 0;

		int currentFloor = GameManager.Instance.CurrentFloor;
		Dictionary<NpcRoles, int> guaranteedSpawns = GameManager
			.Instance
			.LevelInstances[currentFloor]
			.NpcGuaranteedSpawns;
		List<NpcRoles> rolesToSpawn = new();
		foreach (KeyValuePair<NpcRoles, int> kvp in guaranteedSpawns)
		{
			for (int i = 0; i < kvp.Value; i++)
			{
				rolesToSpawn.Add(kvp.Key);
			}
		}

		int spawnCount = Mathf.Min(rolesToSpawn.Count, _npcPool.Count, _spawnPoints.Count);
		if (spawnCount == 0)
		{
			HandleAllNpcsArrived();
			return;
		}

		for (int i = 0; i < spawnCount; i++)
		{
			int spawnIndex = GetRandomAvailableSpawnIndex();
			if (spawnIndex < 0)
			{
				Debug.LogWarning("No available spawn points");
				spawnIndex = i % _spawnPoints.Count;
			}

			NpcController npc = _npcPool[i];
			Vector3 spawnPos = _spawnPoints[spawnIndex].position;

			npc.Initialize(rolesToSpawn[i], spawnPos);
			npc.OnArrivedAtPosition += HandleNpcArrived;
			_activeNpcs.Add(npc);

			Vector3 restPos = _restPoints[i % _restPoints.Count].position;
			npc.LerpToPosition(restPos);
		}
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

		for (int i = 0; i < _activeNpcs.Count; i++)
		{
			NpcController npc = _activeNpcs[i];
			Vector3 goalPos = _goalPoints[i % _goalPoints.Count].position;
			npc.LerpToPosition(goalPos);
		}
	}

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
		Debug.Log("All active NPCs arrived at their position.");
		if (GameManager.Instance != null)
		{
			GameManager.Instance.SetNpcsFinishedMoving(true);
		}
		OnAllNpcsArrived?.Invoke();
	}

	private void UnsubscribeFromActiveNpcs()
	{
		foreach (NpcController npc in _activeNpcs)
		{
			npc.OnArrivedAtPosition -= HandleNpcArrived;
		}
	}

	// Picks a random spawn index that is not already occupied
	private int GetRandomAvailableSpawnIndex()
	{
		List<int> available = new();
		for (int i = 0; i < _spawnPoints.Count; i++)
		{
			if (!_occupiedSpawnIndices.Contains(i))
			{
				available.Add(i);
			}
		}

		if (available.Count == 0)
		{
			return -1;
		}

		int chosen = available[Random.Range(0, available.Count)];
		_occupiedSpawnIndices.Add(chosen);
		return chosen;
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
		_activeNpcs[npcIndex].LerpToPosition(_goalPoints[goalPointIndex].position);
	}
}
