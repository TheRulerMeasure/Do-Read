using Godot;
using System;

public class ItemNote : Item
{
    [Export(PropertyHint.MultilineText)]
    private string Text;
    // [Export]
    // private NodePath NoteUIPath;

    private PackedScene _paperNotePacked;

    public override void _Ready()
    {
        base._Ready();

        _paperNotePacked = ResourceLoader.Load<PackedScene>("res://scenes/objects/UI/PaperNote.tscn");
    }

    public override void UseItem()
    {
        _ShowNote();
        base.UseItem();
    }

    private void _ShowNote()
    {
        // if (NoteUIPath == null) return;
        if (Text == null) return;
        var noteUI = GetTree().CurrentScene.GetNode("UI/NoteUI");
        if (noteUI.GetChildCount() > 0) return;
        var paperNote = _paperNotePacked.Instance<PaperNote>();
        paperNote.Label = Text;
        noteUI.AddChild(paperNote);
    }
}
