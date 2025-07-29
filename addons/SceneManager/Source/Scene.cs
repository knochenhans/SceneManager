using Godot;
using System.Threading.Tasks;
using static Logger;

public partial class Scene : Node
{
	[Signal] public delegate void ReadyFinishedEventHandler();
	[Signal] public delegate void ExitFinishedEventHandler();

	[Export] public float FadeInTime = 1f;
	[Export] public float FadeOutTime = 1f;

	PackedScene FadeScene = ResourceLoader.Load<PackedScene>("res://addons/SceneManager/Nodes/Fade.tscn");

	public SceneManager SceneManager => GetNode<SceneManager>("/root/SceneManager");

	private async Task Fade(Fade.FadeDirectionEnum direction, float time)
	{
		var fade = FadeScene.Instantiate<Fade>();
		GetNode<CanvasLayer>("CanvasLayer").AddChild(fade);
		fade.Run(direction, time);
		await ToSignal(fade, global::Fade.SignalName.FadeFinished);
	}

	public async override void _Ready()
	{
		Log($"Starting scene {this.Name}", LogTypeEnum.Framework);
		await Fade(global::Fade.FadeDirectionEnum.In, FadeInTime);

		EmitSignal(SignalName.ReadyFinished);
	}

	public async void Exit()
	{
		Log($"Exiting scene {this.Name}", LogTypeEnum.Framework);
		await Fade(global::Fade.FadeDirectionEnum.Out, FadeOutTime);

		EmitSignal(SignalName.ExitFinished);
		QueueFree();
	}
}
