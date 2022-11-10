using Godot;
using GDColl = Godot.Collections;
using System;

public class ItemKey : Area2D
{
    [Signal]
    delegate void Trigger(bool high);
    [Signal]
    delegate void Searched(bool searched, NodePath nodePath);
    [Signal]
    delegate void Init(NodePath nodePath);
    [Signal]
    delegate void AddKey(int keyIndex);

    private bool _searched = false;

    [Export]
    private GDColl.Array<NodePath> TargetPaths;
    [Export]
    public bool IsSearched {
        get => _searched;
        set => _SetSearched(value);
    }
    [Export]
    private int KeyIndex = 0;

    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(AddKey), gg, "_OnAddKey");
        Connect(nameof(Searched), gg, "_OnNoteSearched");
        Connect(nameof(Init), gg, "_OnItemNoteInit");
        EmitSignal(nameof(Init), this.GetPath());

        if (TargetPaths == null) return;
        foreach (var item in TargetPaths)
        {
            Connect(nameof(Trigger), GetNode(item), "_OnTriggered");
        }
    }

    private void _SetSearched(bool searched)
    {
        _searched = searched;
        EmitSignal(nameof(Searched), _searched, this.GetPath());
        if (_searched) QueueFree();
    }

    private void _OnUsed()
    {
        if (IsSearched) return;
        EmitSignal(nameof(AddKey), KeyIndex);
        IsSearched = true;
        EmitSignal(nameof(Trigger), true);
    }
}
