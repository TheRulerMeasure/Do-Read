using Godot;
using System;

public class PaperNote : Control
{
    private string label;

    [Export(PropertyHint.MultilineText)]
    public string Label {
        get => label;
        set => _SetLabel(value);
    }

    public override void _Ready()
    {
        // var label = GetNode<Label>("NoteBody/PaperColor/LabelMargin/Label");
        // label.RectPosition = new Vector2(268, 632);
        ShowPaper();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("move_left") ||
            Input.IsActionJustPressed("move_right") || 
            Input.IsActionJustPressed("move_up") ||
            Input.IsActionJustPressed("move_down") ||
            Input.IsActionJustPressed("interact_item"))
        {
            // GD.Print("note Freed");
            this.QueueFree();
        }
    }

    public void ShowPaper()
    {
        GetNode<AnimationPlayer>("AnimationPlayer").Play("show_paper");
        // GetNode<ColorRect>("NoteBody/PaperColor").Modulate = new Color(Colors.White, 1);
    }

    private void _SetLabel(string v)
    {
        this.label = v;
        var label = GetNode<Label>("NoteBody/PaperColor/LabelMargin/Label");
        label.Text = this.label;
    }
}
