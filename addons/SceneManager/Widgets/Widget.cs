using Godot;

using System.Threading.Tasks;

public partial class Widget : Control
{
    #region [Fields and Properties]
    [Signal] public delegate void CloseButtonPressedEventHandler();

    [Export] public string WidgetTitle;
    [Export] public bool ShowTitleBar = true;
    [Export] public bool EnableDragging = true;
    [Export] public bool EnableCloseButton = true;

    [ExportGroup("Visual Settings")]
    [Export] public bool Center = false;
    [Export] public float FadeInDuration = 0.2f;
    [Export] public float FadeOutDuration = 0.1f;
    [Export] public float Opacity = 1;

    Label TitleLabel => GetNode<Label>("%WidgetTitleLabel");
    Panel TitleBar => GetNode<Panel>("%TitleBar");
    Button CloseButtonTitleBar => GetNode<Button>("%CloseButtonTitleBar");
    Button CloseButton => GetNode<Button>("%CloseButton");

    bool isDragging = false;
    bool movedToTop = false;
    Control parentControl = null;
    Vector2 offset;

    public enum OpeningState
    {
        Opening,
        Opened,
        Closing,
        Closed
    }

    public OpeningState CurrentOpeningState = OpeningState.Closed;

    #endregion

    #region [Godot]
    public override void _Ready()
    {
        parentControl = GetParentOrNull<Control>();
        Modulate = new Color(1, 1, 1, 0f);

        TitleLabel.Text = WidgetTitle;
        TitleBar.Visible = ShowTitleBar;
        CloseButtonTitleBar.Visible = EnableCloseButton;
        CloseButtonTitleBar.Pressed += OnCloseButtonPressed;
        CloseButton.Pressed += OnCloseButtonPressed;

        GrabFocus();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (CurrentOpeningState != OpeningState.Opened)
            return;

        base._GuiInput(@event);

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
    #endregion

    #region [Events]
    public void OnCloseButtonPressed() => EmitSignal(SignalName.CloseButtonPressed);
    #endregion

    #region [Lifecycle]
    public async Task Open()
    {
        CurrentOpeningState = OpeningState.Opening;
        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.In, FadeInDuration, Opacity);
        CurrentOpeningState = OpeningState.Opened;
    }

    public async Task Close()
    {
        CurrentOpeningState = OpeningState.Closing;
        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.Out, FadeOutDuration, Opacity);
        CurrentOpeningState = OpeningState.Closed;
        QueueFree();
    }
    #endregion
}
