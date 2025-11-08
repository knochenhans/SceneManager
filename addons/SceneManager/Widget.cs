using Game.Utils;
using Godot;

public partial class Widget : Control
{
	[Export] public string WidgetTitle = "Widget";
	[Export] public bool EnableDragging = true;
	
	[Export] public float FadeInDuration = 0.2f;
	[Export] public float FadeOutDuration = 0.1f;
	[Export] public float Opacity = 1;

	Label TitleLabel => GetNode<Label>("%WidgetTitleLabel");

	private bool isDragging = false;
	private bool movedToTop = false;
	private Control parentControl = null;
	private Vector2 offset;

	public async override void _Ready()
	{
		parentControl = GetParent() as Control;
		Modulate = new Color(1, 1, 1, 0f);

		await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.In, FadeInDuration, Opacity);
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent
			&& mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				offset = GetGlobalMousePosition() - GlobalPosition;
				isDragging = true;
			}
			else
			{
				isDragging = false;
				movedToTop = false;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (!isDragging)
			return;

		Vector2 globalMouse = GetGlobalMousePosition();
		Vector2 targetGlobal = globalMouse - offset;

		if (parentControl != null)
		{
			Vector2 parentGlobal = parentControl.GlobalPosition;
			Vector2 minGlobal = parentGlobal;
			Vector2 maxGlobal = parentGlobal + parentControl.Size - Size;

			targetGlobal.X = Mathf.Clamp(targetGlobal.X, minGlobal.X, maxGlobal.X);
			targetGlobal.Y = Mathf.Clamp(targetGlobal.Y, minGlobal.Y, maxGlobal.Y);
		}

		if (!movedToTop && parentControl != null)
		{
			parentControl.MoveChild(this, parentControl.GetChildCount() - 1);
			movedToTop = true;
		}

		GlobalPosition = targetGlobal;
	}

	public async void OnCloseButtonPressed()
    {
        await Close();
    }

    public async System.Threading.Tasks.Task Close()
    {
        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.Out, FadeOutDuration, Opacity);
        QueueFree();
    }
}
