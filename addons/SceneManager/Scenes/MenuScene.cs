using Godot;

public partial class MenuScene : CentralLayoutScene
{
    public static async void OnStartButtonPressed() => await SceneManager.Instance.ChangeToDefaultNextScene();
    public static void OnOptionsButtonPressed() => SceneManager.Instance.ChangeToScene("options");
    public static void OnExitButtonPressed() => SceneManager.Instance.ChangeToScene("credits");

    protected override void OnBackgroundInput(InputEvent @event)
    {
    }
}

