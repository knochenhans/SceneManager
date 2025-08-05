using System.Linq;
using Godot;
using Godot.Collections;
using static Logger;

public partial class Scene : Node
{
	[Export] public float FadeInTime = 0.5f;
	[Export] public float FadeOutTime = 0.5f;
	[Export] public float LifeTime = 0.0f;
	[Export] public string DefaultNextScene = "";

	protected ColorRect BackgroundNode => GetNode<ColorRect>("ColorRect");
	protected VBoxContainer ButtonsNode => GetNode<VBoxContainer>("%Buttons");
	protected Array<SceneButton> SceneButtons;

	Timer LifeTimerNode => GetNode<Timer>("LifeTimer");
	public UISoundPlayer UISoundPlayer;

	public override void _Ready()
	{
		SceneButtons = [.. ButtonsNode.GetChildren().Where(node => node is SceneButton).Cast<SceneButton>()];

		BackgroundNode.GuiInput += OnBackgroundClicked;

		if (LifeTime > 0)
		{
			LifeTimerNode.WaitTime = LifeTime;
			LifeTimerNode.Start();
			LifeTimerNode.Timeout += ChangeToNextScene;
			Log($"Scene {Name} will change to next scene after {LifeTime} seconds.", "Scene", LogTypeEnum.Framework);
		}

		foreach (var button in SceneButtons)
			button.UISoundPlayer = UISoundPlayer;

		Log($"Starting scene {SceneFilePath}", "SceneManager", LogTypeEnum.Framework);
	}

	protected virtual void OnBackgroundClicked(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
		{
			UISoundPlayer.PlaySound("click1");
			ChangeToNextScene();
		}
	}

	protected async void ChangeToNextScene() => await SceneManager.Instance.ChangeToDefaultNextScene();

	public virtual void DisableInput()
	{
		BackgroundNode.SetBlockSignals(true);
		if (SceneButtons != null)
		{
			foreach (var button in SceneButtons)
				button.SetBlockSignals(true);
		}
	}
}
