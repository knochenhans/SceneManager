using System.Threading.Tasks;
using Godot;

public static class FadeHelper
{
	public enum FadeDirectionEnum
	{
		In,
		Out
	}

	public static async Task TweenFadeModulate(CanvasItem fadeNode, FadeDirectionEnum direction, float duration, float targetOpacity = 1.0f, string fadeProperty = "modulate", Tween.TransitionType transitionType = Tween.TransitionType.Linear, Tween.EaseType easeType = Tween.EaseType.InOut)
	{
		var originalColor = fadeNode.Modulate;

		Color target = direction == FadeDirectionEnum.In
			? new Color(originalColor.R, originalColor.G, originalColor.B, 0)
			: new Color(originalColor.R, originalColor.G, originalColor.B, targetOpacity);

		await RunTween(fadeNode, fadeNode.Modulate, target, duration, fadeProperty, transitionType, easeType);
	}

	public static async Task TweenFadeModulate(CanvasItem fadeNode, Color start, Color end, float duration, string fadeProperty = "modulate", Tween.TransitionType transitionType = Tween.TransitionType.Linear, Tween.EaseType easeType = Tween.EaseType.InOut)
	{
		await RunTween(fadeNode, start, end, duration, fadeProperty, transitionType, easeType);
	}

	private static async Task RunTween(CanvasItem fadeNode, Color start, Color end, float duration, string fadeProperty, Tween.TransitionType transitionType = Tween.TransitionType.Linear, Tween.EaseType easeType = Tween.EaseType.InOut)
	{
		fadeNode.Modulate = start;
		var tcs = new TaskCompletionSource();
		var tween = fadeNode.CreateTween();
		tween.TweenProperty(fadeNode, fadeProperty, end, duration)
			.SetTrans(transitionType)
			.SetEase(easeType);
		tween.Finished += () => tcs.SetResult();
		await tcs.Task;
	}
}
