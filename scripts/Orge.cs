using Godot;
using System;

public class Orge : KinematicBody2D
{
    [Signal]
    delegate void Died();
    [Signal]
    delegate void Hurt(int dmg);

    private bool _hitAreaDisabled = true;

    private int _health = 3;

    [Export]
    private int MaxHealth = 3;
    [Export]
    public int Health {
        get => _health;
        set => _SetHealth(value);
    }
    [Export]
    private float MaxSpeed = 97f;
    [Export]
    private float Acceleration = 360f;
    [Export]
    public bool HitAreaDisabled {
        get => _hitAreaDisabled;
        set => _SetHitAreaDisabled(value);
    }
    private Vector2 _motion = Vector2.Zero;
    private Player _player;

    public float Attack = 0f;

    public float Stagger = 0f;

    public override void _Ready()
    {
        var seekArea = GetNode<Area2D>("SeekArea");
        seekArea.Connect("body_entered", this, nameof(_OnBodyEntered));
        seekArea.Connect("body_exited", this, nameof(_OnBodyExited));
        var hitArea = GetNode<Area2D>("HitArea");
        hitArea.Connect("body_entered", this, nameof(_OnHitAreaBodyEntered));
        // hitArea.Connect("body_exited", this, nameof(_OnHitAreaBodyExited));
    }

    public override void _PhysicsProcess(float delta)
    {
        var animTree = GetNode<AnimationTree>("AnimationTree");
        var axis = Stagger > 0f || Attack > 0f ? Vector2.Zero : _GetAxisInput();

        if (axis == Vector2.Zero)
        {
            if (Stagger > 0f)
            {
                animTree.Set("parameters/idle_walk/current", 0);
            }
            else if (Attack > 0f)
            {
                animTree.Set("parameters/movement_attack/current", 1);
            }
            else
            {
                animTree.Set("parameters/idle_walk/current", 0);
            }
            _ApplyFriction(Acceleration * delta);
        }
        else
        {
            _ApplyMovement(axis * Acceleration * delta);
            animTree.Set("parameters/movement_attack/current", 0);
            animTree.Set("parameters/idle_walk/current", 1);
        }
        _motion = MoveAndSlide(_motion);
        Stagger -= delta;
        if (Stagger < 0f) Stagger = 0f;
        Attack -= delta;
        if (Attack < 0f) Attack = 0f;
    }

    public void SetHealth(int health, bool lethal=true)
    {
        var oldHealth = Health;
        Health = health;
        // GD.Print(Name, Health);
        if (!lethal) return;
        EmitSignal(nameof(Hurt), oldHealth - health);
        if (Health <= 0)
        {
            EmitSignal(nameof(Died));
            QueueFree();
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

    private Vector2 _GetAxisInput()
    {
        if (_player == null)
        {
            return Vector2.Zero;
        }
        Vector2 pos;
        var sprite = GetNode<Sprite>("Sprite");
        if (_player.Position.x > Position.x)
        {
            sprite.FlipH = false;
            pos = GetNode<CollisionShape2D>("HitArea/RightCol").GlobalPosition;
        }
        else
        {
            sprite.FlipH = true;
            pos = GetNode<CollisionShape2D>("HitArea/LeftCol").GlobalPosition;
        }
        // GD.Print(pos.DistanceSquaredTo(_player.Position));
        if (pos.DistanceSquaredTo(_player.Position) <= 1600)
        {
            if (Attack <= 0f)
            {
                Attack += 0.7f;
            }
            return Vector2.Zero;
        }
        return pos.DirectionTo(_player.Position);
    }

    private void _ApplyMovement(Vector2 acc)
    {
        _motion += acc;
        _motion = _motion.LimitLength(MaxSpeed);
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

    private void _OnBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            _player = body as Player;
        }
    }

    private void _OnBodyExited(Node body)
    {
        if (body.IsInGroup("player"))
        {
            _player = null;
        }
    }

    private void _OnHitAreaBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            if (_player == null) return;
            _player.SetHealth(_player.Health - 1);
            _player.Push(Position.DirectionTo(_player.Position), 470f, 0.2f);
        }
    }

    // private void _OnHitAreaBodyExited(Node body)
    // {

    // }
}
