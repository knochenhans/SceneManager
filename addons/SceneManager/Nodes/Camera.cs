using Godot;
using System.Threading.Tasks;

public partial class Camera : Camera2D, ICamera<Vector2, Node2D>
{
    #region [Fields and Properties]
    [ExportGroup("Limit Settings")]
    [Export] public bool LimitToStageLimits = true; // Enabled by default
    [Export] public Vector2 StageLimitAddedMargin = new(16, 16);

    private Rect2 currentStageLimits;
    private bool hasLimits = false;

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

    public override void _Ready() => AnchorMode = AnchorModeEnum.DragCenter;

    public void SetFollowTarget(Node2D target) => FollowTarget = target;
    public void ClearFollowTarget() => FollowTarget = null;
    public void SetFollowTarget(Node target) => FollowTarget = target as Node2D;

    public void SetLimits(Rect2 stageLimits)
    {
        currentStageLimits = stageLimits;
        hasLimits = true;

        if (LimitToStageLimits)
            ClampPositionToLimits();
    }

    public void Reset()
    {
        Position = Vector2.Zero;
        Zoom = Vector2.One;

        Logger.Log("Camera reset to default position and zoom", Logger.LogTypeEnum.World);
    }

    public async Task MoveTo(Vector2 position, float duration, Tween.TransitionType transitionType = Tween.TransitionType.Linear, Tween.EaseType easeType = Tween.EaseType.InOut)
    {
        Vector2 targetPos = (hasLimits && LimitToStageLimits) ? GetClampedPosition(position) : position;

        var tcs = new TaskCompletionSource();
        var tween = CreateTween();
        tween.SetEase(easeType);
        tween.SetTrans(transitionType);

        tween.TweenProperty(this, "position", targetPos, duration);

        if (hasLimits && LimitToStageLimits)
        {
            tween.TweenCallback(Callable.From(() => Position = GetClampedPosition(Position)));
        }

        tween.Finished += tcs.SetResult;
        await tcs.Task;

        Logger.Log($"Camera moved to {Position}", Logger.LogTypeEnum.World);
    }

    public async Task ZoomTo(Vector2 zoom, float duration, Tween.TransitionType transitionType = Tween.TransitionType.Linear, Tween.EaseType easeType = Tween.EaseType.InOut)
    {
        var tcs = new TaskCompletionSource();
        var tween = CreateTween();
        tween.SetEase(easeType);
        tween.SetTrans(transitionType);
        tween.TweenProperty(this, "zoom", zoom, duration);
        tween.Finished += tcs.SetResult;
        await tcs.Task;

        Logger.Log($"Camera zoomed to {zoom}", Logger.LogTypeEnum.World);
    }

    public override void _Process(double delta)
    {
        if (FollowTarget != null && IsInstanceValid(FollowTarget))
            Position = Position.Lerp(FollowTarget.Position, 0.1f);

        if (hasLimits && LimitToStageLimits)
            ClampPositionToLimits();
    }

    public Vector2 GetClampedPosition(Vector2 targetPosition)
    {
        if (!hasLimits || !LimitToStageLimits)
            return targetPosition;

        Vector2 viewportSizeWorld = GetViewportRect().Size / Zoom;
        Vector2 halfViewSize = viewportSizeWorld / 2.0f;

        float minX = currentStageLimits.Position.X - StageLimitAddedMargin.X + halfViewSize.X;
        float maxX = currentStageLimits.End.X + StageLimitAddedMargin.X - halfViewSize.X;
        float minY = currentStageLimits.Position.Y - StageLimitAddedMargin.Y + halfViewSize.Y;
        float maxY = currentStageLimits.End.Y + StageLimitAddedMargin.Y - halfViewSize.Y;

        Vector2 clampedPos = targetPosition;

        if (minX > maxX)
            clampedPos.X = currentStageLimits.Position.X + (currentStageLimits.Size.X / 2.0f);
        else
            clampedPos.X = Mathf.Clamp(targetPosition.X, minX, maxX);

        if (minY > maxY)
            clampedPos.Y = currentStageLimits.Position.Y + (currentStageLimits.Size.Y / 2.0f);
        else
            clampedPos.Y = Mathf.Clamp(targetPosition.Y, minY, maxY);

        return clampedPos;
    }

    private void ClampPositionToLimits() => Position = GetClampedPosition(Position);

    public void Enable(bool enable)
    {
        Enabled = enable;
        if (enable)
            MakeCurrent();
        SetProcess(enable);
        SetProcessInput(enable);
        SetPhysicsProcess(enable);
    }
}