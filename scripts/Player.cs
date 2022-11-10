using Godot;
using GDColl = Godot.Collections;
using System;

public class Player : KinematicBody2D
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
    delegate void Died();
    [Signal]
    delegate void Hurt(int dmg);
    [Signal]
    delegate void HitBody(Node body);

    private bool _hitAreaDisabled = true;
    private int _health = 5;

    [Export]
    private int MaxHealth = 10;
    [Export]
    public int Health {
        get => _health;
        set => _SetHealth(value);
    }
    [Export]
    private float MaxSpeed = 247f;
    [Export]
    private float Acceleration = 2750f;
    [Export]
    private bool HasWeapon = false;
    [Export]
    public bool HitAreaDisabled {
        get => _hitAreaDisabled;
        set => _SetHitAreaDisabled(value);
    }
    private Vector2 _motion = Vector2.Zero;

    public float Stagger = 0f;
    public float Attack = 0f;

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
        GetNode<Area2D>("Area2D").Connect("area_entered", this, nameof(_OnAreaEntered));
        GetNode<Area2D>("Area2D").Connect("area_exited", this, nameof(_OnAreaExited));
        var itemBag = GetNode<ItemBag>("ItemBag");
        Connect(nameof(FoundItem), itemBag, "_OnFoundItem");
        Connect(nameof(AwayItem), itemBag, "_OnAwayItem");
        Connect(nameof(UseItem), itemBag, "_OnUseItem");
        Connect(nameof(CycleItem), itemBag, "_OnCycleItem");

        var hitArea = GetNode<Area2D>("HitArea");
        hitArea.Connect("body_entered", this, nameof(_OnHitAreaBodyEntered));

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        HasWeapon = gg.GetKeys().Contains(1);

        gg.Connect("AddedKey", this, nameof(_OnGlobalKeyAdded));
    }

    public override void _Process(float delta)
    {
        Attack -= delta;
        if (Attack < 0)
        {
            Attack = 0f;
        }
        if (Input.IsActionJustPressed("interact_item"))
        {
            if (Mathf.IsZeroApprox(_motion.Length())) EmitSignal(nameof(UseItem));
        }
        else if (Input.IsActionJustPressed("cycle"))
        {
            EmitSignal(nameof(CycleItem));
        }
        else if (HasWeapon && Attack <= 0f && Stagger <= 0f && Input.IsActionJustPressed("attack"))
        {
            Attack += 0.35f;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        var axis = Stagger > 0f || Attack > 0f ? Vector2.Zero : _GetAxisInput();
        var animTree = GetNode<AnimationTree>("AnimationTree");
        if (axis == Vector2.Zero)
        {
            _ApplyFriction(Acceleration * delta);
            if (Stagger > 0f)
            {
                animTree.Set("parameters/motion_hurt/current", 1);
            }
            else if (Attack > 0f)
            {
                animTree.Set("parameters/idle_run/current", 2);
            }
            else
            {
                animTree.Set("parameters/motion_hurt/current", 0);
                animTree.Set("parameters/idle_run/current", 0);
                if (HasWeapon)
                {
                    animTree.Set("parameters/with_pipe/current", 1);
                }
                else
                {
                    animTree.Set("parameters/with_pipe/current", 0);
                }
            }
        }
        else
        {
            _ApplyMovement(axis * Acceleration * delta);
            animTree.Set("parameters/motion_hurt/current", 0);
            animTree.Set("parameters/idle_run/current", 1);
        }
        _motion = MoveAndSlide(_motion);
        Stagger -= delta;
        if (Stagger < 0f) Stagger = 0f;
    }

    public void SetHealth(int health, bool lethal=true)
    {
        var oldHealth = Health;
        Health = health;
        // GD.Print(Health);
        if (!lethal) return;
        EmitSignal(nameof(Hurt), oldHealth - health);
        if (Health <= 0)
        {
            EmitSignal(nameof(Died));
            return;
        }
    }

    public void Push(Vector2 dir, float force, float staggerAmount=0.5f)
    {
        Stagger += staggerAmount;
        _motion += dir*force;
        GetNode<Sprite>("Sprite").FlipH = dir.x > 0;
        // GD.Print("Pushed");
    }

    private void _SetHealth(int health)
    {
        _health = health;
        if (_health > MaxHealth)
        {
            _health = MaxHealth;
        }
    }

    private Vector2 _GetAxisInput()
    {
        var x = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        var y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
        if (!Mathf.IsZeroApprox(x)) GetNode<Sprite>("Sprite").FlipH = x < 0;
        var vector2 = new Vector2(x, y);
        return vector2.Normalized();
    }

    private void _ApplyFriction(float amount)
    {
        if (_motion.Length() > amount)
        {
            _motion -= _motion.Normalized() * amount;
            return;
        }
        _motion = Vector2.Zero;
    }

    private void _ApplyMovement(Vector2 acc)
    {
        _motion += acc;
        _motion = _motion.LimitLength(MaxSpeed);
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

    private void _OnGlobalKeyAdded(int n)
    {
        if (HasWeapon) return;
        HasWeapon = n == 1;
    }

    private void _OnHitAreaBodyEntered(Node body)
    {
        if (body.IsInGroup("enemy"))
        {
            body.Call(
                "SetHealth",
                (int) body.Get("Health") - 1,
                true
            );
            body.Call(
                "Push",
                Position.DirectionTo((Vector2) body.Get("position")),
                250f,
                0.5f
            );
            EmitSignal(nameof(HitBody), body);
        }
    }

    // private void _OnAnimFinished(string animName)
    // {
    //     GD.Print(animName);
    //     if (animName == "attack_pipe")
    //     {
    //         var animTree = GetNode<AnimationTree>("AnimationTree");
    //         animTree.Set("parameters/motion_attack/current", 0);
    //         Attacking = false;
    //     }
    // }
}
