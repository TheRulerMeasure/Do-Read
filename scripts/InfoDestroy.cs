using Godot;
using GDColl = Godot.Collections;
using System;

public class InfoDestroy : Node
{
    [Signal]
    delegate void DestroyTriggered(NodePath path);

    [Export]
    private GDColl.Array<NodePath> ObjectsToBeDestroyed;
    [Export]
    private bool SaveToData = true;

    private bool _triggered = false;

    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.InitInfoDestroy(GetPath());
        Connect(nameof(DestroyTriggered), gg, "_OnInfoDestroyTriggered");
    }

    public void DestroyObjects()
    {
        _triggered = true;
        if (ObjectsToBeDestroyed == null) return;
        foreach (var path in ObjectsToBeDestroyed)
        {
            GetNode(path).QueueFree();
        }
    }

    private void _OnTriggered(bool high)
    {
        if (!_triggered)
        {
            DestroyObjects();
            EmitSignal(nameof(DestroyTriggered), GetPath());
            _triggered = true;
        }
    }
}
