using System.Threading.Tasks;
using Godot;

public partial class Fade : ColorRect
{
	[Signal]
	public delegate void FadeFinishedEventHandler();

	public AnimationPlayer AnimationPlayerNode { get; set; }

	public override void _Ready() => AnimationPlayerNode = GetNode<AnimationPlayer>("AnimationPlayer");

	public async void FadeIn(float duration)
	{
		AnimationPlayerNode.Play("FadeIn");
		AnimationPlayerNode.Seek(0, true);
		AnimationPlayerNode.SpeedScale = 1 / duration;
		await ToSignal(AnimationPlayerNode, "animation_finished");
		EmitSignal(SignalName.FadeFinished);
		QueueFree();
	}

	public async void FadeOut(float duration)
	{
		AnimationPlayerNode.Play("FadeOut");
		AnimationPlayerNode.Seek(0, true);
		AnimationPlayerNode.SpeedScale = 1 / duration;
		await ToSignal(AnimationPlayerNode, "animation_finished");
		EmitSignal(SignalName.FadeFinished);
		QueueFree();
	}
}
