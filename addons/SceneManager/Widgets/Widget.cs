using Game.Utils;
using Godot;
using System.Threading.Tasks;

public partial class Widget : Control
{
	[Signal] public delegate void CloseButtonPressedEventHandler();

	[Export] public string WidgetTitle = "Widget";
	[Export] public bool ShowTitleBar = true;
	[Export] public bool EnableDragging = true;
	[Export] public bool EnableCloseButton = true;

	[ExportCategory("Visual Settings")]
	[Export] public bool Center = false;
	[Export] public float FadeInDuration = 0.2f;
	[Export] public float FadeOutDuration = 0.1f;
	[Export] public float Opacity = 1;

	Label TitleLabel => GetNode<Label>("%WidgetTitleLabel");
	Panel TitleBar => GetNode<Panel>("%TitleBar");
	Button CloseButton => GetNode<Button>("%CloseButton");

	private bool isDragging = false;
	private bool movedToTop = false;
	private Control parentControl = null;
	private Vector2 offset;

	public async override void _Ready()
	{
		parentControl = GetParentOrNull<Control>();
		Modulate = new Color(1, 1, 1, 0f);

		TitleLabel.Text = WidgetTitle;
		TitleBar.Visible = ShowTitleBar;
		CloseButton.Visible = EnableCloseButton;
		CloseButton.Pressed += OnCloseButtonPressed;
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
		else if (@event is InputEventKey keyEvent
			&& keyEvent.Keycode == Key.Escape
			&& keyEvent.Pressed
			&& !keyEvent.Echo)
		{
			OnCloseButtonPressed();
		}
	}

	public override void _Process(double delta)
	{
		if (!EnableDragging)
			return;

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
		EmitSignal(SignalName.CloseButtonPressed);
	}

	public async Task Open()
	{
		await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.In, FadeInDuration, Opacity);
	}

	public async Task Close()
	{
		await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.Out, FadeOutDuration, Opacity);
		QueueFree();
	}
}
