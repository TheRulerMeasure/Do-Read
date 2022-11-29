using Godot;
using System;

public class InfoResetData : Node
{
    [Signal]
    delegate void ResetData();

    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(ResetData), gg, "_OnResetData");
    }

    private void _OnTriggered(bool high)
    {
        EmitSignal(nameof(ResetData));
    }
}
