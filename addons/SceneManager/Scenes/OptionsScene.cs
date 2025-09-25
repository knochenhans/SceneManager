using Godot;

public partial class OptionsScene : Scene
{
    OptionsContainer OptionGridNode => GetNode<OptionsContainer>("%OptionGrid");

    public override void _Ready() => OptionGridNode.Init();

    public async void OnExitButtonPressed() => await SceneManager.Instance.ChangeToDefaultNextScene();

    protected override void OnBackgroundClicked(InputEvent @event)
    { }
}
