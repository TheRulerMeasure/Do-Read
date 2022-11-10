using Godot;
using System;

public class DoubleDoor : Node2D
{
    [Export]
    private bool Openned = false;

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
        var area = GetNode<Area2D>("Area2D");
        area.Connect("body_entered", this, nameof(_OnBodyEntered));
        area.Connect("body_exited", this, nameof(_OnBodyExited));
        var animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        if (Openned) animPlayer.Play("open_idle");
        else animPlayer.Play("close_idle");
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
            Open();
            return;
        }
        Close();
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

    private void _OnBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            _Blur(true);
        }
    }

    private void _OnBodyExited(Node body)
    {
        if (body.IsInGroup("player"))
        {
            _Blur(false);
        }
    }
}
