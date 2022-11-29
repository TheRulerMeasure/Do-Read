using Godot;
using System;

public class InfoBGMusic : Node
{
    [Signal]
    delegate void PlayMusic(int musicIndex);

    [Export(PropertyHint.Enum, "ColorDiving, OneLastLookBack, NoMusic")]
    private int Music = 0;

    public override void _Ready()
    {
        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        Connect(nameof(PlayMusic), gg, "_OnMusicPlayMusic");
    }

    private void _OnTriggered(bool high)
    {
        EmitSignal(nameof(PlayMusic), Music);
    }
}
