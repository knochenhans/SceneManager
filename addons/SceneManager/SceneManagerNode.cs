using Godot;
using Godot.Collections;

public partial class SceneManagerNode : Node
{
	public Array<string> SceneNames { get; set; }

	string CurrentSceneName { get; set; }
	Scene CurrentScene { get; set; }

	public void Init(Array<string> sceneNames, string initialSceneName)
	{
		CurrentSceneName = initialSceneName;
		SceneNames = sceneNames;

		CallDeferred("ChangeToScene", CurrentSceneName);
	}

	public async void ChangeToScene(string sceneName)
	{
		GD.Print($"Changing to scene {sceneName}");
		CurrentSceneName = sceneName;

		if (CurrentScene != null)
		{
			CurrentScene.Exit();
			await ToSignal(CurrentScene, "ExitFinished");
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
			await ToSignal(CurrentScene, "ExitFinished");
		}

		GetTree().Quit();
	}
}
