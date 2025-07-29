using Godot;
using Godot.Collections;
using static Logger;

public partial class SceneManager : Node
{
	public Array<string> SceneNames { get; set; }

	string CurrentSceneName { get; set; }
	Scene CurrentScene { get; set; }

	public void Init(Array<string> sceneNames, string initialSceneName)
	{
		CurrentSceneName = initialSceneName;
		SceneNames = sceneNames;

		CallDeferred(MethodName.ChangeToScene, CurrentSceneName);
	}

	public async void ChangeToScene(string sceneName)
	{
		Log($"Changing to scene {sceneName}", LogTypeEnum.Framework);
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
