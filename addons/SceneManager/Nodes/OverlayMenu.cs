using System.Linq;
using Godot;
using Godot.Collections;

public partial class OverlayMenu : ColorRect
{
    [Signal] public delegate void ClosedEventHandler();

    OptionGrid OptionGridNode => GetNode<OptionGrid>("%OptionGrid");
    public UISoundPlayer UISoundPlayer;
    protected VBoxContainer ButtonsNode => GetNode<VBoxContainer>("%Buttons");
    protected Array<SceneButton> OverlayButtons;


    public override void _Ready()
    {
        OverlayButtons = [.. ButtonsNode.GetChildren().Where(node => node is SceneButton).Cast<SceneButton>()];

        Visible = false;
        SelfModulate = new Color(0, 0, 0, SceneManager.Instance.OverlayMenuOpacity);
    }

    public async void ShowMenu()
    {
        Visible = true;
        OptionGridNode.Init(UISoundPlayer);

        foreach (var button in OverlayButtons)
            button.UISoundPlayer = UISoundPlayer;

        await FadeManager.TweenFadeModulate(this, FadeManager.FadeDirectionEnum.Out, SceneManager.Instance.OverlayMenuFadeTime, SceneManager.Instance.OverlayMenuOpacity, "self_modulate");
    }

    public async void HideMenu()
    {
        Visible = false;
        OptionGridNode.DisableInput();
        await FadeManager.TweenFadeModulate(this, FadeManager.FadeDirectionEnum.In, SceneManager.Instance.OverlayMenuFadeTime, fadeProperty: "self_modulate");

        OptionGridNode.Clear();
    }

    public void OnBackButtonPressed() => EmitSignal(SignalName.Closed);
}
