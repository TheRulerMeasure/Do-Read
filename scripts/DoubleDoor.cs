using Godot;
using System;

public class DoubleDoor : Node2D
{
    // [Signal]
    // delegate void Triggered(NodePath path);

    [Export]
    private bool Locked = false;
    [Export]
    private Color DoorColor = Colors.White;

    public float TheSize1 {
        get => (float) GetNode<Sprite>("DoorLeft").Material.Get("shader_param/the_size");
        set => GetNode<Sprite>("DoorLeft").Material.Set("shader_param/the_size", value);
    }

    public float TheSize2 {
        get => (float) GetNode<Sprite>("DoorRight").Material.Get("shader_param/the_size");
        set => GetNode<Sprite>("DoorRight").Material.Set("shader_param/the_size", value);
    }

    public override void _Ready()
    {
        GetNode<Sprite>("DoorLeft").Modulate = DoorColor;
        GetNode<Sprite>("DoorRight").Modulate = DoorColor;

        var area = GetNode<Area2D>("AboveFadeArea");
        area.Connect("body_entered", this, nameof(_OnAboveFadeBodyEntered));
        area.Connect("body_exited", this, nameof(_OnAboveFadeBodyExited));

        var areaDoor = GetNode<Area2D>("AreaDoor");
        areaDoor.Connect("body_entered", this, nameof(_OnAreaDoorBodyEntered));
        areaDoor.Connect("body_exited", this, nameof(_OnAreaDoorBodyExited));

        // var gg = GetNode<GameGlobal>("/root/GameGlobal");
        // gg.InitDoubleDoor(this.GetPath());
        // Connect(nameof(Triggered), gg, "_OnDoubleDoorTriggered");

        GetNode<AnimationPlayer>("AnimationPlayer").Play("close_idle");
    }

    public override void _Process(float delta)
    {
        var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        if (!animPlayer.IsPlaying()) return;
        TheSize1 = Mathf.Abs(GetNode<Sprite>("DoorLeft").Position.x) / 48;
        TheSize2 = 1 - Mathf.Abs(GetNode<Sprite>("DoorRight").Position.x) / 48;
    }

    public void Open()
    {
        var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animPlayer.Play("open");
    }

    public void Close()
    {
        var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animPlayer.Play("close");
    }

    private void _OnTriggered(bool high)
    {
        if (high)
        {
            Locked = false;
            // EmitSignal(nameof(Triggered), GetPath());
            // GD.Print("door high");
        }
    }

    private void _Blur(bool blurOut)
    {
        var tween = GetNode<Tween>("Tween");
        tween.StopAll();

        if (blurOut)
        {
            tween.InterpolateProperty(
                this,
                "modulate",
                Colors.White,
                new Color(Colors.White, 0.75f),
                0.2f,
                Tween.TransitionType.Cubic,
                Tween.EaseType.InOut
            );
            tween.Start();
            return;
        }
        tween.InterpolateProperty(
            this,
            "modulate",
            new Color(Colors.White, 0.75f),
            Colors.White,
            0.2f,
            Tween.TransitionType.Cubic,
            Tween.EaseType.InOut
        );
        tween.Start();
    }

    private void _OnAboveFadeBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            _Blur(true);
        }
    }

    private void _OnAboveFadeBodyExited(Node body)
    {
        if (body.IsInGroup("player"))
        {
            _Blur(false);
        }
    }

    private void _OnAreaDoorBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            if (Locked) return;
            Open();
        }
    }

    private void _OnAreaDoorBodyExited(Node body)
    {
        if (body.IsInGroup("player"))
        {
            if (Locked) return;
            Close();
        }
    }
}
