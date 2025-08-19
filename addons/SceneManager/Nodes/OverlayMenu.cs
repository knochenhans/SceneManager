using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class OverlayMenu : ColorRect
{
    [Signal] public delegate void ClosedEventHandler();

    OptionGrid OptionGridNode => GetNode<OptionGrid>("%OptionGrid");

    protected VBoxContainer ButtonsNode => GetNodeOrNull<VBoxContainer>("%Buttons");
    protected Array<SceneButton> OverlayButtons;

    public override void _Ready()
    {
        if (ButtonsNode != null)
            OverlayButtons = [.. ButtonsNode.GetChildren().Where(node => node is SceneButton).Cast<SceneButton>()];

        Visible = false;
        SelfModulate = new Color(0, 0, 0, SceneManager.Instance.OverlayMenuOpacity);
    }

    public async Task ShowMenu()
    {
        Visible = true;
        OptionGridNode.Init();

        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.Out, SceneManager.Instance.OverlayMenuFadeTime, SceneManager.Instance.OverlayMenuOpacity, "self_modulate", transitionType: Tween.TransitionType.Cubic);
    }

    public async Task HideMenu()
    {
        Visible = false;
        OptionGridNode.DisableInput();
        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.In, SceneManager.Instance.OverlayMenuFadeTime, fadeProperty: "self_modulate", transitionType: Tween.TransitionType.Cubic);

        OptionGridNode.Clear();
    }

    public void OnBackButtonPressed() => EmitSignal(SignalName.Closed);
}
