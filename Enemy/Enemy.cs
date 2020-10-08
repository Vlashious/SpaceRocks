using Godot;
using System;

public class Enemy : Area2D
{
    [Signal] delegate void Shoot();
    [Export] private readonly PackedScene _bullet;
    [Export] private int _speed = 150;
    [Export] private int _health = 3;
    private PathFollow2D _follow;
    public Player Target { get; set; }
    public override void _Ready()
    {
        var kek = (int)GD.Randi() % 3 + 1;
        GetNode<Sprite>("Sprite").Frame = kek;
        GD.Print(kek);
        var path = GetNode<Node>("EnemyPaths").GetChildren()[(int)GD.Randi() % GetNode<Node>("EnemyPaths").GetChildCount()] as Node;
        _follow = new PathFollow2D();
        path.AddChild(_follow);
        _follow.Loop = false;
    }
    public override void _Process(float delta)
    {
        _follow.Offset += _speed * delta;
        Position = _follow.GlobalPosition;
        if (_follow.UnitOffset >= 1)
        {
            QueueFree();
        }
    }
    private void _on_AnimationPlayer_animation_finished(string name)
    {
        QueueFree();
    }

    private void _on_GunTimer_timeout()
    {

    }
}
