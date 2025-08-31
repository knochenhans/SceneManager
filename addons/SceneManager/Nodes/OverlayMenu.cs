using System.Threading.Tasks;
using Godot;
using MenuEntryData = Godot.Collections.Dictionary<string, Godot.Variant>;


public partial class OverlayMenu : ColorRect
{
    [Signal] public delegate void ClosedEventHandler();
    [Signal] public delegate void EntrySelectedEventHandler(MenuEntryData entryData);

    OptionsContainer OptionGridNode;
    Button BackButtonNode;
    Button QuitButtonNode;

    protected Control InnerContainerNode => GetNodeOrNull<Control>("%InnerContainer");
    protected OverlayInner InnerNode;

    public override void _Ready()
    {
        Visible = false;
        SelfModulate = new Color(0, 0, 0, SceneManager.Instance.OverlayMenuOpacity);
    }

    public async Task ShowMenu(OverlayInner inner)
    {
        InnerNode = inner;
        InnerContainerNode.AddChild(InnerNode);
        InnerNode.EntrySelected += (entry) => EmitSignal(SignalName.EntrySelected, entry);
        InnerNode.BackButtonPressed += async () => await HideMenu();
        InnerNode.QuitButtonPressed += SceneManager.Instance.Quit;

        Visible = true;

        foreach (var control in inner.GetChildren())
            if (control is OptionsContainer optionGrid)
                OptionGridNode = optionGrid;

        OptionGridNode?.Init();

        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.Out, SceneManager.Instance.OverlayMenuFadeTime, SceneManager.Instance.OverlayMenuOpacity, "self_modulate", transitionType: Tween.TransitionType.Cubic);
    }

    public async Task HideMenu()
    {
        Visible = false;
        OptionGridNode?.DisableInput();
        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.In, SceneManager.Instance.OverlayMenuFadeTime, fadeProperty: "self_modulate", transitionType: Tween.TransitionType.Cubic);

        InnerNode.EntrySelected -= (entryName) => EmitSignal(SignalName.EntrySelected, entryName);

        OptionGridNode?.Clear();

        if (BackButtonNode != null)
            BackButtonNode.Pressed -= async () => await HideMenu();

        if (QuitButtonNode != null)
            QuitButtonNode.Pressed -= async () => await HideMenu();

        EmitSignal(SignalName.Closed);
    }
}
