using Godot;
using GDColl = Godot.Collections;
using System;

public class GameGlobal : Node
{
    [Signal]
    delegate void CriticalLevelChanged(int curLevel);
    [Signal]
    delegate void FadeLevelChange(bool fadeOut);
    [Signal]
    delegate void PhysicProcessAll(bool process);
    [Signal]
    delegate void DialogInteracted(string dialogData);
    [Signal]
    delegate void DialogFinished();
    [Signal]
    delegate void KeyAdded(int keyIndex);
    [Signal]
    delegate void AddedWeapon();
    [Signal]
    delegate void ItemHealthUsed();
    [Signal]
    delegate void FreePlayer();

    private GDColl.Array<int> _keys = new GDColl.Array<int>();
    private GDColl.Array<NodePath> _searchedItemPaths = new GDColl.Array<NodePath>();
    private GDColl.Array<NodePath> _deadEnemyPaths = new GDColl.Array<NodePath>();
    private GDColl.Array<NodePath> _deadCratePaths = new GDColl.Array<NodePath>();
    private GDColl.Array<NodePath> _triggeredInfoDestroyPaths = new GDColl.Array<NodePath>();
    // private GDColl.Array<NodePath> _disposedNodePaths = new GDColl.Array<NodePath>();
    private bool _enteredLevel = false;
    private PackedScene _playerPacked;
    private string _curDeplayPath;

    [Export]
    private bool PlayerHasWeapon = false;
    [Export]
    private int PlayerHealth = 5;
    [Export]
    private int PlayerMaxHealth = 10;

    public override void _Ready()
    {
        GetNode<Timer>("RespawnTimer").Connect("timeout", this, nameof(_OnRespawnTimerTimeout));
        _playerPacked = ResourceLoader.Load<PackedScene>("res://scenes/objects/Play.tscn");
    }

    public void InitKeyNoti(NodePath nodePath)
    {
        var keyNoti = GetNode<KeyNotifier>(nodePath);
        if (!_keys.Contains(keyNoti.KeyIndex)) return;
        keyNoti.GlobalHasKey();
    }

    public void InitInfoDestroy(NodePath path)
    {
        if (!_triggeredInfoDestroyPaths.Contains(path)) return;
        GetNode<InfoDestroy>(path).DestroyObjects();
    }

    // public void InitDoubleDoor(NodePath path)
    // {
    //     if (!_triggeredDoubleDoors.Contains(path)) return;
    //     GetNode(path).Call("_OnTriggered", true);
    // }

    public void InitHealthIndicator(HealthIndi healthIndi)
    {
        var criticalLevel = _CalcCriticalLevel();
        healthIndi.SetLevel(criticalLevel);
    }

    // public void InitDisposible(NodePath path)
    // {
    //     if (!_disposedNodePaths.Contains(path)) return;
    //     GetNode(path).QueueFree();
    // }

    public void InitPlayer(Play player)
    {
        player.MaxHealth = PlayerMaxHealth;
        player.Health = PlayerHealth;
        player.HasWeapon = PlayerHasWeapon;
    }

    public void InitCrate(NodePath cratePath)
    {
        if (!_deadCratePaths.Contains(cratePath)) return;
        GetNode(cratePath).QueueFree();
    }

    private void _OnPlayerDamaged(int damage)
    {
        PlayerHealth -= damage;
        var criticalLevel = _CalcCriticalLevel();
        EmitSignal(nameof(CriticalLevelChanged), criticalLevel);
    }

    public void InitItem(NodePath itemPath)
    {
        if (!_searchedItemPaths.Contains(itemPath)) return;
        GetNode<Item>(itemPath).Searched = true;
    }

    public void InitEnemy(NodePath enemyPath)
    {
        if (!_deadEnemyPaths.Contains(enemyPath)) return;
        GetNode(enemyPath).QueueFree();
    }

    public void InitFadeUI(NodePath fadeUIPath)
    {
        if (!_enteredLevel) return;
        var fade = GetNode<FadeUI>(fadeUIPath);
        fade.GetNode<ColorRect>("ColorRect").Modulate = Colors.White;
        fade.StartsFading(false, 0.2f);
        _enteredLevel = false;
    }

    private void _OnEnemyDied(NodePath enemyPath)
    {
        if (_deadEnemyPaths.Contains(enemyPath)) return;
        _deadEnemyPaths.Add(enemyPath);
    }

    private void _OnAddedKey(int n)
    {
        if (_keys.Contains(n)) return;
        _keys.Add(n);
        EmitSignal(nameof(KeyAdded), n);
    }

    private void _OnSearchChanged(bool searched, NodePath itemPath)
    {
        if (_searchedItemPaths.Contains(itemPath)) return;
        _searchedItemPaths.Add(itemPath);
    }

    private void _OnEnteredChangeLevel(string newLevel, string exitPathName)
    {
        // GetTree().PhysicsInterpolation = false;
        EmitSignal(nameof(PhysicProcessAll), false);
        EmitSignal(nameof(FadeLevelChange), true);
        var timer = GetNode<Timer>("FadeLvlChangeTimer");
        var arr = new GDColl.Array();
        arr.Add(newLevel);
        arr.Add(exitPathName);
        timer.Connect("timeout", this, nameof(_OnFadeLvlChangeTimerTimeout), arr);
        timer.Start();
    }

    async private void _EnterNewLevel(string newLevel, string exitPathName)
    {
        _enteredLevel = true;
        GetTree().ChangeScene(newLevel);
        await ToSignal(GetTree(), "node_added");

        var dest = GetTree().CurrentScene.GetNode<Node2D>(exitPathName);
        _curDeplayPath = exitPathName;
        if (dest.GetParent().HasNode("Play"))
        {
            dest.GetParent().GetNode("Play").QueueFree();
        }
        var player = _playerPacked.Instance<Play>();
        player.Position = dest.Position;
        dest.GetParent().AddChild(player);
    }

    private void _RespawnPlayer()
    {
        EmitSignal(nameof(FreePlayer));
        PlayerHealth = 5;
        var criticalLevel = _CalcCriticalLevel();
        EmitSignal(nameof(CriticalLevelChanged), criticalLevel);
        var player = _playerPacked.Instance<KinematicBody2D>();
        player.Position = GetTree().CurrentScene.GetNode<Node2D>(_curDeplayPath).Position;
        GetTree().CurrentScene.GetNode(_curDeplayPath).GetParent().AddChild(player);
    }

    private int _CalcCriticalLevel()
    {
        float criticalLevel = Mathf.Min(6, PlayerMaxHealth) - Mathf.Min(6, PlayerHealth);
        criticalLevel /= 1.5f;
        return Mathf.CeilToInt(criticalLevel);
    }

    private void _OnFadeLvlChangeTimerTimeout(string newLevel, string exitPathName)
    {
        _EnterNewLevel(newLevel, exitPathName);
        var timer = GetNode<Timer>("FadeLvlChangeTimer");
        timer.Disconnect("timeout", this, nameof(_OnFadeLvlChangeTimerTimeout));
    }

    private void _OnInterDialogInteracted(string dialogFile, NodePath path)
    {
        EmitSignal(nameof(PhysicProcessAll), false);
        EmitSignal(nameof(DialogInteracted), dialogFile);
        // GD.Print("Dialoging");
        if (this.IsConnected(nameof(DialogFinished), this.GetNode(path), "_OnGGDialogFinished")) return;
        // GD.Print("NotConnected");
        Connect(nameof(DialogFinished), GetNode(path), "_OnGGDialogFinished");
    }

    private void _OnDialogFinished()
    {
        EmitSignal(nameof(DialogFinished));
        EmitSignal(nameof(PhysicProcessAll), true);
    }

    private void _OnAddedWeapon()
    {
        PlayerHasWeapon = true;
        EmitSignal(nameof(AddedWeapon));
    }

    private void _OnItemHealthUsed(int healAmount)
    {
        EmitSignal(nameof(ItemHealthUsed), healAmount);
        PlayerHealth += healAmount;
        if (PlayerHealth > PlayerMaxHealth) PlayerHealth = PlayerMaxHealth;
        var criticalLevel = _CalcCriticalLevel();
        EmitSignal(nameof(CriticalLevelChanged), criticalLevel);
    }

    private void _OnCrateDied(NodePath cratePath)
    {
        if (_deadCratePaths.Contains(cratePath)) return;
        _deadCratePaths.Add(cratePath);
    }

    private void _OnInfoDestroyTriggered(NodePath path)
    {
        if (_triggeredInfoDestroyPaths.Contains(path)) return;
        _triggeredInfoDestroyPaths.Add(path);
    }

    // private void _OnNodeDisposed(NodePath path)
    // {
    //     if (_disposedNodePaths.Contains(path)) return;
    //     _disposedNodePaths.Add(path);
    // }

    private void _OnPlayerDied()
    {
        GD.Print("player dead");
        GetNode<Timer>("RespawnTimer").Start();
    }

    private void _OnRespawnTimerTimeout()
    {
        _RespawnPlayer();
    }

    private void _OnMusicPlayMusic(int musicIndex)
    {
        _PlayMusic(musicIndex);
    }

    private void _PlayMusic(int musicIndex)
    {
        if (musicIndex == 0)
        {
            GetNode<AudioStreamPlayer>("Music2").Stop();
            GetNode<AudioStreamPlayer>("Music1").Play();
            return;
        }
        if (musicIndex == 1)
        {
            GetNode<AudioStreamPlayer>("Music1").Stop();
            GetNode<AudioStreamPlayer>("Music2").Play();
            return;
        }
        GetNode<AudioStreamPlayer>("Music1").Stop();
        GetNode<AudioStreamPlayer>("Music2").Stop();
    }

    private void _OnResetData()
    {
        PlayerHealth = 5;
        PlayerMaxHealth = 10;
        PlayerHasWeapon = false;
        _keys.Clear();
        _searchedItemPaths.Clear();
        _deadEnemyPaths.Clear();
        _deadCratePaths.Clear();
        _triggeredInfoDestroyPaths.Clear();
    }
}
