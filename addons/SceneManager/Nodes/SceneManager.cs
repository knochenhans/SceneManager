using System.Threading.Tasks;

using Godot;
using Godot.Collections;

using static Logger;

using InitData = Godot.Collections.Dictionary<string, Godot.Variant>;

public partial class SceneManager : Node
{
    #region [Fields and Properties]
    [Export] public SceneManagerResource SceneManagerResource;
    [Export] public AudioBusLayout AudioBusLayout;

    [ExportCategory("Mouse")]
    [Export] public Dictionary<string, CursorSetResource> CursorSets = [];
    [Export] public int MouseScaleFactor = 1;

    public Dictionary<string, PackedScene> ScenesPackedScenes => SceneManagerResource.ScenesPackedScenes;
    public string InitialSceneName => SceneManagerResource.initialSceneName;

    public static SceneManager Instance { get; private set; }

    public Array<string> SceneNames;
    string CurrentSceneName;
    Scene CurrentScene;

    ColorRect FadeScene => GetNode<ColorRect>("%Fade");
    CanvasLayer CanvasLayer => GetNode<CanvasLayer>("CanvasLayer");
    CursorManager CursorManager;
    #endregion

    #region [Godot]
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

    public override async void _Ready()
    {
        Log("SceneManager is ready.", "SceneManager", LogTypeEnum.Framework);
        Log($"Found {ScenesPackedScenes.Count} scenes in ScenesPackedScenes.", "SceneManager", LogTypeEnum.Framework);
        foreach (var scene in ScenesPackedScenes)
            Log($"Scene: {scene.Key}", "SceneManager", LogTypeEnum.Framework);
        Log($"Initial scene: {InitialSceneName}", "SceneManager", LogTypeEnum.Framework);

        if (AudioBusLayout != null)
            AudioServer.SetBusLayout(AudioBusLayout);

        CursorManager = new CursorManager(CursorSets, MouseScaleFactor);

        await ChangeToScene(InitialSceneName);
    }
    #endregion

    #region [Lifecycle]
    public async Task ChangeToScene(string sceneName, InitData initData = null)
    {
        if (CurrentScene != null)
            await ExitCurrentScene();

        await StartScene(sceneName, initData);
    }

    private async Task StartScene(string sceneName, InitData initData = null)
    {
        sceneName = sceneName.ToLower();
        Log($"Starting scene {sceneName}", "SceneManager", LogTypeEnum.Framework);

        if (ScenesPackedScenes.Count == 0)
        {
            LogError("No scenes available to load. Please check the SceneManagerResource.", "SceneManager", LogTypeEnum.Framework);
            return;
        }

        CurrentSceneName = sceneName;
        CurrentScene = ScenesPackedScenes.TryGetValue(CurrentSceneName, out var packedScene) ? packedScene.Instantiate() as Scene : null;

        if (CurrentScene == null)
        {
            LogError($"Failed to instantiate scene {CurrentSceneName}", "SceneManager", LogTypeEnum.Framework);
            return;
        }

        CurrentScene.InitData = initData;
        CurrentScene.CursorManager = CursorManager;
        AddChild(CurrentScene);

        CurrentScene.Init();

        if (!CurrentScene.DontFadeInOnStart)
            await FadeIn(CurrentScene.FadeInTime);
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

        await FadeOut(CurrentScene.FadeOutTime);

        await CurrentScene.Close();
        CurrentScene.QueueFree();

        await ToSignal(CurrentScene, Node.SignalName.TreeExited);

        CurrentScene = null;
    }

    public async Task ChangeToDefaultNextScene()
    {
        if (CurrentScene.DefaultNextScene?.Length == 0)
        {
            Log($"No default next scene set for {CurrentScene.Name}, quitting instead.", "SceneManager", LogTypeEnum.Framework);
            await ExitCurrentScene();
            Quit();
        }
        else
        {
            Log($"Changing to default next scene {CurrentScene.DefaultNextScene}", "SceneManager", LogTypeEnum.Framework);
            await ChangeToScene(CurrentScene.DefaultNextScene);
        }
    }

    public void RestartScene() => GetTree().ReloadCurrentScene();

    public async void Quit()
    {
        await ExitCurrentScene();

        GetTree().Quit();
    }
    #endregion

    #region [Utility]
    // Use fadeout as direction because we are fading from black to the scene
    public async Task FadeIn(float duration) => await FadeHelper.TweenFadeModulate(FadeScene, FadeHelper.FadeDirectionEnum.Out, duration, transitionType: Tween.TransitionType.Cubic);
    public async Task FadeOut(float duration) => await FadeHelper.TweenFadeModulate(FadeScene, FadeHelper.FadeDirectionEnum.In, duration, transitionType: Tween.TransitionType.Cubic);
    public void SetFadeColor(Color color) => FadeScene.Color = color;
    public void SetFadeModulate(Color color) => FadeScene.Modulate = color;
    #endregion
}