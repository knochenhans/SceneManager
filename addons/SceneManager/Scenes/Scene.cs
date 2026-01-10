using System;
using System.Linq;
using System.Threading.Tasks;

using Godot;
using Godot.Collections;

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

    [Export] public string DefaultNextScene = "";
    [Export] bool PlayUIMusic = false;

    [ExportGroup("Mouse")]
    [Export] public Input.MouseModeEnum DefaultMouseMode = Input.MouseModeEnum.Visible;

    [ExportGroup("Fade Settings")]
    [Export] public bool DontFadeInOnStart = false;
    [Export] public float FadeInTime = 0.5f;
    [Export] public float FadeOutTime = 0.5f;
    [Export] public float LifeTime = 0.0f;

    protected ColorRect BackgroundNode => GetNodeOrNull<ColorRect>("SceneBackground");
    protected SceneStateEnum SceneState = SceneStateEnum.TransitioningIn;

    Timer LifeTimerNode => GetNode<Timer>("LifeTimer");

    public CursorManager CursorManager = null!;
    protected Input.MouseModeEnum LastMouseMode;
    public InitData InitData;
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
    #endregion

    #region [Events]
    protected virtual void OnBackgroundInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.Pressed)
        {
            UISoundPlayer.Instance.PlaySound("click1");
            ChangeToNextScene();
        }
    }

    public virtual void OnWidgetOpened(string widgetName, Widget widgetInstance)
    {
    }

    public virtual void OnWidgetClosed(string widgetName)
    {
    }
    #endregion

    #region [Lifecycle]
    public virtual void Init()
    {
        if (UISoundPlayer.Instance == null)
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
            UISoundPlayer.Instance.StartOrKeepMusic();
        else
            UISoundPlayer.Instance.StopMusic();

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

