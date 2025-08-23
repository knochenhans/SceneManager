using System.Threading.Tasks;
using Godot;

public partial class OverlayMenu : ColorRect
{
    [Signal] public delegate void ClosedEventHandler();

    OptionGrid OptionGridNode;
    Button BackButtonNode;
    Button QuitButtonNode;

    protected Control ButtonsContainerNode => GetNodeOrNull<Control>("%ButtonsContainer");

    public override void _Ready()
    {
        Visible = false;
        SelfModulate = new Color(0, 0, 0, SceneManager.Instance.OverlayMenuOpacity);
    }

    public async Task ShowMenu(PackedScene innerPackedScene)
    {
        var buttons = innerPackedScene.Instantiate<Control>();
        ButtonsContainerNode.AddChild(buttons);

        Visible = true;

        foreach (var control in buttons.GetChildren())
            if (control is OptionGrid optionGrid)
                OptionGridNode = optionGrid;

        OptionGridNode?.Init();

        BackButtonNode = buttons.GetNodeOrNull<Button>("%BackButton");
        QuitButtonNode = buttons.GetNodeOrNull<Button>("%QuitButton");

        if (BackButtonNode != null)
            BackButtonNode.Pressed += OnBackButtonPressed;

        if (QuitButtonNode != null)
            QuitButtonNode.Pressed += OnQuitButtonPressed;

        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.Out, SceneManager.Instance.OverlayMenuFadeTime, SceneManager.Instance.OverlayMenuOpacity, "self_modulate", transitionType: Tween.TransitionType.Cubic);
    }

    public async Task HideMenu()
    {
        Visible = false;
        OptionGridNode?.DisableInput();
        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.In, SceneManager.Instance.OverlayMenuFadeTime, fadeProperty: "self_modulate", transitionType: Tween.TransitionType.Cubic);

        OptionGridNode?.Clear();

        if (BackButtonNode != null)
            BackButtonNode.Pressed -= OnBackButtonPressed;

        if (QuitButtonNode != null)
            QuitButtonNode.Pressed -= OnQuitButtonPressed;
    }

    public void OnBackButtonPressed() => EmitSignal(SignalName.Closed);
    public void OnQuitButtonPressed() => SceneManager.Instance.Quit();
}
