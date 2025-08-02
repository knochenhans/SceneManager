using Godot;
using Godot.Collections;
using static Logger;

public partial class SceneManager : Node
{
	[Export] public Dictionary<string, PackedScene> ScenesPackedScenes = [];
	[Export] public string initialSceneName = "game";

	public static SceneManager Instance { get; private set; }

	public Array<string> SceneNames { get; set; }
	string CurrentSceneName { get; set; }
	Scene CurrentScene { get; set; }

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

	public override void _Ready()
	{
		CallDeferred(MethodName.ChangeToScene, initialSceneName);
	}

	public async void ChangeToScene(string sceneName)
	{
		CurrentSceneName = sceneName;

		if (CurrentScene != null)
		{
			CurrentScene.Exit();
			await ToSignal(CurrentScene, Scene.SignalName.ExitFinished);
		}

		var newScene = ScenesPackedScenes[CurrentSceneName].Instantiate();
		Log($"Changing to scene {newScene.Name}", "SceneManager", LogTypeEnum.Framework);
		GetTree().Root.AddChild(newScene);
		GetTree().CurrentScene = newScene;
		CurrentScene = newScene as Scene;
	}

	public void ChangeToDefaultNextScene()
	{
		if (CurrentScene.DefaultNextScene == "")
		{
			Log($"No default next scene set for {CurrentScene.Name}, quitting.", "SceneManager", LogTypeEnum.Framework);
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
		{
			CurrentScene.Exit();
			await ToSignal(CurrentScene, Scene.SignalName.ExitFinished);
		}

		GetTree().Quit();
	}
}
