using Godot;
using System;

public class Enemy : Area2D
{
    [Signal] delegate void ShootSignal(PackedScene scene, Vector2 globPos, float dir);
    [Export] private readonly PackedScene _bullet;
    [Export] private int _speed = 150;
    [Export] private int _health = 3;
    private PathFollow2D _follow;
    public Player Target { get; set; }
    public override void _Ready()
    {
        var kek = 1;
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
        ShootPulse(3, 0.15f);
    }
    public async void TakeDamage(int amount)
    {
        _health -= amount;
        GetNode<AnimationPlayer>("AnimationPlayer").Play("flash");
        if (_health <= 0)
        {
            Explode();
        }
        await ToSignal(GetNode<AnimationPlayer>("AnimationPlayer"), "animation_finished");
        GetNode<AnimationPlayer>("AnimationPlayer").Play("rotation");
    }

    private void Explode()
    {
        GetNode<AudioStreamPlayer2D>("ExplodeSound").Play();
        _speed = 0;
        GetNode<Timer>("GunTimer").Stop();
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        GetNode<Sprite>("Sprite").Hide();
        GetNode<Sprite>("Explosion").Show();
        GetNode<AnimationPlayer>("Explosion/AnimationPlayer").Play("explosion");
    }

    private async void ShootPulse(int n, float delay)
    {
        for (int i = 0; i < n; i++)
        {
            ShootBullet();
            await ToSignal(GetTree().CreateTimer(delay), "timeout");
        }
    }
    private void ShootBullet()
    {
        var dir = Target.GlobalPosition - GlobalPosition;
        dir = dir.Rotated((float)GD.RandRange(-0.1, 0.1));
        EmitSignal("ShootSignal", _bullet, GlobalPosition, dir.Angle());
        GetNode<AudioStreamPlayer2D>("ShootSound").Play();
    }
    private void _on_Enemy_body_entered(Node body)
    {
        if (body is Player p)
        {
            p.Shields -= 50;
        }
        Explode();
    }
}
