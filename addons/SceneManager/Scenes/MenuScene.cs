using Godot;

public partial class MenuScene : Scene
{
    public async void OnStartButtonPressed() => await SceneManager.Instance.ChangeToDefaultNextScene();
    public void OnOptionsButtonPressed() => SceneManager.Instance.ChangeToScene("options");
    public void OnExitButtonPressed() => SceneManager.Instance.ChangeToScene("credits");

    protected override void OnBackgroundClicked(InputEvent @event)
    {
    }
}

