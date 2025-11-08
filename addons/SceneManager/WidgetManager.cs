using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public class WidgetManager(BaseGame game, Control widgetsNode, Dictionary<string, PackedScene> widgetScenes)
{
	private readonly BaseGame game = game;
	private readonly Control widgetsNode = widgetsNode;
	private readonly Dictionary<string, PackedScene> widgetScenes = widgetScenes;
	private readonly Dictionary<string, Vector2> widgetPositions = [];

	public Dictionary<string, Widget> ActiveWidgets = [];

    public void OpenWidget(string widgetName, string widgetTitle = "Widget")
	{
		if (widgetScenes != null && widgetScenes.TryGetValue(widgetName, out var widgetScene))
		{
			var widgetInstance = widgetScene.Instantiate<Widget>();
			widgetInstance.Name = widgetName;
			widgetInstance.WidgetTitle = widgetTitle;
			if (widgetPositions.TryGetValue(widgetName, out var savedPosition))
			{
				Callable.From(() => widgetInstance.GlobalPosition = savedPosition).CallDeferred();
			}

			widgetsNode.AddChild(widgetInstance);
			ActiveWidgets[widgetName] = widgetInstance;

			game.OnWidgetOpened(widgetName, widgetInstance);
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
			widgetPositions[widgetName] = widgetToClose.GlobalPosition;
			await widgetToClose.Close();
			ActiveWidgets.Remove(widgetName);

			game.OnWidgetClosed(widgetName);
		}
	}

	public void ToggleWidget(string widgetName, string widgetTitle = "Widget")
	{
		if (ActiveWidgets.ContainsKey(widgetName))
			CloseWidget(widgetName);
		else
			OpenWidget(widgetName, widgetTitle);
	}
}
