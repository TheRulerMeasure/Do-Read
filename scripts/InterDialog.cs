using Godot;
using GDColl = Godot.Collections;
using System;

public class InterDialog : Item
{
    [Signal]
    delegate void Interacted(string dialogFile, NodePath path);
    [Signal]
    delegate void Trigger(bool high);

    [Export]
    private GDColl.Array<NodePath> LinkedObjects;

    [Export(PropertyHint.File)]
    private string DialogFile;

    public override void _Ready()
    {
        base._Ready();

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(Interacted), gg, "_OnInterDialogInteracted");

        if (LinkedObjects == null) return;
        foreach (var path in LinkedObjects)
        {
            if (!GetNode(path).HasMethod("_OnTriggered")) continue;
            Connect(nameof(Trigger), GetNode(path), "_OnTriggered");
        }
    }

    public override void UseItem()
    {
        EmitSignal(nameof(Interacted), DialogFile, GetPath());
        base.UseItem();
    }

    private void _OnGGDialogFinished()
    {
        GD.Print("Done and emiit");
        EmitSignal(nameof(Trigger), true);
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.Disconnect("DialogFinished", this, nameof(_OnGGDialogFinished));
    }
}
