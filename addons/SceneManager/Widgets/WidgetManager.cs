using System.Threading.Tasks;

using Godot;
using Godot.Collections;

public class WidgetManager
{
    #region [Fields and Properties]
    readonly BaseGame Game;
    readonly Control widgetsNode;
    readonly Dictionary<string, PackedScene> widgetScenes;
    readonly Dictionary<string, Vector2> widgetPositions = [];
    readonly float scaleFactor;

    public Dictionary<string, Widget> ActiveWidgets = [];

    public WidgetManager(BaseGame game, Control widgetsNode, Dictionary<string, PackedScene> widgetScenes, float scaleFactor = 1.0f)
    {
        this.Game = game;
        this.widgetsNode = widgetsNode;
        this.widgetScenes = widgetScenes;
        this.scaleFactor = scaleFactor;

        widgetsNode.MouseFilter = Control.MouseFilterEnum.Ignore;
    }
    #endregion

    //TODO: Game-Verweis entfernen und stattdessen von Game selbst übernehmen lassen

    #region [Public]
    public bool IsWidgetOpen(string widgetName) => ActiveWidgets.ContainsKey(widgetName);
    public Widget GetOpenWidget(string widgetName) => ActiveWidgets.TryGetValue(widgetName, out var widget) ? widget : null;
    public bool IsAnyWidgetOpen() => ActiveWidgets.Count > 0;
    #endregion

    #region [Lifecycle]
    public void OpenWidget(string widgetName, string widgetTitle = "Widget", bool pauseGame = false) => _ = OpenWidgetAsync(widgetName, widgetTitle, pauseGame);

    public async Task OpenWidgetAsync(string widgetName, string widgetTitle = "", bool pauseGame = false)
    {
        if (widgetScenes != null && widgetScenes.TryGetValue(widgetName, out var widgetScene))
        {
            Game.CursorManager.ResetMouseCursor();
            Game.DisableUInput();

            if (pauseGame)
                Game.Pause();

            var widgetInstance = widgetScene.Instantiate<Widget>();
            ActiveWidgets[widgetName] = widgetInstance;
            widgetInstance.Name = widgetName;

            if (!string.IsNullOrEmpty(widgetTitle))
                widgetInstance.WidgetTitle = widgetTitle;

            if (widgetPositions.TryGetValue(widgetName, out var savedPosition))
                Callable.From(() => widgetInstance.GlobalPosition = savedPosition).CallDeferred();

            widgetInstance.CloseButtonPressed += () => CloseWidget(widgetName);

            if (widgetInstance.Center)
            {
                widgetsNode.Size = widgetsNode.GetViewport().GetVisibleRect().Size / scaleFactor;
                widgetInstance.SetAnchorsPreset(Control.LayoutPreset.Center);
                widgetInstance.SetOffsetsPreset(Control.LayoutPreset.Center);
            }

            widgetsNode.AddChild(widgetInstance);
            await widgetInstance.Open();

            Game.OnWidgetOpened(widgetName, widgetInstance);
            widgetsNode.MouseFilter = Control.MouseFilterEnum.Stop;
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
            ActiveWidgets.Remove(widgetName);
            widgetPositions[widgetName] = widgetToClose.GlobalPosition;
            await widgetToClose.Close();

            Game.OnWidgetClosed(widgetName);
            Game.Resume();
            widgetsNode.MouseFilter = Control.MouseFilterEnum.Ignore;

            Game.CursorManager.RestorePreviousMouseCursor();
            Game.EnableUIInput();
        }

    }

    public async Task ToggleWidget(string widgetName, string widgetTitle = "", bool pauseGame = false)
    {
        if (ActiveWidgets.ContainsKey(widgetName))
            await CloseWidgetAsync(widgetName);
        else
            OpenWidget(widgetName, widgetTitle, pauseGame);
    }
    #endregion
}
