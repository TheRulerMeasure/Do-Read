using Godot;
using System;

public class ItemKey : Item
{
    [Signal]
    delegate void AddedKey(int index);

    [Export]
    private int KeyIndex = 0;

    public override void _Ready()
    {
        Connect(nameof(SearchChanged), this, nameof(_OnSearchChanged));
        base._Ready();

        // if (Searched)
        // {
        //     QueueFree();
        //     return;
        // }

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(AddedKey), gg, "_OnAddedKey");
    }

    public override void UseItem()
    {
        EmitSignal(nameof(AddedKey), KeyIndex);
        base.UseItem();
    }

    private void _OnSearchChanged(bool searched, NodePath itemPath)
    {
        // GD.Print("key searched ", searched);
        if (searched) QueueFree();
    }
}
