using Godot;

namespace CoreSystems.GameOptions
{
    public partial class OptionsScene : CentralLayoutScene
    {
        [Export] OptionsContainer OptionGridNode;
        [Export] Button CloseButtonNode;

        public override void _Ready()
        {
            OptionGridNode.Init();
            CloseButtonNode.Pressed += OnCloseButtonPressed;
        }

        public static async void OnCloseButtonPressed() => await SceneManager.Instance.ChangeToDefaultNextScene();

        protected override void OnBackgroundInput(InputEvent @event)
        { }
    }
}