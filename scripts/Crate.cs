using Godot;
using GDColl = Godot.Collections;
using System;

public class Crate : GroundCharacter
{
    [Export]
    private bool SaveToData = true;

    public override void _Ready()
    {
        base._Ready();

        Connect(nameof(Damaged), this, nameof(_OnDamaged));
        Connect(nameof(Died), this, nameof(_OnDied));

        if (!SaveToData) return;
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.InitCrate(this.GetPath());
        var arr = new GDColl.Array();
        arr.Add(this.GetPath());
        Connect(nameof(Died), gg, "_OnCrateDied", arr);
    }

    private void _OnDamaged(int dmg)
    {
        GetNode<Sprite>("Sprite").Frame = 0;
    }

    private void _OnDied()
    {
        QueueFree();
    }
}
