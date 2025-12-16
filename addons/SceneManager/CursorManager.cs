using Godot;
using Godot.Collections;

public class CursorManager
{
    readonly Dictionary<string, CursorSetResource> cursorSets;
    readonly System.Collections.Generic.Stack<CursorSetResource> cursorStack = new();

    private readonly float scaleFactor = 1;

    public CursorManager(Dictionary<string, CursorSetResource> cursorSets, float scaleFactor = 1)
    {
        this.cursorSets = cursorSets ?? [];
        this.scaleFactor = scaleFactor;

        if (this.scaleFactor > 1)
            ScaleCursorsBy(cursorSets, this.scaleFactor);

        SetMouseCursor();
    }

    public void Uninit()
    {
        Input.SetCustomMouseCursor(null);
        if (scaleFactor > 1)
            ScaleCursorsBy(cursorSets, 1 / scaleFactor);
    }

    private static void ScaleCursorsBy(Dictionary<string, CursorSetResource> cursorSets, float scaleFactor)
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
                    scaledImage.Resize((int)(scaledImage.GetWidth() * scaleFactor), (int)(scaledImage.GetHeight() * scaleFactor), Image.Interpolation.Nearest);
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

    public void RestorePreviousMouseCursor()
    {
        if (cursorStack.Count > 1)
        {
            cursorStack.Pop();
            var previousCursorSet = cursorStack.Peek();
            Input.SetCustomMouseCursor(previousCursorSet.Texture, hotspot: previousCursorSet.Hotspot);
        }
        else
        {
            ResetMouseCursor();
        }
    }

    public void ResetMouseCursor()
    {
        SetMouseCursor("default");
    }
}
