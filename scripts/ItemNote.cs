using Godot;
using System;

public class ItemNote : Area2D
{
    [Signal]
    delegate void Searched(bool searched, NodePath nodePath);
    [Signal]
    delegate void Init(NodePath nodePath);

    private bool _searched = false;

    [Export(PropertyHint.MultilineText)]
    private string Text;
    [Export]
    private PackedScene PaperNotePacked;
    [Export]
    private NodePath GameUIPath;

    [Export]
    public bool IsSearched {
        get => _searched;
        set => _SetSearched(value);
    }

    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(Searched), gg, "_OnNoteSearched");
        Connect(nameof(Init), gg, "_OnItemNoteInit");
        EmitSignal(nameof(Init), this.GetPath());
    }

    private void _SetSearched(bool searched)
    {
        _searched = searched;
        EmitSignal(nameof(Searched), _searched, this.GetPath());
    }

    private void _OnUsed()
    {
        _RequestSpawnNote();
    }

    private void _RequestSpawnNote()
    {
        if (GetNode(GameUIPath).GetChildCount() > 0) return;
        _OnAllowSpawningNote();
        IsSearched = true;
    }

    private void _OnAllowSpawningNote()
    {
        var paperNote = PaperNotePacked.Instance<PaperNote>();
        paperNote.Label = Text;
        paperNote.GetNode<Control>("NoteBody").RectPosition = new Vector2(200, 600);
        GetNode(GameUIPath).AddChild(paperNote);
    }
}
