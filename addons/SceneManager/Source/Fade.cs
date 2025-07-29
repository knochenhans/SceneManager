using Godot;
using static Logger;

public partial class Fade : ColorRect
{
	[Signal] public delegate void FadeFinishedEventHandler();

	public enum FadeDirectionEnum
	{
		In,
		Out
	}

	public AnimationPlayer AnimationPlayerNode { get; set; }

	public override void _Ready() => AnimationPlayerNode = GetNode<AnimationPlayer>("AnimationPlayer");

	public async void Run(FadeDirectionEnum direction, float time)
	{
		GD.Print($"Fade {direction} in {time} seconds");
		string animationName = direction == FadeDirectionEnum.In ? "FadeIn" : "FadeOut";
		AnimationPlayerNode.Play(animationName);
		AnimationPlayerNode.Seek(0, true);
		AnimationPlayerNode.SpeedScale = 1 / time;
		await ToSignal(AnimationPlayerNode, AnimationPlayer.SignalName.AnimationFinished);
		EmitSignal(SignalName.FadeFinished);
		QueueFree();
	}
}
