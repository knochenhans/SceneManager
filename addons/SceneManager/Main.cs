using CoreSystems;

using Godot;

public partial class Main : Node
{
    [Export] public SceneManager SceneManager;
    [Export] public UISoundManager UISoundManager;

    protected GameContext GameContext;

    public class GameFlags
    {
        public const string SkipToGame = "skip-to-game";
        public const string Invincible = "invincible";
        public const string NoCollision = "no-collision";
        public const string FreeCam = "free-cam";
        public const string Pause = "pause";
        public const string DisableMusic = "disable-music";
        public const string DisableLights = "disable-lights";
    }

    #region [Godot]
    public override void _EnterTree()
    {
        base._EnterTree();

        GameContext = CreateGameContext();

        Logger.WriteToFile = true;

        DebugConfig.RegisterFlags(
        [
            GameFlags.SkipToGame,
            GameFlags.Invincible,
            GameFlags.NoCollision,
            GameFlags.FreeCam,
            GameFlags.Pause,
            GameFlags.DisableMusic,
            GameFlags.DisableLights
        ]);

        DebugConfig.RegisterProfile("Default", [GameFlags.SkipToGame]);
        DebugConfig.RegisterProfile("NoClip", [GameFlags.SkipToGame, GameFlags.Invincible, GameFlags.NoCollision]);
        DebugConfig.RegisterProfile("Full", [GameFlags.SkipToGame, GameFlags.Invincible, GameFlags.NoCollision, GameFlags.FreeCam, GameFlags.DisableMusic]);

        DebugConfig.InitializeFromCommandLine();

        if (DebugConfig.IsFlagActive(GameFlags.SkipToGame))
            SceneManager.SceneManagerResource.initialSceneName = "game";

        SceneManager.Init(GameContext);

        GameContext.UISoundManager = UISoundManager;
    }

    protected virtual GameContext CreateGameContext() => new();
    #endregion
}
