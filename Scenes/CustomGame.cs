public partial class CustomGame : BaseGame
{
    public void OnButtonPressed() => SceneManager.Instance.ChangeToScene("Menu");
}
