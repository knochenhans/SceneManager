using System.Threading.Tasks;
using Godot;

public static class FadeManager
{
	public enum FadeDirectionEnum
	{
		In,
		Out
	}

	public static async Task TweenFadeModulate(ColorRect fadeNode, FadeDirectionEnum direction, float duration, float targetOpacity = 1.0f, string fadeProperty = "modulate")
	{
		Color target = direction == FadeDirectionEnum.In
			? new Color(1, 1, 1, 0)
			: new Color(1, 1, 1, targetOpacity);

		var tcs = new TaskCompletionSource();
		var tween = fadeNode.CreateTween();
		tween.TweenProperty(fadeNode, fadeProperty, target, duration)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.InOut);
		tween.Finished += () => tcs.SetResult();
		await tcs.Task;
	}
}
