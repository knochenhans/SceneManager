using Godot;

[Tool]
public partial class CustomWindowTitlePanel : Panel
{
    [Export] public Label TitleLabel;
    [Export] public Button CloseButton;

    [Signal] public delegate void CloseButtonPressedEventHandler();

    bool dragging;
    Vector2 dragOffset;
    Control windowControl;

    public override void _Ready() => CloseButton.Pressed += OnCloseButtonPressed;

    private void OnCloseButtonPressed() => EmitSignal(SignalName.CloseButtonPressed);

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed)
            {
                dragging = true;
                dragOffset = windowControl.GlobalPosition - mouseButton.GlobalPosition;
            }
            else
            {
                dragging = false;
            }

            AcceptEvent();
        }
        else if (@event is InputEventMouseMotion motion && dragging)
        {
            windowControl.GlobalPosition = motion.GlobalPosition + dragOffset;
            AcceptEvent();
        }
    }

    public void Init(Control window) => windowControl = window;

    public void SetTitle(string title)
    {
        if (IsInstanceValid(TitleLabel))
        {
            TitleLabel.Text = title;

            if (Engine.IsEditorHint())
            {
                TitleLabel.QueueRedraw();
            }
        }
    }
}