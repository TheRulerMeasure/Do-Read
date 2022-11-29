using Godot;
using System;

public class GroundCharacter : GroundKinematicBody
{
    [Signal]
    public delegate void Died();
    [Signal]
    public delegate void Damaged(int dmg);
    [Signal]
    public delegate void StunChanged(bool stun);

    private int _health = 10;

    [Export]
    public int MaxHealth = 10;
    [Export]
    public int Health {
        get => _health;
        set => _SetHealth(value);
    }

    public override void _Ready()
    {
        base._Ready();
        // Connect(nameof(Pushed), this, nameof(_OnPushed));

        var timer = new Timer();
        AddChild(timer);
        timer.Name = "StunTimer";
        timer.OneShot = true;
        timer.Connect("timeout", this, nameof(_OnStunTimerTimeout));
    }

    public void Stun(float stunDuration=0.2f, bool stack=true)
    {
        // GD.Print("Stunned");
        var timer = GetNode<Timer>("StunTimer");
        if (stack)
        {
            if (timer.IsStopped())
            {
                timer.Start(stunDuration);
                EmitSignal(nameof(StunChanged), true);
                return;
            }
            timer.Start(timer.TimeLeft + stunDuration);
            EmitSignal(nameof(StunChanged), true);
            return;
        }
        timer.Start(stunDuration);
        AllowAxisInput = false;
        EmitSignal(nameof(StunChanged), true);
    }

    public void SetHealthLethal(int health)
    {
        var oldHealth = Health;
        Health = health;
        EmitSignal(nameof(Damaged), oldHealth - health);
        if (Health <= 0)
        {
            if (oldHealth > 0) EmitSignal(nameof(Died));
        }
    }

    // private void _OnPushed(Vector2 dir, float force)
    // {
    //     Stun();
    // }

    private void _SetHealth(int health)
    {
        _health = health;
        if (_health > MaxHealth)
        {
            _health = MaxHealth;
        }
    }

    private void _OnStunTimerTimeout()
    {
        if (Health <= 0) return;
        AllowAxisInput = true;
        EmitSignal(nameof(StunChanged), false);
    }
}
