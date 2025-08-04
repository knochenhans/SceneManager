using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using static Logger;

public partial class SceneManager : Node
{
	[Export] public Dictionary<string, PackedScene> ScenesPackedScenes = [];
	[Export] public string initialSceneName = "game";
	[Export] public float OverlayMenuOpacity = 0.5f;
	[Export] public float OverlayMenuFadeTime = 0.3f;

	public static SceneManager Instance { get; private set; }
	public OverlayMenu OverlayMenuNode => GetNode<OverlayMenu>("%OverlayMenu");

	public Array<string> SceneNames { get; set; }
	string CurrentSceneName { get; set; }
	Scene CurrentScene { get; set; }

	ColorRect FadeScene => GetNode<ColorRect>("%Fade");

	public override void _EnterTree()
	{
		// Singleton setup
		if (Instance != null && Instance != this)
		{
			LogError("Duplicate SceneManager instance detected, destroying the new one.", "SceneManager", LogTypeEnum.Framework);
			QueueFree();
			return;
		}

		Instance = this;
	}

	public override void _Ready() => CallDeferred(MethodName.ChangeToScene, initialSceneName);	

	public async void ChangeToScene(string sceneName)
	{
		if (CurrentScene != null)
			await ExitCurrentScene();

		CurrentSceneName = sceneName;

		CurrentScene = ScenesPackedScenes[CurrentSceneName].Instantiate() as Scene;

		await FadeManager.TweenFadeModulate(FadeScene, FadeManager.FadeDirectionEnum.Out, CurrentScene.FadeOutTime);

		Log($"Changing to scene {CurrentScene.Name}", "SceneManager", LogTypeEnum.Framework);

		AddChild(CurrentScene);

		await FadeManager.TweenFadeModulate(FadeScene, FadeManager.FadeDirectionEnum.In, CurrentScene.FadeInTime);
	}

	private async Task ExitCurrentScene()
	{
		Log($"Exiting scene {CurrentScene.Name}", "SceneManager", LogTypeEnum.Framework);

		CurrentScene.DisableInput();

		await FadeManager.TweenFadeModulate(FadeScene, FadeManager.FadeDirectionEnum.Out, CurrentScene.FadeOutTime);

		CurrentScene.QueueFree();
		await ToSignal(CurrentScene, Node.SignalName.TreeExited);
		CurrentScene = null;
	}

	public async Task ChangeToDefaultNextScene()
	{
		if (CurrentScene.DefaultNextScene == "")
		{
			Log($"No default next scene set for {CurrentScene.Name}, quitting.", "SceneManager", LogTypeEnum.Framework);
			await ExitCurrentScene();
			Quit();
		}
		else
		{
			Log($"Changing to default next scene {CurrentScene.DefaultNextScene}", "SceneManager", LogTypeEnum.Framework);
			ChangeToScene(CurrentScene.DefaultNextScene);
		}
	}

	public void RestartScene() => GetTree().ReloadCurrentScene();

	public async void Quit()
	{
		if (CurrentScene != null)
			await ExitCurrentScene();

		GetTree().Quit();
	}
}
