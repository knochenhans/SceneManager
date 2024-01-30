using Godot;
using System;

public partial class Scene : Node
{
	[Signal]
	public delegate void ReadyFinishedEventHandler();

	[Signal]
	public delegate void ExitFinishedEventHandler();

	[Export]
	public float FadeInTime = 1f;

	[Export]
	public float FadeOutTime = 1f;

	PackedScene FadeScene = ResourceLoader.Load<PackedScene>("res://Scenes/Fade.tscn");

	public SceneManager SceneManagerNode { get; set; }

	public async override void _Ready()
	{
		SceneManagerNode = GetNode<SceneManager>("/root/SceneManager");

		var fade = FadeScene.Instantiate<Fade>();
		GetTree().Root.AddChild(fade);
		fade.FadeIn(FadeInTime);
		await ToSignal(fade, "FadeFinished");

		EmitSignal(SignalName.ReadyFinished);
	}

	public async void Exit()
	{
		var fade = FadeScene.Instantiate<Fade>();
		GetTree().Root.AddChild(fade);
		fade.FadeOut(FadeOutTime);
		await ToSignal(fade, "FadeFinished");

		EmitSignal(SignalName.ExitFinished);
		QueueFree();
	}
}
