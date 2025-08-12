using Godot;
using System;
using System.Threading.Tasks;

public partial class OptionsScene : Scene
{
    OptionGrid OptionGridNode => GetNode<OptionGrid>("%OptionGrid");

    public override void _Ready() => OptionGridNode.Init();

    public async void OnExitButtonPressed() => await SceneManager.Instance.ChangeToDefaultNextScene();

    protected override void OnBackgroundClicked(InputEvent @event)
    { }
}
