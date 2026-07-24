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

	#endregion

	#region Music

	// [field: SerializeField]
	// [field: FoldoutGroup("Music", expanded: true)]
	// public EventReference Bgm { get; private set; }

	#endregion

	#region Elevator Noises

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorOpen_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorClose_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorDescend_Sfx { get; private set; }

	#endregion

	#region Looping SFX

	// [field: SerializeField]
	// [field: FoldoutGroup("Loop SFX", true)]
	// public EventReference Falling_LoopSfx { get; private set; }

	#endregion
}
