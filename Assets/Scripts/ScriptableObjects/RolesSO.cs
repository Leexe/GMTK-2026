using System.Collections.Generic;
using UnityEngine;

public enum NpcRoles
{
	Worker = 0,
	Psychologist = 1,
	Guard = 2,
	Skinwalker = 3,
}

[CreateAssetMenu(fileName = "RolesSO", menuName = "ScriptableObjects/RolesSO", order = 0)]
public class RolesSO : ScriptableObject
{
	[System.Serializable]
	public struct RoleSpawnData
	{
		public NpcRoles Role;

		[Tooltip("Weight for this role")]
		public float SpawnWeight;
	}

	[SerializeField]
	private List<RoleSpawnData> _roleSpawnWeights = new();

	public List<RoleSpawnData> RoleSpawnWeights => _roleSpawnWeights;

	public float GetSpawnWeight(NpcRoles role)
	{
		foreach (RoleSpawnData data in _roleSpawnWeights)
		{
			if (data.Role == role)
			{
				return data.SpawnWeight;
			}
		}
		return 0f;
	}

	public NpcRoles GetRandomRole()
	{
		float totalWeight = 0f;
		foreach (RoleSpawnData data in _roleSpawnWeights)
		{
			totalWeight += data.SpawnWeight;
		}

		if (totalWeight <= 0f)
		{
			return NpcRoles.Worker;
		}

		float randomValue = Random.Range(0f, totalWeight);
		float currentWeight = 0f;

		foreach (RoleSpawnData data in _roleSpawnWeights)
		{
			currentWeight += data.SpawnWeight;
			if (randomValue <= currentWeight)
			{
				return data.Role;
			}
		}

		return NpcRoles.Worker;
	}
}
