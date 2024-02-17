#if TOOLS
using Godot;
using System;

[Tool]
public partial class SceneManager : EditorPlugin
{
	private const string AutoloadName = "SceneManagerNode";

	public override void _EnterTree() => AddAutoloadSingleton(AutoloadName, "res://addons/SceneManager/SceneManagerNode.tscn");

	public override void _ExitTree() => RemoveAutoloadSingleton(AutoloadName);
}
#endif