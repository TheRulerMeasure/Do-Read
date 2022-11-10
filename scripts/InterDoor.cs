using Godot;
using System;

public class InterDoor : Area2D
{
    [Signal]
    delegate void DoorEnter(string mapFile, string exitPath);

    [Export]
    private NodePath ExitPositionPath;
    [Export(PropertyHint.File)]
    private string NewMapFile;
    [Export]
    private string ExitPositionPathString;
    [Export]
    private NodePath FadeUIPath;

    public override void _Ready()
    {
        Connect("body_entered", this, nameof(_OnBodyEntered));
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(DoorEnter), gg, "_OnInterDoorChangeScene");
    }

    private void _StartsFading()
    {
        FadeUI fade;
        if (FadeUIPath == null)
        {
            fade = GetTree().CurrentScene.GetNode<FadeUI>("CanvasLayer/FadeUI");
        }
        else
        {
            fade = GetNode<FadeUI>(FadeUIPath);
        }
        fade.StartsFading(true, 0.3f);
        fade.Connect("DoneFading", this, nameof(_OnFadeUIDoneFading));
    }

    private void _OnFadeUIDoneFading(bool fadeOut)
    {
        EmitSignal(nameof(DoorEnter), NewMapFile, ExitPositionPathString);
    }

    private void _OnBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            if (ExitPositionPath != null) return;
            var player = body as Player;
            player.SetProcess(false);
            player.SetPhysicsProcess(false);
            _StartsFading();
        }
    }
}
