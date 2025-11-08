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

    public async void ShowMenu(OverlayInner inner)
    {
        InnerNode = inner;
        InnerContainerNode.AddChild(InnerNode);
        InnerNode.EntrySelected += (entry) => EmitSignal(SignalName.EntrySelected, entry);
        InnerNode.ButtonPressed += (buttonID) =>
        {
            if (buttonID == "back")
                HideMenu();
            else if (buttonID == "quit")
                SceneManager.Instance.Quit();
        };

        Visible = true;

        foreach (var control in inner.GetChildren())
            if (control is OptionsContainer optionGrid)
                OptionGridNode = optionGrid;

        OptionGridNode?.Init();

        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.In, SceneManager.Instance.OverlayMenuFadeTime, SceneManager.Instance.OverlayMenuOpacity, "self_modulate", transitionType: Tween.TransitionType.Cubic);
    }

    public async void HideMenu()
    {
        Visible = false;
        OptionGridNode?.DisableInput();
        await FadeHelper.TweenFadeModulate(this, FadeHelper.FadeDirectionEnum.Out, SceneManager.Instance.OverlayMenuFadeTime, fadeProperty: "self_modulate", transitionType: Tween.TransitionType.Cubic);

        if (InnerNode != null)
            InnerNode.EntrySelected -= (entryName) => EmitSignal(SignalName.EntrySelected, entryName);

        OptionGridNode?.Clear();

        if (BackButtonNode != null)
            BackButtonNode.Pressed -= HideMenu;

        if (QuitButtonNode != null)
            QuitButtonNode.Pressed -= HideMenu;

        EmitSignal(SignalName.Closed);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.IsPressed() && keyEvent.Keycode == Key.Escape)
            HideMenu();
    }
}
