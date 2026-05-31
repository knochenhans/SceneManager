using System.Threading.Tasks;
using Godot;

public partial class HUDElement : PanelContainer
{
    [Export] public float OpeningDuration = 0.5f;
    [Export] public float ClosingDuration = 0.5f;

    Tween fadeTween;

    public bool IsOpen { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Visible = false;
        Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 0f);
    }

    public async Task Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        Visible = true;

        fadeTween?.Kill();
        fadeTween = CreateTween();
        await ToSignal(fadeTween.TweenProperty(this, "modulate:a", 1f, OpeningDuration), Tween.SignalName.Finished);
    }

    public async Task Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;

        fadeTween?.Kill();
        fadeTween = CreateTween();
        await ToSignal(fadeTween.TweenProperty(this, "modulate:a", 0f, ClosingDuration), Tween.SignalName.Finished);

        Visible = false;
    }
}