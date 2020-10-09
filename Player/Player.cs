using Godot;
using System;

public class Player : RigidBody2D
{
    [Signal] delegate void ShootSignal(PackedScene scene, Vector2 pos, float rot);
    [Signal] delegate void LivesChanged(int val);
    [Signal] delegate void Dead();
    public enum State
    {
        INIT,
        ALIVE,
        INV,
        DEAD
    }
    private State _state;
    private CollisionShape2D _collision;
    private Sprite _sprite;
    [Export]
    private int _enginePower;
    [Export]
    private int _spinPower;
    [Export]
    private PackedScene _bullet;
    [Export]
    private float _fireRate;
    private Vector2 _thrust;
    private float _rotationDir;
    private Vector2 _screenSize;
    private bool _canShoot;
    private int _lives;
    public int Lives
    {
        private get
        {
            return _lives;
        }
        set
        {
            _lives = value;
            EmitSignal("LivesChanged", _lives);
        }
    }

    public override void _Ready()
    {
        _collision = GetNode<CollisionShape2D>("CollisionShape2D");
        _sprite = GetNode<Sprite>("Sprite");
        _screenSize = GetViewport().GetVisibleRect().Size;
        GetNode<Timer>("GunTimer").WaitTime = _fireRate;
        GetNode<AnimationPlayer>("Explosion/AnimationPlayer").Connect("animation_finished", this, "OnAnimFinished");
        ChangeState(State.INIT);
    }
    private void OnAnimFinished(string animName)
    {
        GetNode<Sprite>("Explosion").Hide();
    }
    public void Start()
    {
        GetNode<Sprite>("Sprite").Show();
        Lives = 3;
        ChangeState(State.INV);
    }
    public void ChangeState(State newState)
    {
        switch (newState)
        {
            case State.INIT:
                _collision.SetDeferred("disabled", true);
                _sprite.Modulate = new Color(_sprite.Modulate, 0.5f);
                break;
            case State.INV:
                _collision.SetDeferred("disabled", true);
                _sprite.Modulate = new Color(_sprite.Modulate, 0.5f);
                GetNode<Timer>("InvulnerabilityTimer").Start();
                break;
            case State.DEAD:
                _collision.SetDeferred("disabled", true);
                _sprite.Hide();
                LinearVelocity = new Vector2();
                EmitSignal("Dead");
                break;
            case State.ALIVE:
                _collision.SetDeferred("disabled", false);
                _sprite.Modulate = new Color(_sprite.Modulate, 1f);
                break;
        }
        _state = newState;
    }
    public override void _Process(float delta)
    {
        GetInput();
    }

    private void GetInput()
    {
        _thrust = new Vector2();
        if (_state == State.DEAD || _state == State.INIT) return;
        if (Input.IsActionPressed("thrust"))
        {
            _thrust = new Vector2(_enginePower, 0);
            if (!GetNode<AudioStreamPlayer2D>("EngineSound").Playing)
            {
                GetNode<AudioStreamPlayer2D>("EngineSound").Play();
            }
        }
        else GetNode<AudioStreamPlayer2D>("EngineSound").Stop();
        _rotationDir = 0;
        if (Input.IsActionPressed("rotate_right"))
        {
            _rotationDir += 1;
        }
        if (Input.IsActionPressed("rotate_left"))
        {
            _rotationDir -= 1;
        }
        if (Input.IsActionPressed("shoot") && _canShoot)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (_state == State.INV) return;
        EmitSignal("ShootSignal", _bullet, GetNode<Position2D>("Muzzle").GlobalPosition, Rotation);
        _canShoot = false;
        GetNode<Timer>("GunTimer").Start();
        GetNode<AudioStreamPlayer2D>("LaserSound").Play();
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        AppliedForce = _thrust.Rotated(Rotation);
        AppliedTorque = _spinPower * _rotationDir;
        var xForm = state.Transform;
        if (xForm.origin.x > _screenSize.x) xForm.origin.x = 0;
        if (xForm.origin.x < 0) xForm.origin.x = _screenSize.x;
        if (xForm.origin.y > _screenSize.y) xForm.origin.y = 0;
        if (xForm.origin.y < 0) xForm.origin.y = _screenSize.y;
        state.Transform = xForm;
    }

    private void _on_GunTimer_timeout()
    {
        _canShoot = true;
    }

    private void _on_InvulnerabilityTimer_timeout()
    {
        ChangeState(State.ALIVE);
    }
    private void _on_Player_body_entered(Node body)
    {
        if (body is Rock r)
        {
            r.Explode();
            GetNode<Sprite>("Explosion").Show();
            GetNode<AnimationPlayer>("Explosion/AnimationPlayer").Play("explosion");
            _lives -= 1;
            if (_lives <= 0)
            {
                ChangeState(State.DEAD);
            }
            else ChangeState(State.INV);
        }
    }
}
