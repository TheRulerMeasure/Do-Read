using Godot;
using GDColl = Godot.Collections;
using System;

public class TriggerChangeLevel : Area2D
{
    [Signal]
    delegate void EnteredChangeLevel(string newLevel, string exitPathName);
    [Signal]
    delegate void Trigger(bool high);

    [Export(PropertyHint.File)]
    private string NewLevel;
    [Export]
    private string exitPathName;
    [Export]
    public GDColl.Array<NodePath> TargetPaths;

    public override void _Ready()
    {
        Connect("body_entered", this, nameof(_OnBodyEntered));

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(EnteredChangeLevel), gg, "_OnEnteredChangeLevel");

        if (TargetPaths == null) return;
        foreach (var item in TargetPaths)
        {
            // if (!GetNode(item).HasNode("_OnTriggered")) continue;
            Connect(nameof(Trigger), GetNode(item), "_OnTriggered");
        }
    }

    private void _OnBodyEntered(Node body)
    {
        if (!body.IsInGroup("player")) return;
        if (NewLevel == null) return;
        if (exitPathName == null) return;
        EmitSignal(nameof(EnteredChangeLevel), NewLevel, exitPathName);
        EmitSignal(nameof(Trigger), true);
    }
}
