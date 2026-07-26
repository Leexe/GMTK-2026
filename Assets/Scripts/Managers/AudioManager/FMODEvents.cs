using System.Diagnostics.CodeAnalysis;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "Odin.OdinUnknownGroupingPath")]
public class FMODEvents : MonoSingleton<FMODEvents>
{
	#region Ambience

	[field: SerializeField]
	[field: FoldoutGroup("Ambience", expanded: true)]
	public EventReference Ambience_Amb { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Ambience", expanded: true)]
	public EventReference CreepySound_Sfx { get; private set; }

	#endregion

	#region Music

	[field: SerializeField]
	[field: FoldoutGroup("Music", expanded: true)]
	public EventReference Win_Bgm { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Music", expanded: true)]
	public EventReference Lose_Bgm { get; private set; }

	#endregion

	#region Sfx


	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorArrive_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorBeep_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorClose_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference SkinwalkerEncounter_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorFalling_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorLightsOut_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorOpen_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Buzz", expanded: true)]
	public EventReference ElevatorShortBuzz_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Generator", expanded: true)]
	public EventReference EngineDamage_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Generator", expanded: true)]
	public EventReference EngineExplode_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Generator", expanded: true)]
	public EventReference EngineFix_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("NPCs", expanded: true)]
	public EventReference Footsteps_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("NPCs", expanded: true)]
	public EventReference NpcDeath_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("NPCs", expanded: true)]
	public EventReference SoldierShot_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("NPCs", expanded: true)]
	public EventReference NpcChat_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("NPCs", expanded: true)]
	public EventReference NpcSad_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("UI", expanded: true)]
	public EventReference Clipboard_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("UI", expanded: true)]
	public EventReference Documents_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("UI", expanded: true)]
	public EventReference ForwardText_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("UI", expanded: true)]
	public EventReference Tablet_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Misc", expanded: true)]
	public EventReference PlayerDeath_Sfx { get; private set; }

	#endregion

	#region Loop Sfx

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorDescend_LoopSfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Generator", expanded: true)]
	public EventReference EngineLow_LoopSfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Generator", expanded: true)]
	public EventReference EngineRunning_LoopSfx { get; private set; }

	#endregion
}
