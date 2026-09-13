using CoreSystems.GameOptions;
using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class WindowManager : Control
{
    #region Signals
    public Action<string, CustomWindow> WindowFocusedEventHandler;
    public Action<string, CustomWindow> WindowUnfocusedEventHandler;
    public Action<string, CustomWindow, bool> WindowOpenedEventHandler;
    public Action<string> WindowClosedEventHandler;
    public Action PauseRequestedEventHandler;
    public Action ResumeRequestedEventHandler;
    #endregion

    [Export] public Dictionary<string, PackedScene> WindowScenes { get; set; }
    [Export] public Dictionary<string, string> WindowScenePaths { get; set; } = [];
    [Export] public float ScaleFactor { get; set; } = 1.0f;

    Input.MouseModeEnum defaultGameplayMouseMode = Input.MouseModeEnum.Captured;

    struct WindowState
    {
        public Vector2 Position;
        public Vector2 Size;
    }

    readonly System.Collections.Generic.Dictionary<string, CustomWindow> activeWindows = [];
    readonly System.Collections.Generic.HashSet<string> pendingWindows = [];
    readonly System.Collections.Generic.Dictionary<string, WindowState> windowStates = [];

    #region Queries
    public CustomWindow GetOpenWindow(string windowId) => activeWindows.TryGetValue(windowId, out var window) ? window : null;
    public System.Collections.Generic.IEnumerable<CustomWindow> GetOpenWindows() => activeWindows.Values;
    public bool IsWindowOpen(string windowId) => activeWindows.TryGetValue(windowId, out var window) && IsInstanceValid(window) && window.Visible;
    public bool IsAnyWindowOpen() => activeWindows.Values.Any(w => IsInstanceValid(w) && w.Visible);
    public bool IsAnyModalWindowOpen() => activeWindows.Values.Any(w => IsInstanceValid(w) && w.Visible && w.Modal);
    public CustomWindow GetTopmostVisibleWindow() => activeWindows.Values.LastOrDefault(w => IsInstanceValid(w) && w.Visible);
    public string[] GetOpenWindowIDs() => [.. activeWindows.Keys];
    public string[] GetVisibleWindowIDs() => [.. activeWindows.Values.Where(w => IsInstanceValid(w) && w.Visible).Select(w => w.ID)];
    #endregion

    #region Lifecycle Operations
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        RequestBackgroundLoading();
    }

    public void RequestBackgroundLoading()
    {
        if (WindowScenePaths == null) return;

        foreach (var (id, path) in WindowScenePaths)
        {
            if (string.IsNullOrEmpty(path)) continue;

            var err = ResourceLoader.LoadThreadedRequest(path);
            if (err != Error.Ok)
            {
                Logger.LogError($"Failed to request background load for window '{id}' at '{path}': {err}", Logger.LogTypeEnum.UI);
            }
        }
    }

    public void OpenWindow(string windowId, string windowTitle = "", Variant? data = null) => ExecuteSync(() => OpenWindowAsync(windowId, windowTitle, data));
    public void CloseWindow(string windowId) => ExecuteSync(() => CloseWindowAsync(windowId));
    public void ShowWindow(string windowId) => ExecuteSync(() => ShowWindowAsync(windowId));
    public void HideWindow(string windowId) => ExecuteSync(() => HideWindowAsync(windowId));
    public void ToggleWindow(string windowId, string windowTitle = "") => ExecuteSync(() => ToggleWindowAsync(windowId, windowTitle));

    public void ToggleWindowVisibility(string windowId, string windowTitle = "")
    {
        if (!activeWindows.TryGetValue(windowId, out var window))
        {
            Logger.Log($"Window '{windowId}' is not loaded.", Logger.LogTypeEnum.Framework);
            return;
        }

        SetWindowVisibility(windowId, !window.Visible);
    }

    public CustomWindow PreloadWindow(string windowId, string windowTitle = "")
    {
        if (activeWindows.TryGetValue(windowId, out var existingWindow))
            return existingWindow;

        if (!pendingWindows.Add(windowId))
            return null;

        try
        {
            PackedScene windowScene = GetOrLoadWindowScene(windowId);
            if (windowScene == null)
            {
                Logger.LogError($"Window scene '{windowId}' could not be resolved or loaded in WindowManager.", Logger.LogTypeEnum.UI);
                return null;
            }

            CustomWindow window = windowScene.Instantiate<CustomWindow>();
            window.Name = windowId;

            if (!string.IsNullOrEmpty(windowTitle))
                window.SetTitle(windowTitle);

            window.Visible = false;

            AddChild(window);
            window.ID = windowId;
            activeWindows[windowId] = window;

            RestoreWindowState(windowId, window);
            InitWindowEvents(windowId, window);

            return window;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to preload window '{windowId}': {ex.Message}", Logger.LogTypeEnum.UI);
            if (activeWindows.Remove(windowId, out var failedWindow))
                failedWindow.QueueFree();
            return null;
        }
        finally
        {
            pendingWindows.Remove(windowId);
        }
    }

    public async Task<T> OpenWindowAsync<T>(string windowId, string windowTitle = "", Variant? data = null) where T : CustomWindow
    {
        var window = await OpenWindowAsync(windowId, windowTitle, data);
        return window as T;
    }

    public async Task<CustomWindow> OpenWindowAsync(string windowId, string windowTitle = "", Variant? data = null)
    {
        if (!activeWindows.TryGetValue(windowId, out var window))
        {
            window = PreloadWindow(windowId, windowTitle);
            if (window == null) return null;
        }

        if (!window.Visible)
        {
            window.Visible = true;
            UpdateUIState();

            WindowOpenedEventHandler?.Invoke(windowId, window, window.Modal);
            WindowFocusedEventHandler?.Invoke(windowId, window);

            if (window.Modal && activeWindows.Values.Count(w => w.Visible && w.Modal) == 1)
                PauseRequestedEventHandler?.Invoke();

            await window.OpenAsync(data);
        }

        return window;
    }

    public async Task ShowWindowAsync(string windowId) => SetWindowVisibility(windowId, visible: true);
    public async Task HideWindowAsync(string windowId) => SetWindowVisibility(windowId, visible: false);

    public async Task ToggleWindowVisibilityAsync(string windowId, string windowTitle = "")
    {
        if (!activeWindows.TryGetValue(windowId, out var window))
        {
            Logger.Log($"Window '{windowId}' is not loaded.", Logger.LogTypeEnum.Framework);
            return;
        }

        SetWindowVisibility(windowId, !window.Visible);
    }

    public async Task ToggleWindowAsync(string windowId, string windowTitle = "")
    {
        if (IsWindowOpen(windowId))
            await CloseWindowAsync(windowId);
        else
            await OpenWindowAsync(windowId, windowTitle);
    }

    public async Task CloseWindowAsync(string windowId)
    {
        if (pendingWindows.Contains(windowId) || !activeWindows.Remove(windowId, out var window))
            return;

        SaveWindowState(windowId, window);
        UninitWindowEvents(windowId, window);

        await window.CloseAsync();

        WindowUnfocusedEventHandler?.Invoke(windowId, window);
        WindowClosedEventHandler?.Invoke(windowId);

        UpdateUIState();

        if (!IsAnyModalWindowOpen())
            ResumeRequestedEventHandler?.Invoke();
        window.QueueFree();
    }

    public void Uninit()
    {
        var keys = activeWindows.Keys.ToList();
        foreach (var windowId in keys)
            _ = CloseWindowAsync(windowId);
    }
    #endregion

    #region Helpers
    private PackedScene GetOrLoadWindowScene(string windowId)
    {
        if (WindowScenes != null && WindowScenes.TryGetValue(windowId, out var exportedScene) && exportedScene != null)
            return exportedScene;

        if (WindowScenePaths != null && WindowScenePaths.TryGetValue(windowId, out var path) && !string.IsNullOrEmpty(path))
        {
            var status = ResourceLoader.LoadThreadedGetStatus(path);
            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                return (PackedScene)ResourceLoader.LoadThreadedGet(path);
            }

            return GD.Load<PackedScene>(path);
        }

        return null;
    }

    private void SetWindowVisibility(string windowId, bool visible)
    {
        if (pendingWindows.Contains(windowId) || !activeWindows.TryGetValue(windowId, out var window))
            return;

        if (visible)
        {
            window.Show();
            WindowFocusedEventHandler?.Invoke(windowId, window);
        }
        else
        {
            window.Hide();
            WindowUnfocusedEventHandler?.Invoke(windowId, window);
        }

        UpdateUIState();
    }

    private void InitWindowEvents(string windowId, CustomWindow window)
    {
        window.OpenRequested += OnOpen;
        window.CloseRequested += OnClose;
        window.ShowRequested += OnShow;
        window.HideRequested += OnHide;

        void OnOpen() => OpenWindow(windowId);
        void OnClose() => CloseWindow(windowId);
        void OnShow() => ShowWindow(windowId);
        void OnHide() => HideWindow(windowId);
    }

    private void UninitWindowEvents(string windowId, CustomWindow window)
    {
        // CustomWindow handlers automatically clear when QueueFree destroys the node
    }

    private void SaveWindowState(string windowId, CustomWindow window) => windowStates[windowId] = new WindowState
    {
        Position = window.GlobalPosition,
        Size = window.Size
    };

    private void RestoreWindowState(string windowId, CustomWindow window)
    {
        if (windowStates.TryGetValue(windowId, out var state))
        {
            window.GlobalPosition = state.Position;
            if (state.Size != Vector2.Zero)
                window.Size = state.Size;
        }
    }

    private void UpdateUIState()
    {
        bool anyWindowOpen = IsAnyWindowOpen();

        MouseFilter = IsAnyModalWindowOpen() ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
        Input.MouseMode = anyWindowOpen ? Input.MouseModeEnum.Visible : defaultGameplayMouseMode;
    }

    public void SetDefaultGameplayMouseMode(Input.MouseModeEnum mode)
    {
        defaultGameplayMouseMode = mode;
        UpdateUIState();
    }

    private static async void ExecuteSync(Func<Task> taskFunc)
    {
        try
        {
            await taskFunc();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unhandled window task exception: {ex}", Logger.LogTypeEnum.Framework);
        }
    }

    public async Task HandleCancelOrCloseTopmostAsync(string fallbackWindowId = "options_window", Action onFallbackOpened = null)
    {
        var topWindow = GetTopmostVisibleWindow();

        if (topWindow != null)
        {
            await CloseWindowAsync(topWindow.ID);
        }
        else if (!string.IsNullOrEmpty(fallbackWindowId))
        {
            var window = await OpenWindowAsync(fallbackWindowId);
            if (window is OptionsWindow optionsWindow && onFallbackOpened != null)
                optionsWindow.QuitButtonPressedEventHandler += onFallbackOpened;
        }
    }

    public void HandleCancelOrCloseTopmost(string fallbackWindowId = "options_window", Action onFallbackOpened = null) => ExecuteSync(() => HandleCancelOrCloseTopmostAsync(fallbackWindowId, onFallbackOpened));
    #endregion
}