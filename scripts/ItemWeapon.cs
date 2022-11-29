using Godot;
using System;

public class ItemWeapon : Item
{
    [Signal]
    delegate void AddedWeapon();

    public override void _Ready()
    {
        Connect(nameof(SearchChanged), this, nameof(_OnSearchChanged));
        base._Ready();

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(AddedWeapon), gg, "_OnAddedWeapon");
    }

    public override void UseItem()
    {
        EmitSignal(nameof(AddedWeapon));
        base.UseItem();
    }

    private void _OnSearchChanged(bool searched, NodePath itemPath)
    {
        if (searched) QueueFree();
    }
}
