using CoreSystems;
using Godot;
using Godot.Collections;
using static Logger;

public partial class Main : Node
{
    protected GameContext GameContext;
    protected SceneManager SceneManager => GetNodeOrNull<SceneManager>("SceneManager");

    #region [Godot]
    public override void _EnterTree()
    {
        base._EnterTree();

        GameContext = new GameContext();
        SceneManager.Init(GameContext);
    }
    #endregion
}
