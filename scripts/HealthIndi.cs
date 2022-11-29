using Godot;
using System;

public class HealthIndi : Control
{
    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.InitHealthIndicator(this);
        gg.Connect("CriticalLevelChanged", this, nameof(_OnCriticalLevelChanged));
    }

    public void SetLevel(int level)
    {
        var lvl = Mathf.Clamp(level, 0, 3);
        var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animPlayer.Play("level" + lvl);
    }

    private void _OnCriticalLevelChanged(int level)
    {
        SetLevel(level);
    }
}
