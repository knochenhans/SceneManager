using CoreSystems;
using Godot;

public partial class HUD : CanvasLayer
{
    #region [Fields and Properties]
    protected Control HUDMarginContainer => GetNode<Control>("%HUDMarginContainer");
    protected GameContext GameContext;
    #endregion

    #region [Lifecycle]
    public virtual void Init(GameContext gameContext)
    {
        GameContext = gameContext;
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