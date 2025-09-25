using System.Threading.Tasks;
using Godot;
using MenuEntryData = Godot.Collections.Dictionary<string, Godot.Variant>;

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

        InitGame();
        SaveStateManager = new SaveStateManager(this);
        // SaveStateManager.SaveGameState(initialState, "init");

        Input.MouseMode = DefaultMouseMode;

        CurrentGameState = GameState.Running;
    }


    public override void _Process(double delta)
    {
        if (CurrentGameState != GameState.Running)
            return;
    }

    public override async void _Input(InputEvent @event)
    {
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
                    await ToggleOverlayMenu("load");
                    break;
                case Key.F12:
                    await ToggleOverlayMenu("save");
                    break;
                case Key.Escape:
                    await ToggleOverlayMenu("options");
                    break;
            }
        }

        if (CurrentGameState != GameState.Running)
            return;
    }

    public virtual void InitGame(bool loadGame = false)
    {
    }

    protected async Task ToggleOverlayMenu(string overlayMenuName = "")
    {
        if (CurrentGameState == GameState.Running)
        {
            Pause();
            await SceneManager.Instance.ShowOverlayMenu(overlayMenuName);
        }
        else if (CurrentGameState == GameState.Paused)
            Resume();
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

        var initialState = StageManager.Instance.GetSaveData();
        SaveStateManager.SaveGameState(initialState, saveGameName);
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
        SceneManager.Instance.HideOverlayMenu();

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
}