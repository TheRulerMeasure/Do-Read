using Godot;
using GDColl = Godot.Collections;
using System;

public class Orge : GroundCharacter
{
    private bool _hitAreaDisabled = true;

    [Export]
    public bool HitAreaDisabled {
        get => _hitAreaDisabled;
        set => _SetHitAreaDisabled(value);
    }
    [Export]
    private bool SaveToData = true;

    private GroundCharacter _groundTarget;

    // private int _bugCount = 0;

    private void _SetHitAreaDisabled(bool v)
    {
        _hitAreaDisabled = v;
        var l = GetNode<CollisionShape2D>("HitArea/LeftCol");
        var r = GetNode<CollisionShape2D>("HitArea/RightCol");
        if (_hitAreaDisabled)
        {
            l.Disabled = true;
            r.Disabled = true;
            return;
        }
        var spriteFlipH = GetNode<Sprite>("Sprite").FlipH;
        l.Disabled = !spriteFlipH;
        r.Disabled = spriteFlipH;
    }

    public override void _Ready()
    {
        base._Ready();

        Connect(nameof(StunChanged), this, nameof(_OnStunChanged));
        Connect(nameof(Damaged), this, nameof(_OnDamaged));
        Connect(nameof(Died), this, nameof(_OnDied));

        GetNode<Timer>("DeathTimer").Connect("timeout", this, nameof(_OnDeathTimerTimeout));

        var seekArea = GetNode<Area2D>("SeekArea");
        seekArea.Connect("body_entered", this, nameof(_OnSeekAreaBodyEntered));
        seekArea.Connect("body_exited", this, nameof(_OnSeekAreaBodyExited));
        var hitArea = GetNode<Area2D>("HitArea");
        hitArea.Connect("body_entered", this, nameof(_OnHitAreaBodyEntered));

        if (!SaveToData) return;
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.InitEnemy(this.GetPath());
        var arr = new GDColl.Array();
        arr.Add(this.GetPath());
        Connect(nameof(Died), gg, "_OnEnemyDied", arr);
    }

    public override void _Process(float delta)
    {
        AxisInput = _SeekTargetPos(_groundTarget);

        var animTree = GetNode<AnimationTree>("AnimationTree");
        if (AxisInput.IsEqualApprox(Vector2.Zero))
        {
            animTree.Set("parameters/idle_walk/current", 0);
            if (!GetNode<Timer>("AttackTimer").IsStopped())
            {
                animTree.Set("parameters/movement_attack/current", 1);
            }
        }
        else
        {
            animTree.Set("parameters/idle_walk/current", 1);
            animTree.Set("parameters/movement_attack/current", 0);
        }
    }

    private Vector2 _SeekTargetPos(GroundCharacter target)
    {
        if (!GetNode<Timer>("AttackTimer").IsStopped()) return Vector2.Zero;
        if (target == null)
        {
            return Vector2.Zero;;
        }
        Vector2 pos;
        var sprite = GetNode<Sprite>("Sprite");
        if (target.Position.x > Position.x)
        {
            pos = GetNode<CollisionShape2D>("HitArea/RightCol").GlobalPosition;
            sprite.FlipH = false;
        }
        else
        {
            pos = GetNode<CollisionShape2D>("HitArea/LeftCol").GlobalPosition;
            sprite.FlipH = true;
        }
        if (pos.DistanceSquaredTo(target.Position) <= 1600)
        {
            _Attacking();
            return Vector2.Zero;
        }
        return pos.DirectionTo(target.Position);
    }

    private void _Attacking()
    {
        var timer = GetNode<Timer>("AttackTimer");
        if (timer.IsStopped())
        {
            // AllowAxisInput = false;
            timer.Start(0.7f);
        }
    }

    private void _OnSeekAreaBodyEntered(Node body)
    {
        if (!body.IsInGroup("player")) return;
        _groundTarget = body as GroundCharacter;
    }

    private void _OnSeekAreaBodyExited(Node body)
    {
        if (!body.IsInGroup("player")) return;
        _groundTarget = null;
    }

    private void _OnHitAreaBodyEntered(Node body)
    {
        if (!body.IsInGroup("player")) return;
        var player = body as GroundCharacter;
        player.Stun();
        player.Push(Position.DirectionTo(player.Position), 300f);
        player.SetHealthLethal(player.Health - 1);
    }

    private void _OnStunChanged(bool stun)
    {
        var timer = GetNode<Timer>("AttackTimer");
        if (!timer.IsStopped() && stun) GetNode<Timer>("AttackTimer").Stop();

        var animTree = GetNode<AnimationTree>("AnimationTree");

        if (stun)
        {
            animTree.Set("parameters/idle_walk/current", 0);
        }
    }

    private void _OnDied()
    {
        var timer = GetNode<Timer>("DeathTimer");
        if (!timer.IsStopped()) return;
        SetPhysicsProcess(false);
        SetProcess(false);
        var animTree = GetNode<AnimationTree>("AnimationTree");
        animTree.Set("parameters/alive_dead/current", 1);

        timer.Start();
    }

    private void _OnDeathTimerTimeout()
    {
        QueueFree();
    }

    private void _OnDamaged(int dmg)
    {
        GetNode<AudioStreamPlayer2D>("HurtSound").Play();
    }
}
