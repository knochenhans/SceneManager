using Godot;

[Tool, GlobalClass]
public partial class RichTextFadeIn : RichTextEffect
{
    public string bbcode = "fade_in";

    public override bool _ProcessCustomFX(CharFXTransform charFx)
    {
        float length = charFx.Env.TryGetValue("length", out var lenVal) ? lenVal.As<float>() : 5.0f;
        float speed = charFx.Env.TryGetValue("speed", out var spdVal) ? spdVal.As<float>() : 10.0f;
        float currentPos = (float)(charFx.ElapsedTime * speed);
        float charPos = charFx.Range.X;
        float alpha = Mathf.Clamp((currentPos - charPos) / Mathf.Max(length, 0.001f), 0.0f, 1.0f);

        Color color = charFx.Color;
        color.A = alpha;
        charFx.Color = color;

        return true;
    }
}