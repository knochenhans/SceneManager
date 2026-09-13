using System;
using Godot;
namespace CoreSystems.GameOptions
{
    public partial class OptionsWindow : CustomWindow
    {
        public Action QuitButtonPressedEventHandler;

        [Export] OptionsContainer OptionsContainer;
        [Export] Button CloseButtonNode;
        [Export] Button QuitButtonNode;

        public override void _Ready()
        {
            base._Ready();

            CloseButtonNode.Pressed += OnCloseButtonPressed;
            QuitButtonNode.Pressed += OnQuitButtonPressed;

            OptionsContainer?.Init();
        }

        private void OnQuitButtonPressed() => QuitButtonPressedEventHandler?.Invoke();
    }
}