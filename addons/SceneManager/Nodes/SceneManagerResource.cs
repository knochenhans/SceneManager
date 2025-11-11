using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SceneManagerResource : Resource
{
	[Export] public Dictionary<string, PackedScene> ScenesPackedScenes = [];
	[Export] public string initialSceneName = "game";
}
