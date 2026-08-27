using Godot;

using System.Threading.Tasks;

[Tool]
public partial class CustomWindow : Control
{
    [Signal] public delegate void CloseRequestedEventHandler();

    [Export] public CustomWindowTitlePanel TitlePanel;
    [Export] public bool Modal;

    private string title = "Custom Window";

    [Export]
    public string Title
    {
        get => title;
        set
        {
            title = value;
            if (IsInstanceValid(TitlePanel))
                TitlePanel.SetTitle(title);
        }
    }

    public override void _Ready()
    {
        if (IsInstanceValid(TitlePanel))
        {
            TitlePanel.Init(this);
            TitlePanel.CloseButtonPressed += OnCloseButtonPressed;
            TitlePanel.SetTitle(Title);
        }
    }

    protected void OnCloseButtonPressed() => EmitSignal(SignalName.CloseRequested);

    public void SetTitle(string newTitle) => Title = newTitle;

    public virtual async Task OpenAsync(Variant? data = null)
    {
        Visible = true;
        await Task.CompletedTask;
    }

    public virtual async Task CloseAsync()
    {
        Visible = false;
        await Task.CompletedTask;
    }

    public void Toggle() => Visible = !Visible;
}