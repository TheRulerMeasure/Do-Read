using Godot;
using System;

public class FadeUI : Control
{
    [Signal]
    delegate void DoneFading(bool fadeOut);

    private bool _fadeOut = false;

    public override void _Ready()
    {
        var tween = GetNode<Tween>("Tween");
        tween.Connect("tween_all_completed", this, nameof(_OnTweenAllCompleted));
        // StartsFading(true);
    }

    public void StartsFading(bool fadeOut, float duration=0.9f)
    {
        var tween = GetNode<Tween>("Tween");
        var colorRect = GetNode<ColorRect>("ColorRect");
        if (fadeOut)
        {
            tween.InterpolateProperty(
                colorRect,
                "modulate",
                new Color(Colors.White, 0),
                Colors.White,
                duration
            );
        }
        else
        {
            tween.InterpolateProperty(
                colorRect,
                "modulate",
                Colors.White,
                new Color(Colors.White, 0),
                duration
            );
        }
        _fadeOut = fadeOut;
        tween.Start();
    }

    private void _OnTweenAllCompleted()
    {
        EmitSignal(nameof(DoneFading), _fadeOut);
    }
}
