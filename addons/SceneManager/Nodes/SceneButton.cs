using Godot;

public partial class SceneButton : Button
{
    public override void _Ready()
    {
        Pressed += () => UISoundPlayer.Instance.PlaySound("click1");
        MouseEntered += () => UISoundPlayer.Instance.PlaySound("hover");
    }
}
