using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSO", menuName = "ScriptableObjects/LevelSO", order = 0)]
public class LevelSO : ScriptableObject
{
	[field: SerializeField]
	public List<Level> LevelsList { private set; get; } = new();
}

[System.Serializable]
public class Level
{
	[Tooltip("Optional Ink dialogue knot to play when arriving at this level")]
	public string OnLoadDialogueKnot;
	
	[Tooltip("Guaranteed Spawns")]
	public List<NpcRoles> NpcGuaranteedSpawns;

	[Tooltip("Range of Npcs to Spawn Randomly")]
	public Vector2Int NpcSpawnRandomRange;
}

public class LevelInstance
{
	[Tooltip("Guaranteed Spawns")]
	public Dictionary<NpcRoles, int> NpcGuaranteedSpawns;

	public LevelInstance()
	{
		NpcGuaranteedSpawns = new Dictionary<NpcRoles, int>();
		foreach (NpcRoles role in Enum.GetValues(typeof(NpcRoles)))
		{
			NpcGuaranteedSpawns[role] = 0;
		}
	}

	public LevelInstance(Level level, RolesSO rolesData = null)
		: this()
	{
		if (level?.NpcGuaranteedSpawns != null)
		{
			foreach (NpcRoles role in level.NpcGuaranteedSpawns)
			{
				NpcGuaranteedSpawns[role]++;
			}
		}

		if (rolesData != null && level.NpcSpawnRandomRange != Vector2Int.zero)
		{
			int randomCount = UnityEngine.Random.Range(level.NpcSpawnRandomRange.x, level.NpcSpawnRandomRange.y + 1);
			for (int i = 0; i < randomCount; i++)
			{
				NpcRoles randomRole = rolesData.GetRandomRole();
				NpcGuaranteedSpawns[randomRole]++;
			}
		}
	}
}
