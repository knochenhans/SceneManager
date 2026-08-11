using CoreSystems;
using Godot;

public partial class Main : Node
{
    [Export] public SceneManager SceneManager;
    [Export] public UISoundManager UISoundManager;


    #region [Godot]
    public override void _EnterTree()
    {
        base._EnterTree();

        GameContext = new GameContext();
        SceneManager.Init(GameContext);

        GameContext.UISoundManager = UISoundManager;
    }
    #endregion
}
