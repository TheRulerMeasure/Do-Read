using Godot;
using System;

public class ItemHealth : Item
{
    [Signal]
    delegate void UsedHealthItem(int healAmount);

    [Export]
    private int HealAmount = 2;

    public override void _Ready()
    {
        Connect(nameof(SearchChanged), this, nameof(_OnSearchChanged));
        base._Ready();

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(UsedHealthItem), gg, "_OnItemHealthUsed");
    }

    public override void UseItem()
    {
        EmitSignal(nameof(UsedHealthItem), HealAmount);
        base.UseItem();
    }

    private void _OnSearchChanged(bool searched, NodePath itemPath)
    {
        if (searched) QueueFree();
    }
}