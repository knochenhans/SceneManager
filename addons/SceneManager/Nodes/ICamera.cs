using Godot;
using System.Threading.Tasks;

public interface ICamera
{
    public void SetFollowTarget(Node target);
    public bool IsEnabled();
    public void SetEnabled(bool enabled);
    public void Enable(bool enable);
    public void Reset();
    public Vector2 GetZoom();
    public void SetZoom(Vector2 zoom);
    public Task ZoomTo(Vector2 zoom, float duration);
    public void SetLimits(Rect2 stageLimits);
    public Vector2 GetGlobalMousePosition();
}

public interface ICamera<TVector, TNode> : ICamera
    where TVector : struct
    where TNode : Node
{
    public void SetFollowTarget(TNode target);
    public void ClearFollowTarget();

    public NodePath GetPath();

    public TVector GetPosition();
    public void SetPosition(TVector position);

    public Task MoveTo(TVector position, float duration);
}