using Godot;

public partial class OptionsScene : CentralLayoutScene
{
    OptionsContainer OptionGridNode => GetNode<OptionsContainer>("%OptionGrid");

    public override void _Ready() => OptionGridNode.Init();

    public static async void OnExitButtonPressed() => await SceneManager.Instance.ChangeToDefaultNextScene();

    protected override void OnBackgroundInput(InputEvent @event)
    { }
}
