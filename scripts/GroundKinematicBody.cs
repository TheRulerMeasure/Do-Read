using Godot;
using System;

public class GroundKinematicBody : KinematicBody2D
{
    [Signal]
    public delegate void Pushed(Vector2 dir, float force);

    [Export]
    private float Acceleration = 2750f;
    [Export]
    private float MaxSpeed = 247f;
    [Export]
    public bool AllowAxisInput = true;
    [Export]
    public Vector2 AxisInput = Vector2.Zero;

    private Vector2 _motion = Vector2.Zero;

    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.Connect("PhysicProcessAll", this, nameof(_OnGGPhysicProcess));
    }

    public override void _PhysicsProcess(float delta)
    {
        var axis = _GetAxisInput();
        if (axis.IsEqualApprox(Vector2.Zero))
        {
            _ApplyFriction(Acceleration * delta);
        }
        else
        {
            _ApplyMovement(axis * Acceleration * delta);
        }
        _motion = MoveAndSlide(_motion);
    }

    public void Push(Vector2 dir, float force)
    {
        _motion += dir*force;
        EmitSignal(nameof(Pushed), dir, force);
    }

    private Vector2 _GetAxisInput()
    {
        if (!AllowAxisInput) return Vector2.Zero;
        if (AxisInput == null) return Vector2.Zero;
        return AxisInput.Normalized();
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

    private void _OnGGPhysicProcess(bool process)
    {
        SetPhysicsProcess(process);
    }
}
