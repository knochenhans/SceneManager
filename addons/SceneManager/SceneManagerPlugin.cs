#if TOOLS
using Godot;

[Tool]
public partial class SceneManagerPlugin : EditorPlugin
{
	private const string AutoloadName = "SceneManager";

	public override void _EnterTree() => AddAutoloadSingleton(AutoloadName, "res://addons/SceneManager/Nodes/SceneManager.tscn");

	public override void _ExitTree() => RemoveAutoloadSingleton(AutoloadName);
}
#endif