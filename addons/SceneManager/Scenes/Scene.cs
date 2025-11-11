using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using static Logger;

[GlobalClass]
public partial class Scene : Node
{
	public enum SceneStateEnum
	{
		Idle,
		TransitioningIn,
		TransitioningOut
	}

	[Export] public string DefaultNextScene = "";
	[Export] bool PlayUIMusic = false;

	[ExportGroup("Mouse")]
	[Export] public Dictionary<string, CursorSetResource> CursorSets = [];
	[Export] public Input.MouseModeEnum DefaultMouseMode = Input.MouseModeEnum.Visible;
	[Export] public int ScaleFactor = 1;

	[ExportGroup("Fade Settings")]
	[Export] public float FadeInTime = 0.5f;
	[Export] public float FadeOutTime = 0.5f;
	[Export] public float LifeTime = 0.0f;

	protected ColorRect BackgroundNode => GetNodeOrNull<ColorRect>("SceneBackground");
	protected VBoxContainer ButtonsNode => GetNodeOrNull<VBoxContainer>("%Buttons");
	protected Array<SceneButton> SceneButtons;
	protected SceneStateEnum SceneState = SceneStateEnum.TransitioningIn;

	Timer LifeTimerNode => GetNode<Timer>("LifeTimer");

	public CursorManager CursorManager = null!;
	protected Input.MouseModeEnum LastMouseMode;

	public override void _Ready()
	{
		Log($"Starting scene {SceneFilePath}", "SceneManager", LogTypeEnum.Framework);

		CursorManager = new CursorManager(CursorSets, ScaleFactor);

		if (UISoundPlayer.Instance == null)
			LogError("UISoundPlayer instance is null!", "SceneManager", LogTypeEnum.Framework);

		if (ButtonsNode != null)
			SceneButtons = [.. ButtonsNode.GetChildren().Where(node => node is SceneButton).Cast<SceneButton>()];

		if (BackgroundNode != null)
			BackgroundNode.GuiInput += OnBackgroundClicked;

		if (LifeTime > 0)
		{
			LifeTimerNode.WaitTime = LifeTime;
			LifeTimerNode.Start();
			LifeTimerNode.Timeout += ChangeToNextScene;
			Log($"Scene {Name} will change to next scene after {LifeTime} seconds.", "Scene", LogTypeEnum.Framework);
		}

		if (PlayUIMusic)
			UISoundPlayer.Instance.StartOrKeepMusic();
		else
			UISoundPlayer.Instance.StopMusic();

		LastMouseMode = Input.MouseMode;

		SceneState = SceneStateEnum.Idle;
	}

	protected virtual void OnBackgroundClicked(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
		{
			UISoundPlayer.Instance.PlaySound("click1");
			ChangeToNextScene();
		}
	}

	protected async void ChangeToNextScene()
	{
		SceneState = SceneStateEnum.TransitioningOut;
		await SceneManager.Instance.ChangeToDefaultNextScene();
	}

	public override void _Input(InputEvent @event)
	{
		if (SceneState != SceneStateEnum.Idle)
			return;

		base._Input(@event);
	}

	public virtual void DisableInput()
	{
		BackgroundNode.SetBlockSignals(true);
		if (SceneButtons != null)
		{
			foreach (var button in SceneButtons)
				button.SetBlockSignals(true);
		}
	}

	public async virtual Task Close()
	{
	}
}

