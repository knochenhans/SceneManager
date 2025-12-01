using Godot;

public partial class OptionsWidget : Widget
{
    OptionsContainer OptionsContainer => GetNodeOrNull<OptionsContainer>("%OptionsContainer");
    Button QuitButtonNode => GetNodeOrNull<Button>("%QuitButton");

    public override void _Ready()
    {
        base._Ready();

        QuitButtonNode.Pressed += () =>
        {
            OnCloseButtonPressed();
            SceneManager.Instance.Quit();
        };

        OptionsContainer?.Init();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventKey keyEvent && keyEvent.IsPressed())
            if (keyEvent.Keycode == Key.Escape)
                OnCloseButtonPressed();
    }
}