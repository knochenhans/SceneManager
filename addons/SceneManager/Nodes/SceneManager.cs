using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MenuEntryData = Godot.Collections.Dictionary<string, Godot.Variant>;
using static Logger;



public partial class SceneManager : Node
{
	[Export] public SceneManagerResource SceneManagerResource;
	[Export] public PackedScene OverlayMenuFramePackedScene;
	[Export] public Dictionary<string, PackedScene> OverlayMenusInnerPackedScenes = [];

	[Signal] public delegate void OverlayMenuOpenedEventHandler();
	[Signal] public delegate void OverlayMenuClosedEventHandler();
	[Signal] public delegate void OverlayMenuEntrySelectedEventHandler(MenuEntryData entryData);

	public Dictionary<string, PackedScene> ScenesPackedScenes => SceneManagerResource.ScenesPackedScenes;
	public string InitialSceneName => SceneManagerResource.initialSceneName;
	public float OverlayMenuOpacity => SceneManagerResource.OverlayMenuOpacity;
	public float OverlayMenuFadeTime => SceneManagerResource.OverlayMenuFadeTime;

	public static SceneManager Instance { get; private set; }
	public OverlayMenu OverlayMenuNode;

	public Array<string> SceneNames { get; set; }
	string CurrentSceneName { get; set; }
	Scene CurrentScene { get; set; }

	ColorRect FadeScene => GetNode<ColorRect>("%Fade");
	CanvasLayer CanvasLayer => GetNode<CanvasLayer>("CanvasLayer");

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
		Log("SceneManager is ready.", "SceneManager", LogTypeEnum.Framework);
		Log($"Found {ScenesPackedScenes.Count} scenes in ScenesPackedScenes.", "SceneManager", LogTypeEnum.Framework);
		foreach (var scene in ScenesPackedScenes)
			Log($"Scene: {scene.Key}", "SceneManager", LogTypeEnum.Framework);
		Log($"Initial scene: {InitialSceneName}", "SceneManager", LogTypeEnum.Framework);

		CallDeferred(MethodName.ChangeToScene, InitialSceneName);
	}

	public async void ChangeToScene(string sceneName)
	{
		if (CurrentScene != null)
			await ExitCurrentScene();

		await StartScene(sceneName);
	}

	private async Task StartScene(string sceneName)
	{
		sceneName = sceneName.ToLower();
		Log($"Starting scene {sceneName}", "SceneManager", LogTypeEnum.Framework);

		CurrentSceneName = sceneName;
		CurrentScene = ScenesPackedScenes.TryGetValue(CurrentSceneName, out var packedScene) ? packedScene.Instantiate() as Scene : null;

		if (CurrentScene == null)
		{
			LogError($"Failed to instantiate scene {CurrentSceneName}", "SceneManager", LogTypeEnum.Framework);
			return;
		}

		AddChild(CurrentScene);

		await FadeHelper.TweenFadeModulate(FadeScene, FadeHelper.FadeDirectionEnum.In, CurrentScene.FadeInTime, transitionType: Tween.TransitionType.Cubic);
	}

	private async Task ExitCurrentScene()
	{
		if (CurrentScene == null)
		{
			LogWarning("No current scene to exit.", "SceneManager", LogTypeEnum.Framework);
			return;
		}

		Log($"Exiting scene {CurrentScene.Name}", "SceneManager", LogTypeEnum.Framework);

		CurrentScene.DisableInput();

		await FadeHelper.TweenFadeModulate(FadeScene, FadeHelper.FadeDirectionEnum.Out, CurrentScene.FadeOutTime, transitionType: Tween.TransitionType.Cubic);

		await CurrentScene.Close();
		CurrentScene.QueueFree();

		await ToSignal(CurrentScene, Node.SignalName.TreeExited);

		CurrentScene = null;
	}

	public async Task ChangeToDefaultNextScene()
	{
		if (CurrentScene.DefaultNextScene == "")
		{
			Log($"No default next scene set for {CurrentScene.Name}, quitting instead.", "SceneManager", LogTypeEnum.Framework);
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
		await ExitCurrentScene();

		GetTree().Quit();
	}

	public async Task ShowOverlayMenu(string id = "options")
	{
		OverlayMenuNode = OverlayMenuFramePackedScene.Instantiate<OverlayMenu>();
		CanvasLayer.AddChild(OverlayMenuNode);

		OverlayMenuNode.Closed += HideOverlayMenu;
		OverlayMenuNode.EntrySelected += (entryName) => EmitSignal(SignalName.OverlayMenuEntrySelected, entryName);

		if (OverlayMenusInnerPackedScenes.TryGetValue(id, out var packedScene))
		{
			var inner = packedScene.Instantiate<OverlayInner>();
			await OverlayMenuNode.ShowMenu(inner);
		}

		EmitSignal(SignalName.OverlayMenuOpened);
	}

	public async void HideOverlayMenu()
	{
		await OverlayMenuNode.HideMenu();
		OverlayMenuNode.Closed -= () => HideOverlayMenu();
		OverlayMenuNode.QueueFree();

		EmitSignal(SignalName.OverlayMenuClosed);
	}
}
