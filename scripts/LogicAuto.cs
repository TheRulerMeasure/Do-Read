using Godot;
using GDColl = Godot.Collections;
using System;

public class LogicAuto : Node
{
    public enum Auto
    {
        Key0 = 1 << 0,
        Weapon = 1 << 1,
    }

    [Signal]
    delegate void Trigger(bool high);

    [Export]
    private GDColl.Array<NodePath> TargetPaths;
    [Export(
        PropertyHint.Flags,
        "Player Aqquired Key 0,"+
        "Player Aqquired Weapon"
    )]
    private int TriggerWhen = 0;

    public override void _Ready()
    {
        foreach (var i in TargetPaths)
        {
            Connect(nameof(Trigger), GetNode(i), "_OnTriggered");
        }
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.Connect("AddedKey", this, nameof(_OnGlobalKeyAdded));

        var keys = gg.GetKeys();
        foreach (var i in keys)
        {
            if ((TriggerWhen & (1 << (int) i)) == (1 << (int) i))
            {
                EmitSignal(nameof(Trigger), true);
                break;
            }
        }
    }

    private void _OnGlobalKeyAdded(int n)
    {
        if ((TriggerWhen & (1 << n)) == (1 << n))
        {
            EmitSignal(nameof(Trigger), true);
        }
    }
}
