using System.Linq;
using CoreSystems;
using Godot;
using Godot.Collections;

public partial class CentralLayoutScene : Scene
{
    #region [Fields and Properties]
    [Export] protected VBoxContainer ButtonsNode;

    protected Array<SceneButton> SceneButtons;
    #endregion

    #region [Godot]
    public override void _Ready()
    {
        base._Ready();

        if (ButtonsNode != null)
            SceneButtons = [.. ButtonsNode.GetChildren().OfType<SceneButton>()];

        if (SceneButtons == null)
            return;

        foreach (var button in SceneButtons)
            button.Init(GameContext);
    }
    #endregion

    #region [Utility]
    public override void DisableInput()
    {
        base.DisableInput();
        if (SceneButtons != null)
        {
            foreach (var button in SceneButtons)
                button.SetBlockSignals(true);
        }
    }
    #endregion
}
