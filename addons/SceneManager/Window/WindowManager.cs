using Godot;
using Godot.Collections;

using System.Linq;
using System.Threading.Tasks;

public partial class WindowManager : Control
{
    [Signal] public delegate void WindowOpenedEventHandler(string windowId, CustomWindow windowInstance, bool modal);
    [Signal] public delegate void WindowClosedEventHandler(string windowId);

    [Export] public Dictionary<string, PackedScene> WindowScenes;
    [Export] public float ScaleFactor = 1.0f;

    private sealed partial class WindowState : GodotObject
    {
        public Vector2 Position;
        public Vector2 Size;
    }

    private readonly Dictionary<string, CustomWindow> activeWindows = [];
    private readonly System.Collections.Generic.HashSet<string> pendingWindows = [];
    private readonly Dictionary<string, WindowState> windowStates = [];

    #region Window Queries
    public CustomWindow GetOpenWindow(string windowId) => activeWindows.TryGetValue(windowId, out var window) ? window : null;
    public System.Collections.Generic.IEnumerable<CustomWindow> GetOpenWindows() => activeWindows.Values;
    public bool IsWindowOpen(string windowId) => activeWindows.TryGetValue(windowId, out var window) && window.Visible;
    public bool IsAnyWindowOpen() => activeWindows.Values.Any(window => window.Visible);
    public bool IsAnyModalWindowOpen() => activeWindows.Values.Any(window => window.Visible && window.Modal);
    public CustomWindow GetTopmostVisibleWindow() => activeWindows.Values.LastOrDefault(window => window.Visible);
    #endregion

    #region Lifecycle Operations
    public void OpenWindow(string windowId, string windowTitle = "", Variant? data = null) => _ = OpenWindowAsync(windowId, windowTitle, data);

    public async Task OpenWindowAsync(string windowId, string windowTitle = "", Variant? data = null)
    {
        if (IsWindowOpen(windowId) || pendingWindows.Contains(windowId))
            return;

        pendingWindows.Add(windowId);

        try
        {
            if (!WindowScenes.TryGetValue(windowId, out var windowScene))
            {
                GD.PrintErr($"Window scene '{windowId}' not found in WindowManager.");
                return;
            }

            CustomWindow window = windowScene.Instantiate<CustomWindow>();
            window.Name = windowId;

            if (!string.IsNullOrEmpty(windowTitle))
                window.SetTitle(windowTitle);

            AddChild(window);
            activeWindows[windowId] = window;

            RestoreWindowState(windowId, window);

            window.CloseRequested += () => CloseWindow(windowId);

            UpdateInputState();
            EmitSignal(SignalName.WindowOpened, windowId, window, window.Modal);

            await window.OpenAsync(data);
            UpdateInputState();
        }
        finally
        {
            pendingWindows.Remove(windowId);
        }
    }

    public void CloseWindow(string windowId) => _ = CloseWindowAsync(windowId);

    public async Task CloseWindowAsync(string windowId)
    {
        if (pendingWindows.Contains(windowId))
            return;

        if (!activeWindows.TryGetValue(windowId, out var window))
            return;

        SaveWindowState(windowId, window);

        activeWindows.Remove(windowId);

        await window.CloseAsync();

        window.QueueFree();

        EmitSignal(SignalName.WindowClosed, windowId);
        UpdateInputState();
    }

    public async Task ToggleWindow(string windowId, string windowTitle = "")
    {
        if (IsWindowOpen(windowId))
            await CloseWindowAsync(windowId);
        else
            await OpenWindowAsync(windowId, windowTitle);
    }

    public void ToggleWindowVisibility(string windowId, string windowTitle = "")
    {
        var window = GetOpenWindow(windowId);
        window?.Toggle();
    }

    public void Uninit()
    {
        foreach (var windowId in activeWindows.Keys.ToList())
            _ = CloseWindowAsync(windowId);
    }
    #endregion

    #region Helpers
    private void SaveWindowState(string windowId, CustomWindow window)
    {
        windowStates[windowId] = new WindowState
        {
            Position = window.GlobalPosition,
            Size = window.Size
        };
    }

    private void RestoreWindowState(string windowId, CustomWindow window)
    {
        if (windowStates.TryGetValue(windowId, out var state))
        {
            window.GlobalPosition = state.Position;
            if (state.Size != Vector2.Zero)
                window.Size = state.Size;
        }
    }

    private void UpdateInputState()
    {
        MouseFilter = IsAnyModalWindowOpen()
            ? MouseFilterEnum.Stop
            : MouseFilterEnum.Ignore;
    }
    #endregion
}