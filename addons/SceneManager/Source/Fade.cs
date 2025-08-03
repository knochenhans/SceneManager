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

	public AnimationPlayer AnimationPlayerNode => GetNode<AnimationPlayer>("AnimationPlayer");

	public async void Run(FadeDirectionEnum direction, float time)
	{
		Log($"Fade {direction} for {time} seconds", "SceneManager", LogTypeEnum.Framework);
		string animationName = direction == FadeDirectionEnum.In ? "FadeIn" : "FadeOut";
		AnimationPlayerNode.SpeedScale = 1 / time;
		AnimationPlayerNode.Play(animationName);
		AnimationPlayerNode.Seek(0, true);
		await ToSignal(AnimationPlayerNode, AnimationMixer.SignalName.AnimationFinished);
		EmitSignal(SignalName.FadeFinished);
	}
}
