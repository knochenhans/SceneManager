using CoreSystems;
using Godot;

public partial class HUD : CanvasLayer
{
    #region [Fields and Properties]
    protected Control HUDMarginContainer => GetNode<Control>("%HUDMarginContainer");
    protected GameContext GameContext;
    protected Node2D Player;
    protected StatDefinitionDatabase StatDefinitionDatabase;
    #endregion

    #region [Lifecycle]
    public virtual void Init(GameContext gameContext, Node2D player = null, StatDefinitionDatabase statDefinitionDatabase = null)
    {
        GameContext = gameContext;
        Player = player;
        StatDefinitionDatabase = statDefinitionDatabase;
    }

    public virtual void Uninit()
    {
    }
    #endregion
}