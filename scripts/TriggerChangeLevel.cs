using Godot;
using System;

public class TriggerChangeLevel : Area2D
{
    [Signal]
    delegate void EnteredChangeLevel(string newLevel, string exitPathName);

    [Export(PropertyHint.File)]
    private string NewLevel;
    [Export]
    private string exitPathName;

    public override void _Ready()
    {
        Connect("body_entered", this, nameof(_OnBodyEntered));

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(EnteredChangeLevel), gg, "_OnEnteredChangeLevel");
    }

    private void _OnBodyEntered(Node body)
    {
        if (!body.IsInGroup("player")) return;
        if (NewLevel == null) return;
        if (exitPathName == null) return;
        EmitSignal(nameof(EnteredChangeLevel), NewLevel, exitPathName);
    }
}
