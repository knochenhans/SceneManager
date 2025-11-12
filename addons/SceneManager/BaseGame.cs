using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MenuEntryData = Godot.Collections.Dictionary<string, Godot.Variant>;
using GameStateData = Godot.Collections.Dictionary<string, Godot.Variant>;

using static Logger;

public partial class BaseGame : Scene
{
    [Export] public NotificationManager NotificationManager;

    [ExportGroup("Widgets")]
    [Export] public Dictionary<string, PackedScene> WidgetScenes;

    [ExportGroup("Game Settings")]
    [Export] public int gameVersion = 1;

    protected Array<StageNode> CurrentStageNodes = [];

    public Camera2D Camera => GetViewport().GetCamera2D();
    public Control WidgetsNode => GetNode<Control>("%Widgets");
    protected CanvasLayer CanvasLayer => GetNodeOrNull<CanvasLayer>("CanvasLayer");
    protected CanvasLayer FixedCanvasLayer => GetNodeOrNull<CanvasLayer>("%FixedCanvas");

    public WidgetManager WidgetManager;
    protected SaveStateManager SaveStateManager;

    public SelectionManager SelectionManager => GetNodeOrNull<SelectionManager>("SelectionManager");
    protected TooltipManager TooltipManager => GetNodeOrNull<TooltipManager>("TooltipManager");
    protected MiniMap MiniMap => CanvasLayer.GetNodeOrNull<MiniMap>("%MiniMap");
    protected NavigationRegion2D NavigationRegion;

    public enum GameState
    {
        None,
        Loading,
        Running,
        Paused,
        GameOver,
    }

    protected GameState currentGameState = GameState.None;
    protected GameState CurrentGameState
    {
        get => currentGameState;
        set
        {
            if (currentGameState != value)
            {
                Log($"Game state changed from {currentGameState} to {value}.", "Game", LogTypeEnum.Framework);
                currentGameState = value;
            }
        }
    }

    public enum ControlState
    {
        None,
        Selecting,
        Dragging,
    }

    public ControlState CurrentControlState = ControlState.None;

    public override void _Ready()
    {
        CurrentGameState = GameState.Loading;

        CursorManager = new CursorManager(CursorSets, ScaleFactor);

        UISoundPlayer.Instance?.StopMusic();

        InitGame();
        SaveStateManager = new SaveStateManager(this);

        var initialState = new GameStateData
        {
            { "gameVersion", gameVersion },
        };
        SaveStateManager.SaveGameState(initialState, "init");

        Input.MouseMode = DefaultMouseMode;

        CurrentGameState = GameState.Running;

        Log("Game initialized.", "Game", LogTypeEnum.Framework);
    }

    public override void _Process(double delta)
    {
        if (CurrentGameState != GameState.Running)
            return;
    }

    public override void _Input(InputEvent @event)
    {
        if (CurrentGameState == GameState.Paused)
            return;

        if (@event is InputEventKey keyEvent && keyEvent.IsPressed())
        {
            switch (keyEvent.Keycode)
            {
                case Key.F5:
                    SaveGame();
                    break;
                case Key.F7:
                    LoadGame();
                    break;
                case Key.F11:
                    _ = WidgetManager.ToggleWidget("load", pauseGame: true);
                    break;
                case Key.F12:
                    _ = WidgetManager.ToggleWidget("save", pauseGame: true);
                    break;
                case Key.Escape:
                    _ = WidgetManager.ToggleWidget("options", pauseGame: true);
                    break;
            }
        }

        if (CurrentGameState != GameState.Running)
            return;
    }

    public virtual void InitGame(bool loadGame = false)
    {
        WidgetManager = new WidgetManager(this, WidgetsNode, WidgetScenes, FixedCanvasLayer.Scale.X);
        NavigationRegion = StageManager.Instance.CurrentStageScene.GetNodeOrNull<NavigationRegion2D>("NavigationRegion2D");
    }

    public virtual void UninitGame()
    {
        UninitStageNodes();

        WidgetManager = null;
    }

    protected virtual void InitStageNodes()
    {
        foreach (var stageNode in CurrentStageNodes)
            InitStageNode(stageNode);
    }

    protected virtual void InitStageNode(StageNode stageNode)
    {
        stageNode.Unpause();

        Log($"StageNode {stageNode.ID} has been initialized.", "Game", LogTypeEnum.EnterTree);
    }

    protected virtual void UninitStageNodes()
    {
        foreach (var stageNode in CurrentStageNodes)
            UninitStageNode(stageNode);
    }

    protected virtual void UninitStageNode(StageNode stageNode)
    {
        stageNode.Pause();

        Log($"StageNode {stageNode.ID} has been uninitialized.", "Game", LogTypeEnum.ExitTree);
    }

    public virtual async void LoadGame(string saveGameName = "savegame")
    {
        await SaveStateManager.LoadGameState(saveGameName);
        NotificationManager?.ShowNotification($"Game loaded from {saveGameName}.");

        if (CurrentGameState == GameState.Paused)
            Resume();
    }

    public virtual void SaveGame(string saveGameName = "savegame")
    {
        if (CurrentGameState == GameState.Paused)
            Resume();

        NotificationManager?.ShowNotification($"Game saved as {saveGameName}.");
    }

    public virtual void Pause()
    {
        CurrentGameState = GameState.Paused;

        LastMouseMode = Input.MouseMode;
        Input.MouseMode = Input.MouseModeEnum.Visible;

        Log("Game paused.", "Game", LogTypeEnum.Framework);
    }

    public virtual void Resume()
    {
        if (CurrentGameState != GameState.Paused)
            return;

        Input.MouseMode = LastMouseMode;

        CurrentGameState = GameState.Running;

        Log("Game resumed.", "Game", LogTypeEnum.Framework);
    }

    public override async Task Close()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        await base.Close();
    }

    public virtual void OnWidgetOpened(string widgetName, Widget widgetInstance)
    {
        Log($"Widget opened: {widgetName}", LogTypeEnum.UI);
    }

    public virtual void OnWidgetClosed(string widgetName)
    {
        Log($"Widget closed: {widgetName}", LogTypeEnum.UI);
    }
}