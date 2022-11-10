using Godot;
using GDColl = Godot.Collections;
using System;

public class TriggerMultiple : Area2D
{
    [Signal]
    delegate void Trigger(bool high);

    [Export]
    private bool Disabled = false;
    [Export]
    public GDColl.Array<NodePath> TargetPaths;

    public override void _Ready()
    {
        Connect("body_entered", this, nameof(_OnBodyEntered));
        Connect("body_exited", this, nameof(_OnBodyExited));
        foreach (var item in TargetPaths)
        {
            Connect(nameof(Trigger), GetNode(item), "_OnTriggered");
        }
    }

    private void _OnTriggered(bool high)
    {
        // GD.Print("trigger is triggered");
        Disabled = false;
    }

    private void _OnBodyEntered(Node body)
    {
        if (Disabled) return;
        if (body.IsInGroup("player"))
        {
            EmitSignal(nameof(Trigger), true);
        }
    }

    private void _OnBodyExited(Node body)
    {
        if (Disabled) return;
        if (body.IsInGroup("player"))
        {
            EmitSignal(nameof(Trigger), false);
        }
    }
}
