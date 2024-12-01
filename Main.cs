using Godot;
using Godot.Collections;

public partial class Main : Node
{
	public SceneManager SceneManager => GetNode<SceneManager>("/root/SceneManager");

	public override void _Ready()
	{
		var scenes = new Array<string> { "Intro", "Menu", "Game", "Options", "Credits" };

		SceneManager.Init(scenes, "Intro");
	}
}
