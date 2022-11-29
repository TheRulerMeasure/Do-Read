using Godot;
using GDColl = Godot.Collections;
using System;

public class Dialog : Control
{
    [Signal]
    delegate void Next();
    [Signal]
    delegate void DelaySkipped();
    [Signal]
    delegate void Finished();

    [Export(PropertyHint.File)]
    public string DialogData;

    private GDColl.Array _dialogs;
    private int _curDialogIndex = 0;

    public override void _Ready()
    {
        Visible = false;
        SetProcess(false);
        GetNode<Timer>("CharTimer").Connect("timeout", this, nameof(_OnCharTimerTimeout));

        var gg = GetNode<GameGlobal>("/root/GameGlobal");
        gg.Connect("DialogInteracted", this, nameof(_OnDialogInteracted));
        Connect(nameof(Finished), gg, "_OnDialogFinished");
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("interact_item") || Input.IsActionJustPressed("attack") || Input.IsActionJustPressed("cycle"))
        {
            var label = GetNode<Label>("Node2D/Control/NinePatchRect/MarginContainer/Label");
            if (label.VisibleCharacters < label.GetTotalCharacterCount())
            {
                label.VisibleCharacters = label.GetTotalCharacterCount();
                GetNode<Timer>("CharTimer").Stop();
                EmitSignal(nameof(DelaySkipped));
            }
            else
            {
                _NextDialog();
            }
        }
    }

    public void StartDialog(string dialogData)
    {
        Visible = true;
        SetProcess(true);
        _curDialogIndex = 0;
        _dialogs = _GetDialog(dialogData);
        _AScript();
        // _Display(text, delay);
    }

    private void _NextDialog()
    {
        _curDialogIndex++;
        if (_curDialogIndex >= _dialogs.Count)
        {
            Visible = false;
            SetProcess(false);
            EmitSignal(nameof(Finished));
            // GD.Print("finished Visible", Visible);
            return;
        }
        _AScript();
        // _Display(text, delay);
        EmitSignal(nameof(Next));
    }

    private void _AScript()
    {
        var name = (string) ((GDColl.Dictionary)  _dialogs[_curDialogIndex])["name"];
        var focusLeft = (bool) ((GDColl.Dictionary)  _dialogs[_curDialogIndex])["focus_left"];
        var imageLeft = ((GDColl.Dictionary)  _dialogs[_curDialogIndex])["image_left"];
        var imageLeftNum = (int) GD.Convert(imageLeft, Variant.Type.Int);
        var imageRight = ((GDColl.Dictionary)  _dialogs[_curDialogIndex])["image_right"];
        var imageRightNum = (int) GD.Convert(imageRight, Variant.Type.Int);
        var text = (string) ((GDColl.Dictionary)  _dialogs[_curDialogIndex])["text"];
        var delay = (float) ((GDColl.Dictionary)  _dialogs[_curDialogIndex])["char_delay"];
        _Display(text, name, focusLeft, imageLeftNum, imageRightNum, delay);
    }

    private void _SpritePop(Sprite sp, bool focus)
    {
        Vector2 start, end;
        if (focus)
        {
            start = Vector2.Zero;
            end = new Vector2(0, -15);
        }
        else
        {
            start = new Vector2(0, -15);
            end = Vector2.Zero;
        }
        var tween = GetNode<Tween>("Tween");
        tween.InterpolateProperty(
            sp,
            "position",
            start,
            end,
            0.3f,
            Tween.TransitionType.Sine,
            Tween.EaseType.Out
        );
        tween.Start();
    }

    private void _TellName(string name, bool focusLeft)
    {
        Label nameLabel;
        NinePatchRect patch;
        if (focusLeft)
        {
            GetNode<NinePatchRect>("Node2D/Control/NameRight/Patch").Visible = false;
            patch = GetNode<NinePatchRect>("Node2D/Control/NameLeft/Patch");
            patch.Visible = true;
            nameLabel = GetNode<Label>("Node2D/Control/NameLeft/Patch/MarginContainer/Label");
        }
        else
        {
            GetNode<NinePatchRect>("Node2D/Control/NameLeft/Patch").Visible = false;
            patch = GetNode<NinePatchRect>("Node2D/Control/NameRight/Patch");
            patch.Visible = true;
            nameLabel = GetNode<Label>("Node2D/Control/NameRight/Patch/MarginContainer/Label");
        }
        nameLabel.Text = name;
    }

    private void _Display(string text, string name, bool focusLeft, int imageLeft, int imageRight, float charDelay=0.05f)
    {
        if (imageLeft >= 0 && imageLeft <= 3)
        {
            var sp = GetNode<Sprite>("PosLeft/Sprite");
            sp.Visible = true;
            sp.Frame = imageLeft;
            _SpritePop(sp, focusLeft);
            // if (focusLeft) _SpritePop(sp, true);
            // else _SpritePop(sp, false);
        }
        else
        {
            var sp = GetNode<Sprite>("PosLeft/Sprite");
            sp.Visible = false;
        }

        if (imageRight >= 0 && imageRight <= 3)
        {
            var sp = GetNode<Sprite>("PosRight/Sprite");
            sp.Visible = true;
            sp.Frame = imageRight;
            _SpritePop(sp, !focusLeft);
            // if (!focusLeft) sp.Position = new Vector2(-50, -50);
            // else sp.Position = Vector2.Zero;
        }
        else
        {
            var sp = GetNode<Sprite>("PosRight/Sprite");
            sp.Visible = false;
        }
        _TellName(name, focusLeft);

        var label = GetNode<Label>("Node2D/Control/NinePatchRect/MarginContainer/Label");
        label.Text = text;
        label.VisibleCharacters = 1;
        GetNode<Timer>("CharTimer").WaitTime = charDelay;
        GetNode<Timer>("CharTimer").Start();
        GetNode<AudioStreamPlayer>("CharSound").Play();
    }

    private GDColl.Array _GetDialog(string dialogData)
    {
        DialogData = dialogData;
        var f = new File();
        if (!f.FileExists(DialogData))
        {
            GD.PrintErr("File Path doesn't exist");
            return new GDColl.Array();
        }
        f.Open(DialogData, File.ModeFlags.Read);
        var data = JSON.Parse(f.GetAsText());
        var dict = data.Result as GDColl.Dictionary;
        // var arr = new GDColl.Array(dict["dialogs"]);
        return dict["dialogs"] as GDColl.Array;
    }

    private void _OnCharTimerTimeout()
    {
        var label = GetNode<Label>("Node2D/Control/NinePatchRect/MarginContainer/Label");
        GetNode<AudioStreamPlayer>("CharSound").Play();
        if (label.VisibleCharacters < label.GetTotalCharacterCount())
        {
            label.VisibleCharacters++;
            GetNode<Timer>("CharTimer").Start();
        }
    }

    private void _OnDialogInteracted(string dialogData)
    {
        if (Visible) return;
        StartDialog(dialogData);
    }
}
