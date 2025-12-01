using Godot;
using Godot.Collections;

public class CursorManager
{
    readonly Dictionary<string, CursorSetResource> cursorSets;
    readonly System.Collections.Generic.Stack<CursorSetResource> cursorStack = new();

    private readonly int scaleFactor = 1;

    public CursorManager(Dictionary<string, CursorSetResource> cursorSets, int scaleFactor = 1)
    {
        this.cursorSets = cursorSets ?? [];
        this.scaleFactor = scaleFactor;

        if (this.scaleFactor > 1)
            ScaleCursors(cursorSets, this.scaleFactor);

        SetMouseCursor();
    }

    public void Uninit()
    {
        Input.SetCustomMouseCursor(null);
        if (scaleFactor > 1)
            ScaleCursors(cursorSets, 1);
    }

    private static void ScaleCursors(Dictionary<string, CursorSetResource> cursorSets, int scaleFactor)
    {
        foreach (var cursorSet in cursorSets.Values)
        {
            if (cursorSet == null)
                continue;

            var texture = cursorSet.Texture;
            var image = texture.GetImage();

            if (image is Image img)
            {
                if (img.Duplicate() is Image scaledImage)
                {
                    scaledImage.Resize(scaledImage.GetWidth() * scaleFactor, scaledImage.GetHeight() * scaleFactor, Image.Interpolation.Nearest);
                    cursorSet.Texture = ImageTexture.CreateFromImage(scaledImage);
                    cursorSet.Hotspot *= scaleFactor;
                }
            }
        }
    }

    public void SetMouseCursor(string cursorSetKey = "default")
    {
        if (cursorSets.TryGetValue(cursorSetKey, out CursorSetResource cursorSet))
        {
            Input.SetCustomMouseCursor(cursorSet.Texture, hotspot: cursorSet.Hotspot);
            cursorStack.Push(cursorSet);
        }
    }

    public void ResetMouseCursor()
    {
        SetMouseCursor("default");
    }
}
