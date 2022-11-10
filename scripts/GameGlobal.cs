using Godot;
using GDColl = Godot.Collections;
using System;

public class GameGlobal : Node
{
    [Signal]
    delegate void AddedKey(int n);

    private GDColl.Dictionary gameData = new GDColl.Dictionary();
    private PackedScene playerPacked;
    private PackedScene playerCamPacked;

    public override void _Ready()
    {
        gameData.Add("items", new GDColl.Dictionary());
        gameData.Add("keys", new GDColl.Array());

        playerPacked = ResourceLoader.Load<PackedScene>("res://scenes/objects/Player.tscn");
        playerCamPacked = ResourceLoader.Load<PackedScene>("res://scenes/objects/PlayerCam.tscn");
    }

    async private void _OnInterDoorChangeScene(string newMapFile, string exitPPath)
    {
        GetTree().ChangeScene(newMapFile);
        await ToSignal(GetTree(), "node_added");

        var exit = GetTree().CurrentScene.GetNode<Node2D>(exitPPath);
        var destination = exit.Position;
        var scene = exit.GetParent();
        if (scene.HasNode("Player"))
        {
            scene.GetNode("Player").QueueFree();
        }
        var player = playerPacked.Instance<Player>();
        player.Position = destination;
        scene.AddChild(player);
        var cam = playerCamPacked.Instance<Camera2D>();
        cam.Position = new Vector2(0, -32);
        player.AddChild(cam);
        cam.Current = true;
        player.Connect("Hurt", cam, "_OnPlayerHurt");
        player.Connect("HitBody", cam, "_OnPlayerHitBody");

        var fade = GetTree().CurrentScene.GetNode<FadeUI>("CanvasLayer/FadeUI");
        fade.GetNode<ColorRect>("ColorRect").Modulate = new Color(Colors.White, 1);
        await ToSignal(fade.GetNode<Tween>("Tween"), "ready");
        fade.StartsFading(false);
    }

    public GDColl.Array GetKeys()
    {
        var keys = gameData["keys"] as GDColl.Array;
        return keys;
    }

    private void _OnAddKey(int n)
    {
        EmitSignal(nameof(AddedKey), n);
        var keys = gameData["keys"] as GDColl.Array;
        if (keys.Contains(n)) return;
        keys.Add(n);
    }

    private void _OnItemNoteInit(NodePath nodePath)
    {
        var i1 = gameData["items"] as GDColl.Dictionary;
        if (!i1.Contains(nodePath)) return;
        i1 = i1[nodePath] as GDColl.Dictionary;
        var searched = (bool) i1["searched"];
        var itemNote = GetNode(nodePath);
        itemNote.Set("IsSearched", searched);
    }

    private void _OnNoteSearched(bool searched, NodePath nodePath)
    {
        var note = new GDColl.Dictionary();
        note.Add("type", "item_note");
        note.Add("searched", searched);
        var items = gameData["items"] as GDColl.Dictionary;
        items[nodePath] = note;
    }
}
