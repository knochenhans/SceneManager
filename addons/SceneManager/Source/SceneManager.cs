using Godot;
using Godot.Collections;
using static Logger;

public partial class SceneManager : Node
{
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

	public void Init(Array<string> sceneNames, string initialSceneName)
	{
		CurrentSceneName = initialSceneName;
		SceneNames = sceneNames;

		CallDeferred(MethodName.ChangeToScene, CurrentSceneName);
	}

	public async void ChangeToScene(string sceneName)
	{
		Log($"Changing to scene {sceneName}", "SceneManager", LogTypeEnum.Framework);
		CurrentSceneName = sceneName;

		if (CurrentScene != null)
		{
			CurrentScene.Exit();
			await ToSignal(CurrentScene, Scene.SignalName.ExitFinished);
		}

		var newScene = ResourceLoader.Load<PackedScene>($"res://Scenes/{CurrentSceneName}.tscn").Instantiate();
		GetTree().Root.AddChild(newScene);
		GetTree().CurrentScene = newScene;
		CurrentScene = newScene as Scene;
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
