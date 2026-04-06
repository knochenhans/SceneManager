using CoreSystems;
using Godot;

public partial class HUD : CanvasLayer
{
    #region [Fields and Properties]
    protected Control HUDMarginContainer => GetNode<Control>("%HUDMarginContainer");
    protected GameContext GameContext;
    protected Node2D Player;
    #endregion

    #region [Lifecycle]
    public virtual void Init(GameContext gameContext, Node2D player = null)
    {
        GameContext = gameContext;
        Player = player;
    }

    public virtual void Uninit()
    {
    }
    #endregion

    // #region [Public]
    // public void EnableUIInput()
    // {
    //     HUDMarginContainer.MouseFilter = Control.MouseFilterEnum.Stop;
    // }

    // public void DisableUIInput()
    // {
    //     HUDMarginContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
    // }
    // #endregion
}