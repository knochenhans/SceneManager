public partial class Game : Scene
{
	public void OnButtonPressed() => SceneManager.Instance.ChangeToScene("Menu");
}
