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

	[field: SerializeField]
	[field: FoldoutGroup("Music", expanded: true)]
	public EventReference Lose_Bgm { get; private set; }

	#endregion

	#region Sfx

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorOpen_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorClose_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("Elevator Noises", expanded: true)]
	public EventReference ElevatorDescend_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("NPCs", expanded: true)]
	public EventReference Footsteps_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("UI", expanded: true)]
	public EventReference Clipboard_Sfx { get; private set; }

	[field: SerializeField]
	[field: FoldoutGroup("UI", expanded: true)]
	public EventReference Documents_Sfx { get; private set; }

	#endregion
}
