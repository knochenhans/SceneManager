using CoreSystems;
using Godot;

public partial class SceneButton : Button
{
    public GameContext GameContext;

    public void Init(GameContext gameContext)
    {
        GameContext = gameContext;

        Pressed += () => GameContext.UISoundManager.PlaySound("click1");
        MouseEntered += () => GameContext.UISoundManager.PlaySound("hover");
    }
}
