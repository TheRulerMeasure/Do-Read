using Godot;
using System;

public class Play : GroundCharacter
{
    [Signal]
    delegate void FoundItem(Area2D item);
    [Signal]
    delegate void AwayItem(Area2D item);
    [Signal]
    delegate void UseItem();
    [Signal]
    delegate void CycleItem();
    [Signal]
    delegate void HitBody(Node body);

    private bool _hitAreaDisabled = true;

    [Export]
    public bool HasWeapon = false;
    [Export]
    public bool HitAreaDisabled {
        get => _hitAreaDisabled;
        set => _SetHitAreaDisabled(value);
    }

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
        l.Disabled = spriteFlipH;
        r.Disabled = !spriteFlipH;
    }

    public override void _Ready()
    {
        base._Ready();

        Connect(nameof(Damaged), this, nameof(_OnDamaged));
        Connect(nameof(StunChanged), this, nameof(_OnStunChanged));
        Connect(nameof(Died), this, nameof(_OnDied));

        var cam2D = GetNode<Camera2D>("Camera2D");
        Connect(nameof(HitBody), cam2D, "_OnPlayerHitBody");
        Connect(nameof(Damaged), cam2D, "_OnPlayerHurt");

        GetNode<Timer>("AttackTimer").Connect("timeout", this, nameof(_OnAttackTimerTimeout));

        GetNode<Area2D>("Area2D").Connect("area_entered", this, nameof(_OnAreaEntered));
        GetNode<Area2D>("Area2D").Connect("area_exited", this, nameof(_OnAreaExited));

        GetNode<Area2D>("HitArea").Connect("body_entered", this, nameof(_OnHitAreaBodyEntered));

        var itemBag = GetNode<ItemBag>("ItemBag");
        Connect(nameof(FoundItem), itemBag, "_OnFoundItem");
        Connect(nameof(AwayItem), itemBag, "_OnAwayItem");
        Connect(nameof(UseItem), itemBag, "_OnUseItem");
        Connect(nameof(CycleItem), itemBag, "_OnCycleItem");

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.InitPlayer(this);
        Connect(nameof(Damaged), gg, "_OnPlayerDamaged");
        Connect(nameof(Died), gg, "_OnPlayerDied");
        gg.Connect("AddedWeapon", this, nameof(_OnGGAddedWeapon));
        gg.Connect("ItemHealthUsed", this, nameof(_OnItemHealthUsed));
        gg.Connect("FreePlayer", this, nameof(_OnGGFreePlayer));
    }

    public override void _Process(float delta)
    {
        AxisInput = _MoveInput();
        var animTree = GetNode<AnimationTree>("AnimationTree");
        if (HasWeapon)
        {
            animTree.Set("parameters/with_pipe/current", 1);
        }
        else
        {
            animTree.Set("parameters/with_pipe/current", 0);
        }
        if (AxisInput.IsEqualApprox(Vector2.Zero))
        {
            if (!GetNode<Timer>("AttackTimer").IsStopped())
            {
                animTree.Set("parameters/idle_run/current", 2);
            }
            else
            {
                animTree.Set("parameters/idle_run/current", 0);
            }
        }
        else
        {
            if (!Mathf.IsZeroApprox(AxisInput.x)) GetNode<Sprite>("Sprite").FlipH = AxisInput.x < 0;
            animTree.Set("parameters/idle_run/current", 1);
        }
        _ActInput();
    }

    private Vector2 _MoveInput()
    {
        if (!AllowAxisInput) return Vector2.Zero;
        var x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        var y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
        var vector2 = new Vector2(x, y);
        return vector2;
    }

    private void _ActInput()
    {
        if (HasWeapon && Input.IsActionJustPressed("attack") && GetNode<Timer>("AttackTimer").IsStopped())
        {
            GetNode<Timer>("AttackTimer").Start(0.4f);
            AllowAxisInput = false;
            GetNode<AudioStreamPlayer2D>("SwingSound").Play();
        }
        else if (Input.IsActionJustPressed("interact_item"))
        {
            EmitSignal(nameof(UseItem));
        }
        else if (Input.IsActionJustPressed("cycle"))
        {
            EmitSignal(nameof(CycleItem));
        }
    }

    private void _OnStunChanged(bool stun)
    {
        var timer = GetNode<Timer>("AttackTimer");
        if (!timer.IsStopped() && stun) GetNode<Timer>("AttackTimer").Stop();

        var animTree = GetNode<AnimationTree>("AnimationTree");

        if (stun)
        {
            animTree.Set("parameters/motion_hurt/current", 1);
            return;
        }
        animTree.Set("parameters/motion_hurt/current", 0);
    }

    private void _OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("item"))
        {
            EmitSignal(nameof(FoundItem), area);
        }
    }

    private void _OnAreaExited(Area2D area)
    {
        if (area.IsInGroup("item"))
        {
            EmitSignal(nameof(AwayItem), area);
        }
    }

    private void _HitEnemy(Node body)
    {
        var enemy = body as GroundCharacter;
        enemy.Stun();
        enemy.Push(Position.DirectionTo(enemy.Position), 600);
        enemy.SetHealthLethal(enemy.Health - 1);
        EmitSignal(nameof(HitBody), body);
    }

    private void _HitProp(Node body)
    {
        var prop = body as GroundCharacter;
        prop.SetHealthLethal(prop.Health - 1);
        EmitSignal(nameof(HitBody), body);
    }

    private void _OnHitAreaBodyEntered(Node body)
    {
        if (body.IsInGroup("enemy"))
        {
            _HitEnemy(body);
            return;
        }
        if (body.IsInGroup("breakable_prop"))
        {
            _HitProp(body);
            GetNode<AudioStreamPlayer>("CrateBreakSound").Play();
            return;
        }
        
    }

    private void _OnAttackTimerTimeout()
    {
        AllowAxisInput = true;
    }

    private void _OnDied()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
    }

    private void _OnDamaged(int dmg)
    {
        GetNode<AudioStreamPlayer2D>("HurtSound").Play();
    }

    private void _OnGGAddedWeapon()
    {
        HasWeapon = true;
    }

    private void _OnItemHealthUsed(int healAmount)
    {
        Health += healAmount;
        GetNode<AudioStreamPlayer2D>("HealSound").Play();
    }

    private void _OnGGFreePlayer()
    {
        QueueFree();
    }
}
