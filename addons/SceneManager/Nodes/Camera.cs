using Godot;

public partial class Camera : Camera2D
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
    #endregion
}
