using Godot;

public partial class MenuScene : CentralLayoutScene
{
    public static async void OnStartButtonPressed() => await SceneManager.Instance.ChangeToDefaultNextScene();
    public static void OnOptionsButtonPressed() => _ = SceneManager.Instance.ChangeToScene("options");
    public static void OnExitButtonPressed() => _ = SceneManager.Instance.ChangeToScene("credits");

    protected override void OnBackgroundInput(InputEvent @event)
    {
    }
}

