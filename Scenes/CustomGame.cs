public partial class CustomGame : Scene
{
    public void OnButtonPressed() => SceneManager.Instance.ChangeToScene("Menu");
}
