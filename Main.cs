using Godot;
using Godot.Collections;

public partial class Main : Node2D
{
	public SceneManagerNode SceneManagerNode { get; set; }

	public override void _Ready()
	{
		SceneManagerNode = GetNode<SceneManagerNode>("/root/SceneManagerNode");

		var scenes = new Array<string> { "Intro", "Menu", "Game", "Options", "Credits" };

		SceneManagerNode.Init(scenes, "Intro");
	}
}
