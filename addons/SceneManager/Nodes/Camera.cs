using Godot;
using System.Threading.Tasks;

public partial class Camera : Camera2D, ICamera<Vector2, Node2D>
{
    #region [Fields and Properties]
    [ExportGroup("Limit Settings")]
    [Export] public bool LimitToStageLimits = false;
    [Export] public Vector2 StageLimitAddedMargin = new(16, 16);

    [ExportGroup("Shake Settings")]
    [Export] float ShakeDecay = 3f;
    [Export] Vector2 ShakeMaxOffset = new(10, 10);
    [Export] float ShakeMaxRoll = 0.1f;

    float Trauma = 0f;
    float TraumaPower = 2f;
    FastNoiseLite Noise = new();
    float noiseY = 0f;

    public Node2D FollowTarget;
    #endregion

    public void SetFollowTarget(Node2D target) => FollowTarget = target;
    public void ClearFollowTarget() => FollowTarget = null;

    public void SetLimits(Rect2 stageLimits)
    {
        if (!LimitToStageLimits)
            return;

        LimitLeft = (int)stageLimits.Position.X - (int)StageLimitAddedMargin.X;
        LimitTop = (int)stageLimits.Position.Y - (int)StageLimitAddedMargin.Y;
        LimitRight = (int)stageLimits.End.X + (int)StageLimitAddedMargin.X;
        LimitBottom = (int)stageLimits.End.Y + (int)StageLimitAddedMargin.Y;
    }

    public void Reset()
    {
        Position = Vector2.Zero;
        Zoom = Vector2.One;

        Logger.Log("Camera reset to default position and zoom", Logger.LogTypeEnum.World);
    }

    public async Task MoveTo(Vector2 position, float duration)
    {
        var tcs = new TaskCompletionSource();
        var tween = CreateTween();
        tween.TweenProperty(this, "position", position, duration);
        tween.Finished += tcs.SetResult;
        await tcs.Task;

        Logger.Log($"Camera moved to {position}", Logger.LogTypeEnum.World);
    }

    public async Task ZoomTo(Vector2 zoom, float duration)
    {
        var tcs = new TaskCompletionSource();
        var tween = CreateTween();
        tween.TweenProperty(this, "zoom", zoom, duration);
        tween.Finished += tcs.SetResult;
        await tcs.Task;

        Logger.Log($"Camera zoomed to {zoom}", Logger.LogTypeEnum.World);
    }

    public override void _Process(double delta)
    {
        if (FollowTarget != null)
            Position = Position.Lerp(FollowTarget.Position, 0.1f);
    }

    public void Enable(bool enable)
    {
        Enabled = enable;
        if (enable)
            MakeCurrent();
        SetProcess(enable);
        SetProcessInput(enable);
        SetPhysicsProcess(enable);
    }

    public void SetFollowTarget(Node target) => FollowTarget = target as Node2D;
}
