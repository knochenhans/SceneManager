public partial class Menu : Scene
{
    public void OnStartButtonPressed() => SceneManager.Instance.ChangeToScene("Game");
    public void OnOptionsButtonPressed() => SceneManager.Instance.ChangeToScene("Options");
    public void OnExitButtonPressed() => SceneManager.Instance.ChangeToScene("Credits");
}
