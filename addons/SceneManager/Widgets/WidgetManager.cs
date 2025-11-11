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

	public void OpenWidget(string widgetName, string widgetTitle = "Widget", bool pauseGame = false) => _ = OpenWidgetAsync(widgetName, widgetTitle, pauseGame);

	public async Task OpenWidgetAsync(string widgetName, string widgetTitle = "Widget", bool pauseGame = false)
	{
		if (widgetScenes != null && widgetScenes.TryGetValue(widgetName, out var widgetScene))
		{
			if (pauseGame)
				game.Pause();

			var widgetInstance = widgetScene.Instantiate<Widget>();
			widgetInstance.Name = widgetName;
			widgetInstance.WidgetTitle = widgetTitle;
			if (widgetPositions.TryGetValue(widgetName, out var savedPosition))
			{
				Callable.From(() => widgetInstance.GlobalPosition = savedPosition).CallDeferred();
			}
			widgetInstance.CloseButtonPressed += () => CloseWidget(widgetName);

			widgetsNode.AddChild(widgetInstance);
			await widgetInstance.Open();
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
			ActiveWidgets.Remove(widgetName);
			widgetPositions[widgetName] = widgetToClose.GlobalPosition;
			await widgetToClose.Close();

			game.OnWidgetClosed(widgetName);
			game.Resume();
		}
	}

	public async Task ToggleWidget(string widgetName, string widgetTitle = "Widget", bool pauseGame = false)
	{
		if (ActiveWidgets.ContainsKey(widgetName))
			await CloseWidgetAsync(widgetName);
		else
			OpenWidget(widgetName, widgetTitle, pauseGame);
	}
}
