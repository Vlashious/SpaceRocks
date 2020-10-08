using Godot;
using System;

public class Bullet : Area2D
{
    [Export]
    private int _speed;
    private Vector2 _velocity;

    public void Start(Vector2 pos, float dir)
    {
        Position = pos;
        Rotation = dir;
        _velocity = new Vector2(_speed, 0).Rotated(dir);
    }

    public override void _Process(float delta)
    {
        Position += _velocity * delta;
    }

    private void _on_VisibilityNotifier2D_screen_exited()
    {
        QueueFree();
    }

    private void _on_Bullet_body_entered(Node body)
    {
        if (body is Rock rock)
        {
            rock.Explode();
            QueueFree();
        }
    }
}
