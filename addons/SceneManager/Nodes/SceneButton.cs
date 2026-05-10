using CoreSystems;
using Godot;

public partial class SceneButton : Button
{
    public GameContext GameContext;

    public void Init(GameContext gameContext)
    {
        GameContext = gameContext;

        Pressed += () => GameContext.UISoundPlayer.PlaySound("click1");
        MouseEntered += () => GameContext.UISoundPlayer.PlaySound("hover");
    }
}
