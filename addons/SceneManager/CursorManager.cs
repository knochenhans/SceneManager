using Godot;
using Godot.Collections;

public class CursorManager
{
    readonly Dictionary<string, CursorSetResource> cursorSets;
    readonly System.Collections.Generic.Stack<CursorSetResource> cursorStack = new();

    public CursorManager(Dictionary<string, CursorSetResource> cursorSets, int scaleFactor = 1)
    {
        this.cursorSets = cursorSets ?? [];

		if (scaleFactor > 1)
			ScaleCursors(cursorSets, scaleFactor);

        SetMouseCursor();
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
                    var scaledTexture = ImageTexture.CreateFromImage(scaledImage);
					cursorSet.Texture = scaledTexture;
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
		// if (cursorStack.Count > 0)
		// 	cursorStack.Pop();

		// if (cursorStack.Count > 0)
		// {
		// 	var last = cursorStack.Peek();
		// 	Input.SetCustomMouseCursor(last.Texture, hotspot: last.Hotspot);
		// }
		// else
		// {
		// 	Input.SetCustomMouseCursor(null);
		// }
        SetMouseCursor("default");
	}
}
