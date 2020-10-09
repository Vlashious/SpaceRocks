using Godot;
using System;

public class EnemyBullet : Area2D
{
    [Export] private int _speed;
    private Vector2 _vel;
    public void Start(Vector2 pos, float dir)
    {
        Position = pos;
        _vel = new Vector2(_speed, 0).Rotated(dir);
        Rotation = dir;
    }
    public override void _Process(float delta)
    {
        Position += _vel * delta;
    }
    private void _on_EnemyBullet_area_entered(Area2D area)
    {
        if (area is Enemy) return;
        QueueFree();
    }
    private void _on_VisibilityNotifier2D_screen_exited()
    {
        QueueFree();
    }
    private void _on_EnemyBullet_body_entered(Node body)
    {
        if (body is Player player)
        {
            player.Shields -= 15;
        }
        QueueFree();
    }
}
