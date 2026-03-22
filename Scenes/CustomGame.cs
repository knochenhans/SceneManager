public partial class CustomGame : BaseGame
{
    public void OnButtonPressed() => SceneManager.Instance.ChangeToScene("Menu");

    public override void InitGame(bool loadGame = false)
    {
        base.InitGame(loadGame);

        WidgetManager.OpenWidget("test", "Test Widget Title");
    }
}
