using CoreSystems;
using Godot;

public partial class Main : Node
{
    protected GameContext GameContext;
    [Export] public SceneManager SceneManager;

    #region [Godot]
    public override void _EnterTree()
    {
        base._EnterTree();

        GameContext = new GameContext();
        SceneManager.Init(GameContext);
    }
    #endregion
}
