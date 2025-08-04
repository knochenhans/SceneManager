using Godot;

public partial class OverlayMenu : ColorRect
{
    OptionGrid OptionGridNode => GetNode<OptionGrid>("%OptionGrid");

    public override void _Ready()
    {
        Visible = false;
        SelfModulate = new Color(0, 0, 0, SceneManager.Instance.OverlayMenuOpacity);
    }

    private void TweenSelfModulate(Color target, float duration = 0.3f)
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "self_modulate", target, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.InOut);
    }

    public void ShowMenu()
    {
        Visible = true;
        TweenSelfModulate(new Color(0, 0, 0, SceneManager.Instance.OverlayMenuOpacity));

        OptionGridNode.Init();
    }

    public void HideMenu()
    {
        Visible = false;
        TweenSelfModulate(new Color(0, 0, 0, 0));

        OptionGridNode.Clear();
    }
}
