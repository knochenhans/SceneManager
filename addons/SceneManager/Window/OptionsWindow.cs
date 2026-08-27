using Godot;
namespace CoreSystems.GameOptions
{
    public partial class OptionsWindow : CustomWindow
    {
        [Export] OptionsContainer OptionsContainer;
        [Export] Button CloseButtonNode;

        public override void _Ready()
        {
            base._Ready();

            CloseButtonNode.Pressed += OnCloseButtonPressed;

            OptionsContainer?.Init();
        }
    }
}