using System.Linq;

using Godot;
using Godot.Collections;

using static Logger;

public partial class CentralLayoutScene : Scene
{
    #region [Fields and Properties]
    protected VBoxContainer ButtonsNode => GetNodeOrNull<VBoxContainer>("%Buttons");
    protected Array<SceneButton> SceneButtons;
    #endregion

    #region [Godot]
    public override void _Ready()
    {
        base._Ready();

        if (ButtonsNode != null)
            SceneButtons = [.. ButtonsNode.GetChildren().OfType<SceneButton>()];
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
