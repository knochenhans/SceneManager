using Godot;
using Godot.Collections;
using static Logger;

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
    FastNoiseLite noise = new();
    float noiseY = 0f;
    #endregion

    #region [Godot]
    public override void _Ready()
    {
        GD.Randomize();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        noise.Seed = (int)GD.Randi();
        noise.FractalOctaves = 4;
        noise.Frequency = 1.0f / 10.0f;
    }

    public override void _Process(double delta)
    {
        if (Trauma > 0f)
        {
            Shake();
            Trauma = Mathf.Clamp(Trauma - ((float)delta * ShakeDecay), 0f, 1f);
        }
    }
    #endregion

    #region [Lifecycle]
    private void Shake()
    {
        float amount = Mathf.Pow(Trauma, TraumaPower);
        noiseY++;
        // Rotation = ShakeMaxRoll * amount * noise.GetNoise2D(noise.Seed, noiseY);
        Offset = new Vector2(
            // ShakeMaxOffset.X * amount * noise.GetNoise2D((noise.Seed * 2) + 1, noiseY),
            // ShakeMaxOffset.Y * amount * noise.GetNoise2D((noise.Seed * 3) + 2, noiseY)
            ShakeMaxOffset.X * amount * GD.RandRange(-1, 1),
            ShakeMaxOffset.Y * amount * GD.RandRange(-1, 1)
        );
        GD.Print($"Camera Shake - Trauma: {Trauma}, Amount: {amount}, Rotation: {Rotation}, Offset: {Offset}");
    }
    #endregion

    #region [Public]
    public void AddTrauma(float amount)
    {
        Trauma = Mathf.Clamp(Trauma + amount, 0f, 1f);
    }
    #endregion
}
