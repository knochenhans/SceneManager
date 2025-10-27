using Godot;

public partial class OverlayInner : VBoxContainer
{
    [Signal] public delegate void EntrySelectedEventHandler(Variant entryData);
    [Signal] public delegate void ButtonPressedEventHandler(string buttonID);

    Button BackButtonNode => GetNodeOrNull<Button>("%BackButton");
    Button QuitButtonNode => GetNodeOrNull<Button>("%QuitButton");

    public override void _Ready()
    {
        if (BackButtonNode != null)
            BackButtonNode.Pressed += () => EmitSignal(SignalName.ButtonPressed, "back");

        if (QuitButtonNode != null)
            QuitButtonNode.Pressed += () => EmitSignal(SignalName.ButtonPressed, "quit");
    }
}
