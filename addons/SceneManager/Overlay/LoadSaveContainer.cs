using Godot;

public partial class LoadSaveContainer : VBoxContainer
{
    [Signal] public delegate void EntrySelectedEventHandler(string entryName);

    public override void _Ready()
    {
        using var dir = DirAccess.Open("user://");
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".save"))
                {
                    // skip init.save and autosave_quit.save
                    if (fileName == "init.save" || fileName == "autosave_quit.save")
                    {
                        fileName = dir.GetNext();
                        continue;
                    }

                    var saveName = fileName.Replace(".save", ""); // copy inside loop

                    var button = new Button
                    {
                        Text = saveName,
                        SizeFlagsHorizontal = SizeFlags.ExpandFill,
                        SizeFlagsVertical = SizeFlags.ExpandFill
                    };
                    AddChild(button);
                    button.Pressed += () => EmitSignal(SignalName.EntrySelected, saveName);
                }
                fileName = dir.GetNext();
            }
        }
        else
        {
            Logger.LogError("An error occurred when trying to access the path.", Logger.LogTypeEnum.Framework);
        }
    }
}