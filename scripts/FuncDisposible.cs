using Godot;
using System;

public class FuncDisposible : Node
{
    [Signal]
    delegate void Disposed(NodePath path);

    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(Disposed), gg, "_OnNodeDisposed");
        gg.InitDisposible(this.GetPath());
    }

    private void _OnTrigger(bool on)
    {
        EmitSignal(nameof(Dispose), GetPath());
        QueueFree();
    }
}
