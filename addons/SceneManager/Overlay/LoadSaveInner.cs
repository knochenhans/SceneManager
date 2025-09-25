using Godot;
using MenuEntryData = Godot.Collections.Dictionary<string, Godot.Variant>;

public partial class LoadSaveInner : OverlayInner
{
    [Export] public string EntryString = string.Empty;

    LoadSaveContainer LoadSaveContainerNode => GetNodeOrNull<LoadSaveContainer>("%LoadSaveContainer");

    public override void _Ready()
    {
        base._Ready();

        if (LoadSaveContainerNode != null)
            LoadSaveContainerNode.EntrySelected += (entryName) => EmitSignal(OverlayInner.SignalName.EntrySelected, new MenuEntryData { { EntryString, entryName } });
    }
}