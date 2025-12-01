using System.Threading.Tasks;

using Godot;
using Godot.Collections;

public class WidgetManager
{
    #region [Fields and Properties]
    readonly Scene game;
    readonly Control widgetsNode;
    readonly Dictionary<string, PackedScene> widgetScenes;
    readonly Dictionary<string, Vector2> widgetPositions = [];
    readonly float scaleFactor;

    public Dictionary<string, Widget> ActiveWidgets = [];

    public WidgetManager(Scene game, Control widgetsNode, Dictionary<string, PackedScene> widgetScenes, float scaleFactor = 1.0f)
    {
        this.game = game;
        this.widgetsNode = widgetsNode;
        this.widgetScenes = widgetScenes;
        this.scaleFactor = scaleFactor;

        widgetsNode.MouseFilter = Control.MouseFilterEnum.Ignore;
    }
    #endregion

    //TODO: Game-Verweis entfernen und stattdessen von Game selbst übernehmen lassen

    #region [Lifecycle]
    public void OpenWidget(string widgetName, string widgetTitle = "Widget", bool pauseGame = false) => _ = OpenWidgetAsync(widgetName, widgetTitle, pauseGame);

    public async Task OpenWidgetAsync(string widgetName, string widgetTitle = "", bool pauseGame = false)
    {
        if (widgetScenes != null && widgetScenes.TryGetValue(widgetName, out var widgetScene))
        {
            if (pauseGame)
                game.Pause();

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

            game.OnWidgetOpened(widgetName, widgetInstance);
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

            game.OnWidgetClosed(widgetName);
            game.Resume();
            widgetsNode.MouseFilter = Control.MouseFilterEnum.Ignore;
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
