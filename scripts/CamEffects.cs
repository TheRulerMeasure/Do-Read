using Godot;
using System;

public class CamEffects : Camera2D
{
    [Export]
    private float ShakeDecay = 12f;
    [Export]
    private float NoiseShakeSpeed = 20f;
    private float _noiseI = 0f;
    private float _shakeStrength = 0f;
    private RandomNumberGenerator _rand = new RandomNumberGenerator();
    private OpenSimplexNoise _noise = new OpenSimplexNoise();

    public override void _Ready()
    {
        _rand.Randomize();

        _noise.Seed = (int) _rand.Randi();
        _noise.Period = 2;
    }

    public override void _Process(float delta)
    {
        if (!Mathf.IsZeroApprox(_shakeStrength))
        {
            _shakeStrength = Mathf.Lerp(_shakeStrength, 0, ShakeDecay * delta);
        }
        this.Offset = _GetNoiseOffset(delta);
    }

    public void ApplyShake(float strength=60f)
    {
        _shakeStrength += strength;
    }

    private Vector2 _GetNoiseOffset(float delta)
    {
        _noiseI += NoiseShakeSpeed * delta;

        return new Vector2(
            _noise.GetNoise2d(1, _noiseI) * _shakeStrength,
            _noise.GetNoise2d(100, _noiseI) * _shakeStrength
        );
    }

    private void _OnPlayerHurt(int dmg)
    {
        ApplyShake(60*dmg);
    }

    private void _OnPlayerHitBody(Node body)
    {
        ApplyShake(50);
    }
}
