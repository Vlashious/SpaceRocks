using Godot;
using System;
using System.Collections.Generic;

public class Main : Node
{
    [Export] private readonly PackedScene _rock;
    [Export] private readonly PackedScene _enemy;
    private int _level = 0;
    private int _score = 0;
    private bool _isPlaying = false;
    private void _on_Player_ShootSignal(PackedScene scene, Vector2 pos, float rot)
    {
        var b = scene.Instance() as Bullet;
        if (b == null)
        {
            var enemyBullet = scene.Instance() as EnemyBullet;
            enemyBullet.Start(pos, rot);
            AddChild(enemyBullet);
            return;
        }
        b.Start(pos, rot);
        AddChild(b);
    }

    public override void _Ready()
    {
        foreach (var i in Range(0, 3))
        {
            SpawnRock(3);
        }
    }

    public override void _Process(float delta)
    {
        if (_isPlaying && GetNode<Node>("Rocks").GetChildCount() == 0)
        {
            GetNode<Player>("Player").ChangeState(Player.State.INV);
            NewLevel();
        }
    }

    private void SpawnRock(int size, Vector2? pos = null, Vector2? vel = null)
    {
        if (!pos.HasValue)
        {
            var path = GetNode<PathFollow2D>("RockPath/RockSpawn");
            path.Offset = GD.Randi();
            pos = path.Position;
        }
        if (!vel.HasValue)
        {
            vel = new Vector2(1, 0).Rotated((float)GD.RandRange(0, 2 * Math.PI)) * (float)GD.RandRange(100, 150);
        }
        var rock = _rock.Instance() as Rock;
        rock.Start(pos.Value, vel.Value, size);
        GetNode<Node>("Rocks").AddChild(rock);
        rock.Connect("Exploded", this, "_on_Rock_exploded");
    }

    private async void NewGame()
    {
        GetNode<AudioStreamPlayer2D>("Music").Play();
        foreach (Rock rock in GetNode<Node>("Rocks").GetChildren())
        {
            rock.QueueFree();
        }
        _level = 0;
        _score = 0;
        GetNode<HUD>("HUD").UpdateScore(_score);
        GetNode<Player>("Player").Start();
        GetNode<HUD>("HUD").ShowMessage("Get Ready!");
        await ToSignal(GetNode<Timer>("HUD/MessageTimer"), "timeout");
        _isPlaying = true;
        NewLevel();
    }

    private void NewLevel()
    {
        GetNode<AudioStreamPlayer2D>("LevelUpSound").Play();
        _level++;
        GetNode<HUD>("HUD").ShowMessage($"Wave {_level}");
        GetNode<Timer>("EnemyTimer").WaitTime = (float)GD.RandRange(5, 8);
        GetNode<Timer>("EnemyTimer").Start();
        foreach (var i in Range(0, _level)) SpawnRock(3);
    }

    private void GameOver()
    {
        GetNode<AudioStreamPlayer2D>("Music").Stop();
        _isPlaying = false;
        GetNode<HUD>("HUD").GameOver();
    }

    IEnumerable<int> Range(int from, int to)
    {
        for (int i = from; i < to; i++)
        {
            yield return i;
        }
    }

    private void _on_Rock_exploded(int size, int radius, Vector2 pos, Vector2 vel)
    {
        GetNode<AudioStreamPlayer2D>("ExplodeSound").Play();
        if (size == 1) return;
        foreach (var offset in new int[] { -1, 1 })
        {
            var dir = (pos - GetNode<Player>("Player").Position).Normalized().Tangent() * offset;
            var newPos = pos + dir * radius;
            var newvel = dir * vel.Length() * 1.1f;
            SpawnRock(size - 1, newPos, newvel);
        }
    }
    private void _on_EnemyTimer_timeout()
    {
        var e = _enemy.Instance() as Enemy;
        AddChild(e);
        e.Target = GetNode<Player>("Player");
        e.Connect("ShootSignal", this, "_on_Player_ShootSignal");
        GetNode<Timer>("EnemyTimer").WaitTime = (float)GD.RandRange(10, 20);
        GetNode<Timer>("EnemyTimer").Start();
    }
    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("pause"))
        {
            if (!_isPlaying)
            {
                return;
            }

            GetTree().Paused = !GetTree().Paused;
            if (GetTree().Paused)
            {
                GetNode<Label>("HUD/MessageLabel").Text = "Paused";
                GetNode<Label>("HUD/MessageLabel").Show();
            }
            else
            {
                GetNode<Label>("HUD/MessageLabel").Text = "";
                GetNode<Label>("HUD/MessageLabel").Hide();
            }
        }
    }
}
