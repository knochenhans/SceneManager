using Godot;
using static Logger;

public partial class Scene : Node
{
	[Export] public float FadeInTime = 0.5f;
	[Export] public float FadeOutTime = 0.5f;
	[Export] public float LifeTime = 0.0f;
	[Export] public string DefaultNextScene = "";

	CanvasLayer CanvasLayerNode => GetNode<CanvasLayer>("CanvasLayer");
	protected ColorRect BackgroundNode => GetNode<ColorRect>("ColorRect");
	Timer LifeTimerNode => GetNode<Timer>("LifeTimer");

	public override void _Ready()
	{
		BackgroundNode.GuiInput += OnBackgroundClicked;

		if (LifeTime > 0)
		{
			LifeTimerNode.WaitTime = LifeTime;
			LifeTimerNode.Start();
			LifeTimerNode.Timeout += ChangeToNextScene;
			Log($"Scene {Name} will change to next scene after {LifeTime} seconds.", "Scene", LogTypeEnum.Framework);
		}

		Log($"Starting scene {SceneFilePath}", "SceneManager", LogTypeEnum.Framework);
	}

	protected virtual void OnBackgroundClicked(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
			ChangeToNextScene();
	}

    protected async void ChangeToNextScene() => await SceneManager.Instance.ChangeToDefaultNextScene();
}
