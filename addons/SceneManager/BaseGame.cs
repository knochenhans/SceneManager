using System.Threading.Tasks;
using Godot;
using MenuEntryData = Godot.Collections.Dictionary<string, Godot.Variant>;
using GameStateData = Godot.Collections.Dictionary<string, Godot.Variant>;

using static Logger;

public partial class BaseGame : Scene
{
    [ExportGroup("Game Settings")]
    [Export] public int gameVersion = 1;

    [ExportGroup("Mouse")]
    [Export] public Texture2D DefaultCursorTexture = null!;
    [Export] public Vector2 DefaultCursorHotspot = new(0, 0);
    [Export] public Input.MouseModeEnum DefaultMouseMode = Input.MouseModeEnum.Visible;

    public Camera2D Camera => GetViewport().GetCamera2D();

    protected NotificationManager NotificationManager => GetNodeOrNull<NotificationManager>("%NotificationManager");
    protected SaveStateManager SaveStateManager;

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

    public enum OverlayMenuState
    {
        Closed,
        Opening,
        Open,
        Closing,
    }

    protected OverlayMenuState CurrentOverlayMenuState = OverlayMenuState.Closed;

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

        Input.SetCustomMouseCursor(DefaultCursorTexture, Input.CursorShape.Arrow, DefaultCursorHotspot);

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
        if (CurrentOverlayMenuState == OverlayMenuState.Open)
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
                    ShowOverlayMenu("load");
                    break;
                case Key.F12:
                    ShowOverlayMenu("save");
                    break;
                case Key.Escape:
                    ShowOverlayMenu("options");
                    break;
            }
        }

        if (CurrentGameState != GameState.Running)
            return;
    }

    public virtual void InitGame(bool loadGame = false)
    {
    }

    protected void ShowOverlayMenu(string overlayMenuName = "")
    {
        if (CurrentGameState == GameState.Running)
        {
            Pause();
            CurrentOverlayMenuState = OverlayMenuState.Opening;
            SceneManager.Instance.ShowOverlayMenu(overlayMenuName);
            CurrentOverlayMenuState = OverlayMenuState.Open;
        }
    }

    protected virtual async void LoadGame(string saveGameName = "savegame")
    {
        await SaveStateManager.LoadGameState(saveGameName);
        NotificationManager.ShowNotification($"Game loaded from {saveGameName}.");

        if (CurrentGameState == GameState.Paused)
            Resume();
    }

    protected virtual void SaveGame(string saveGameName = "savegame")
    {
        if (CurrentGameState == GameState.Paused)
            Resume();

        NotificationManager.ShowNotification($"Game saved as {saveGameName}.");
    }

    protected virtual void Pause()
    {
        CurrentGameState = GameState.Paused;

        Input.MouseMode = Input.MouseModeEnum.Visible;

        Log("Game paused.", "Game", LogTypeEnum.Framework);
    }

    protected virtual void Resume()
    {
        Input.MouseMode = DefaultMouseMode;

        CurrentGameState = GameState.Running;

        Log("Game resumed.", "Game", LogTypeEnum.Framework);
    }

    public override async Task Close()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;

        await base.Close();
    }

    public void OnOverlayMenuEntrySelected(MenuEntryData entryData)
    {
        if (entryData.TryGetValue("load", out var entryName))
            LoadGame((string)entryName);
        else if (entryData.TryGetValue("save", out var entryName2))
            SaveGame((string)entryName2);
    }

    public void OnOverlayMenuClosed()
    {
        if (CurrentOverlayMenuState == OverlayMenuState.Open)
        {
            CurrentOverlayMenuState = OverlayMenuState.Closing;
            CurrentOverlayMenuState = OverlayMenuState.Closed;
        }
        Resume();
    }
}