using Godot;
using System;

public class Rock : RigidBody2D
{
    [Signal] delegate void Exploded(int size, int radius, Vector2 pos, Vector2 vel);
    private Vector2 _screenSize;
    public int Size { get; private set; }
    private int _radius;
    private float _scaleFactor = 0.2f;

    public void Start(Vector2 pos, Vector2 vel, int size)
    {
        _screenSize = OS.WindowSize;
        GetNode<Sprite>("Explosion").Scale = new Vector2(0.75f, 0.75f) * size;
        Position = pos;
        Size = size;
        Mass = 1.5f * size;
        GetNode<Sprite>("Sprite").Scale = Vector2.One * _scaleFactor * size;
        _radius = (int)(GetNode<Sprite>("Sprite").Texture.GetSize().x / 2 * _scaleFactor * size);
        var shape = new CircleShape2D();
        shape.Radius = _radius;
        GetNode<CollisionShape2D>("CollisionShape2D").Shape = shape;
        LinearVelocity = vel;
        AngularVelocity = (float)GD.RandRange(-1.5, 1.5);
    }

    public void Explode()
    {
        Layers = 0;
        GetNode<Sprite>("Sprite").Hide();
        GetNode<AnimationPlayer>("Explosion/AnimationPlayer").Play("explosion");
        EmitSignal("Exploded", Size, _radius, Position, LinearVelocity);
        LinearVelocity = new Vector2();
        AngularVelocity = 0;
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        var xForm = state.Transform;
        if (xForm.origin.x > _screenSize.x + _radius) xForm.origin.x = 0 - _radius;
        if (xForm.origin.x < 0 - _radius) xForm.origin.x = _screenSize.x + _radius;
        if (xForm.origin.y > _screenSize.y + _radius) xForm.origin.y = 0 - _radius;
        if (xForm.origin.y < 0 - _radius) xForm.origin.y = _screenSize.y + _radius;
        state.Transform = xForm;
    }

    private void _on_AnimationPlayer_animation_finished(string name)
    {
        QueueFree();
    }
}
