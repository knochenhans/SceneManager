using System.Threading.Tasks;

using CoreSystems;

using Godot;

using static Logger;

using InitData = Godot.Collections.Dictionary<string, Godot.Variant>;


[GlobalClass]
public partial class Scene : Node
{
    #region [Fields and Properties]
    public enum SceneStateEnum
    {
        Idle,
        TransitioningIn,
        TransitioningOut
    }

    [Export] Timer LifeTimerNode;
    [Export] protected ColorRect BackgroundNode;

    [ExportGroup("Transitions")]
    [Export] public string DefaultNextScene = "";
    [Export(PropertyHint.Enum, "ui_accept,ui_cancel,ui_focus_next,ui_focus_prev")] public string SkipInputAction = "ui_accept";
    [Export] public bool AllowInputSkip = false;
    [Export] bool PlayUIMusic = false;

    [ExportGroup("Mouse")]
    [Export] public Input.MouseModeEnum DefaultMouseMode = Input.MouseModeEnum.Visible;

    [ExportGroup("Fade Settings")]
    [Export] public bool DontFadeInOnStart = false;
    [Export] public float FadeInTime = 0.5f;
    [Export] public float FadeOutTime = 0.5f;
    [Export] public float LifeTime = 0.0f;

    protected SceneStateEnum SceneState = SceneStateEnum.TransitioningIn;

    protected Input.MouseModeEnum LastMouseMode;
    public CursorManager CursorManager { get; private set; }
    public InitData InitData { get; private set; } = [];

    public GameContext GameContext;
    #endregion

    #region [Godot]
    public override void _Ready()
    {
        Log($"Starting scene {SceneFilePath}", "SceneManager", LogTypeEnum.Framework);
    }

    public override void _Input(InputEvent @event)
    {
        if (SceneState != SceneStateEnum.Idle)
            return;

        base._Input(@event);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (SceneState != SceneStateEnum.Idle || !AllowInputSkip)
            return;

        if (!string.IsNullOrEmpty(SkipInputAction) && @event.IsActionPressed(SkipInputAction))
        {
            GetTree().Root.SetInputAsHandled();
            ChangeToNextScene();
        }
    }
    #endregion

    #region [Events]
    protected virtual void OnBackgroundInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
        {
            GameContext.UISoundManager.PlaySound("click1");
            ChangeToNextScene();
        }
    }

    public virtual void OnWindowFocused(string windowName, CustomWindow windowInstance)
    {
    }

    public virtual void OnWindowUnfocused(string windowName, CustomWindow windowInstance)
    {
    }

    public virtual void OnWindowOpened(string windowName, CustomWindow windowInstance, bool modal = false)
    {
    }

    public virtual void OnWindowClosed(string windowName)
    {
    }
    #endregion

    #region [Lifecycle]
    public virtual void Init(GameContext gameContext, InitData initData, CursorManager cursorManager)
    {
        GameContext = gameContext;
        InitData = initData;
        CursorManager = cursorManager;

        if (GameContext.UISoundManager == null)
            LogError("UISoundPlayer instance is null!", "SceneManager", LogTypeEnum.Framework);

        if (BackgroundNode != null)
            BackgroundNode.GuiInput += OnBackgroundInput;

        if (LifeTime > 0)
        {
            LifeTimerNode.WaitTime = LifeTime;
            LifeTimerNode.Start();
            LifeTimerNode.Timeout += ChangeToNextScene;
            Log($"Scene {Name} will change to next scene after {LifeTime} seconds.", "Scene", LogTypeEnum.Framework);
        }

        if (PlayUIMusic)
            GameContext.UISoundManager.StartOrKeepMusic();
        else
            GameContext.UISoundManager.StopMusic();

        LastMouseMode = Input.MouseMode;

        SceneState = SceneStateEnum.Idle;
    }

    protected async void ChangeToNextScene()
    {
        SceneState = SceneStateEnum.TransitioningOut;
        await SceneManager.Instance.ChangeToDefaultNextScene();
    }

    public virtual void Pause()
    {
    }

    public virtual void Resume()
    {
    }

    public async virtual Task Close()
    {
    }
    #endregion

    #region [Utility]
    public virtual void DisableInput() => BackgroundNode.SetBlockSignals(true);
    #endregion
}

