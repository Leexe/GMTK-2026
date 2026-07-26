using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcMaterialMapSO", menuName = "ScriptableObjects/NpcMaterialMapSO", order = 0)]
public class NpcMaterialMapSO : ScriptableObject
{
	[field: SerializeField]
	public List<Material> WorkerMaterials { get; private set; }

	[field: SerializeField]
	public List<Material> PsychologistMaterials { get; private set; }

	[field: SerializeField]
	public List<Material> GuardMaterials { get; private set; }

	public List<Material> GetMaterialsForRole(NpcRoles role)
	{
		return role switch
		{
			NpcRoles.Worker => WorkerMaterials,
			NpcRoles.Psychologist => PsychologistMaterials,
			NpcRoles.Guard => GuardMaterials,
			_ => WorkerMaterials,
		};
	}
}
