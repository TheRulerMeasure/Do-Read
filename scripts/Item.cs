using Godot;
using System;

public class Item : Area2D
{
    [Signal]
    public delegate void SearchChanged(bool searched, NodePath path);

    private bool _searched = false;

    [Export]
    public bool Searched
    {
        get => _searched;
        set => _SetSearched(value);
    }
    // [Export]
    // private bool RemoveOnSearched = false;
    [Export]
    private bool SaveToData = true;

    public override void _Ready()
    {
        if (!SaveToData) return;
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(SearchChanged), gg, "_OnSearchChanged");
        gg.InitItem(this.GetPath());
    }

    public virtual void UseItem()
    {
        Searched = true;
        // GD.Print("Item Used");
    }

    private void _SetSearched(bool v)
    {
        _searched = v;
        EmitSignal(nameof(SearchChanged), _searched, this.GetPath());
        // if (!RemoveOnSearched) return;
        // if (_searched) QueueFree();
    }
}
