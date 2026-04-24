using System.Linq;
using System.Threading.Tasks;

using Godot;
using Godot.Collections;

public partial class WidgetManager : Control
{
    #region [Fields and Properties]
    [Signal] public delegate void WidgetOpenedEventHandler(string widgetName, Widget widgetInstance, bool pauseGame);
    [Signal] public delegate void WidgetClosedEventHandler(string widgetName);

    [Export] public float ScaleFactor = 1.0f;

    Dictionary<string, PackedScene> WidgetScenes;
    readonly Dictionary<string, Vector2> WidgetPositions = [];

    public Dictionary<string, Widget> ActiveWidgets = [];

    public void Init(Dictionary<string, PackedScene> widgetScenes)
    {
        WidgetScenes = widgetScenes;
    }

    public void Uninit()
    {
        foreach (var widgetName in ActiveWidgets.Keys.ToList())
            _ = CloseWidgetAsync(widgetName);
    }
    #endregion

    //TODO: Game-Verweis entfernen und stattdessen von Game selbst übernehmen lassen

    #region [Public]
    public bool IsWidgetOpen(string widgetName) => ActiveWidgets.ContainsKey(widgetName);
    public Widget GetOpenWidget(string widgetName) => ActiveWidgets.TryGetValue(widgetName, out var widget) ? widget : null;
    public bool IsAnyWidgetOpen() => ActiveWidgets.Count > 0;
    #endregion

    #region [Lifecycle]
    public void OpenWidget(string widgetName, string widgetTitle = "", Variant? data = null) => _ = OpenWidgetAsync(widgetName, widgetTitle, data);

    public async Task OpenWidgetAsync(string widgetName, string widgetTitle = "", Variant? data = null)
    {
        if (IsWidgetOpen(widgetName))
            return;

        if (WidgetScenes != null && WidgetScenes.TryGetValue(widgetName, out var widgetScene))
        {
            var widgetInstance = widgetScene.Instantiate<Widget>();
            EmitSignal(SignalName.WidgetOpened, widgetName, widgetInstance, widgetInstance.Modal);
            ActiveWidgets[widgetName] = widgetInstance;
            widgetInstance.Name = widgetName;

            if (!string.IsNullOrEmpty(widgetTitle))
                widgetInstance.WidgetTitle = widgetTitle;

            if (WidgetPositions.TryGetValue(widgetName, out var savedPosition))
                Callable.From(() => widgetInstance.GlobalPosition = savedPosition).CallDeferred();

            widgetInstance.CloseButtonPressed += () => CloseWidget(widgetName);

            if (widgetInstance.Center)
            {
                // Size = GetViewport().GetVisibleRect().Size / ScaleFactor;
                widgetInstance.SetAnchorsPreset(LayoutPreset.Center);
                widgetInstance.SetOffsetsPreset(LayoutPreset.Center);
            }

            AddChild(widgetInstance);
            await widgetInstance.Open(data);

            if (widgetInstance.Modal)
                MouseFilter = MouseFilterEnum.Stop;
        }
        else
        {
            Logger.LogError($"Widget scene '{widgetName}' not found.", "WidgetManager", Logger.LogTypeEnum.UI);
        }
    }

    public void CloseWidget(string widgetName) => _ = CloseWidgetAsync(widgetName);

    public async Task CloseWidgetAsync(string widgetName)
    {
        if (ActiveWidgets.TryGetValue(widgetName, out var widgetToClose))
        {
            MouseFilter = MouseFilterEnum.Ignore;

            ActiveWidgets.Remove(widgetName);
            WidgetPositions[widgetName] = widgetToClose.GlobalPosition;
            await widgetToClose.Close();

            EmitSignal(SignalName.WidgetClosed, widgetName);
        }
    }

    public async Task ToggleWidget(string widgetName, string widgetTitle = "")
    {
        if (ActiveWidgets.ContainsKey(widgetName))
            await CloseWidgetAsync(widgetName);
        else
            OpenWidget(widgetName, widgetTitle);
    }
    #endregion
}
