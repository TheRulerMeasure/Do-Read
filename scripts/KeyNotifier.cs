using Godot;
using GDColl = Godot.Collections;
using System;

class KeyNotifier : Node
{
    [Signal]
    delegate void TriggerObject(bool high);

    [Export]
    public int KeyIndex = 0;
    [Export]
    GDColl.Array<NodePath> LinkedObjects;

    // private bool _triggered = false;

    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.InitKeyNoti(this.GetPath());
        gg.Connect("KeyAdded", this, nameof(_OnGlobalKeyAdded));

        if (LinkedObjects == null) return;
        foreach (var i in LinkedObjects)
        {
            var target = GetNode(i);
            if (!target.HasMethod("_OnTriggered"))
            {
                GD.Print("Dont have");
                continue;
            }
            Connect(nameof(TriggerObject), target, "_OnTriggered");
        }
    }

    public void GlobalHasKey()
    {
        if (LinkedObjects == null) return;
        foreach (var i in LinkedObjects)
        {
            var target = GetNode(i);
            if (!target.HasMethod("_OnTriggered"))
            {
                GD.Print("Dont have");
                continue;
            }
            // target.Call("_OnTriggered", true);
            target.CallDeferred("_OnTriggered", true);
            // Connect(nameof(TriggerObject), target, "_OnTriggered");
        }
    }

    private void _OnGlobalKeyAdded(int keyIndex)
    {
        // if (_triggered) return;
        if (keyIndex == this.KeyIndex)
        {
            // _triggered = true;
            EmitSignal(nameof(TriggerObject), true);
            // GD.Print("Triggered");
        }
    }
}
