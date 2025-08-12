using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SceneManagerResource : Resource
{
	[Export] public Dictionary<string, PackedScene> ScenesPackedScenes = [];
	[Export] public string initialSceneName = "game";
	[Export] public float OverlayMenuOpacity = 0.5f;
	[Export] public float OverlayMenuFadeTime = 0.3f;
}
