using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSO", menuName = "ScriptableObjects/LevelSO", order = 0)]
public class LevelSO : ScriptableObject
{
	[field: SerializeField]
	public List<Level> LevelsList { private set; get; } = new();
}

[Serializable]
public class Level
{
	[Tooltip("Optional Ink dialogue knot to play when arriving at this level")]
	public string OnLoadDialogueKnot;
	
	[Tooltip("Guaranteed Spawns")]
	public List<NpcRoles> NpcGuaranteedSpawns;

	[Tooltip("Range of Npcs to Spawn Randomly")]
	public Vector2Int NpcSpawnRandomRange;

	//

	// come up with a number for how many of each role should spawn (which is partially random)
	public Dictionary<NpcRoles, int> GenerateSpawnCounts(RolesSO rolesData = null)
	{
		Dictionary<NpcRoles, int> spawnCounts = new();
		foreach (NpcRoles role in Enum.GetValues(typeof(NpcRoles)))
		{
			spawnCounts[role] = 0;
		}

		if (NpcGuaranteedSpawns != null)
		{
			foreach (NpcRoles role in NpcGuaranteedSpawns)
			{
				spawnCounts[role]++;
			}
		}

		if (rolesData != null && NpcSpawnRandomRange != Vector2Int.zero)
		{
			int randomCount = UnityEngine.Random.Range(NpcSpawnRandomRange.x, NpcSpawnRandomRange.y + 1);
			for (int i = 0; i < randomCount; i++)
			{
				NpcRoles randomRole = rolesData.GetRandomRole();
				spawnCounts[randomRole]++;
			}
		}

		return spawnCounts;
	}
}
