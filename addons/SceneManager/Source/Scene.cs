using Godot;
using System.Threading.Tasks;
using static Logger;

public partial class Scene : Node
{
	[Signal] public delegate void ReadyFinishedEventHandler();
	[Signal] public delegate void ExitFinishedEventHandler();

	[Export] public float FadeInTime = 0.5f;
	[Export] public float FadeOutTime = 0.5f;
	[Export] public string DefaultNextScene = "";

	PackedScene FadeScene = ResourceLoader.Load<PackedScene>("res://addons/SceneManager/Nodes/Fade.tscn");
	CanvasLayer CanvasLayerNode => GetNode<CanvasLayer>("CanvasLayer");
	ColorRect BackgroundNode => GetNode<ColorRect>("ColorRect");

	private async Task Fade(Fade.FadeDirectionEnum direction, float time)
	{
		var fade = FadeScene.Instantiate<Fade>();
		CanvasLayerNode.AddChild(fade);
		fade.Run(direction, time);
		await ToSignal(fade, global::Fade.SignalName.FadeFinished);
	}

	public async override void _Ready()
	{
		BackgroundNode.GuiInput += @event =>
		{
			if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
				SceneManager.Instance.ChangeToDefaultNextScene();
		};

		Log($"Starting scene {SceneFilePath}", "SceneManager", LogTypeEnum.Framework);
		await Fade(global::Fade.FadeDirectionEnum.In, FadeInTime);

		EmitSignal(SignalName.ReadyFinished);
	}

	public async void Exit()
	{
		Log($"Exiting scene {SceneFilePath}", "SceneManager", LogTypeEnum.Framework);
		await Fade(global::Fade.FadeDirectionEnum.Out, FadeOutTime);

		EmitSignal(SignalName.ExitFinished);
		QueueFree();
	}
}
