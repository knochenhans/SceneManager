public partial class Menu : Scene
{
    public void OnStartButtonPressed() => SceneManager.ChangeToScene("Game");
    public void OnOptionsButtonPressed() => SceneManager.ChangeToScene("Options");
    public void OnExitButtonPressed() => SceneManager.ChangeToScene("Credits");
}
