using Godot;

public partial class HUD : CanvasLayer
{
    #region [Fields and Properties]
    protected Control HUDMarginContainer => GetNode<Control>("%HUDMarginContainer");
    #endregion

    #region [Lifecycle]
    public virtual void Init()
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