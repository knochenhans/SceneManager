using Godot;

public partial class OverlayMenu : ColorRect
{
    OptionGrid OptionGridNode => GetNode<OptionGrid>("%OptionGrid");

    public override void _Ready()
    {
        Visible = false;
        SelfModulate = new Color(0, 0, 0, SceneManager.Instance.OverlayMenuOpacity);
    }

    public async void ShowMenu()
    {
        Visible = true;
        OptionGridNode.Init();

        await FadeManager.TweenFadeModulate(this, FadeManager.FadeDirectionEnum.Out, SceneManager.Instance.OverlayMenuFadeTime, SceneManager.Instance.OverlayMenuOpacity, "self_modulate");
    }

    public async void HideMenu()
    {
        Visible = false;
        await FadeManager.TweenFadeModulate(this, FadeManager.FadeDirectionEnum.In, SceneManager.Instance.OverlayMenuFadeTime, fadeProperty: "self_modulate");

        OptionGridNode.Clear();
    }
}
