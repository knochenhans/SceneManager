using Godot;
using System;

public partial class GameOverOverlayInner : OverlayInner
{
    Button RetryButtonNode => GetNodeOrNull<Button>("%RetryButton");

    public override void _Ready()
    {
        base._Ready();

        RetryButtonNode.Pressed += OnRetryButtonPressed;
    }

    private void OnRetryButtonPressed() => EmitSignal(SignalName.ButtonPressed, "retry");
}
