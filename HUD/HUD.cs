using Godot;
using System;

public class HUD : CanvasLayer
{
    [Signal] delegate void StartGame();
    private TextureProgress _shieldBar;
    private Texture _redBar;
    private Texture _yellowBar;
    private Texture _greenBar;
    private TextureRect[] _livesCounter;
    public override void _Ready()
    {
        _shieldBar = GetNode<TextureProgress>("MarginContainer/HBoxContainer/ShieldBar");
        _redBar = GD.Load("res://assets/barHorizontal_red_mid 200.png") as Texture;
        _yellowBar = GD.Load("res://assets/barHorizontal_yellow_mid 200.png") as Texture;
        _greenBar = GD.Load("res://assets/barHorizontal_green_mid 200.png") as Texture;
        _livesCounter = new TextureRect[]
        {
            GetNode<TextureRect>("MarginContainer/HBoxContainer/LivesCounter/L1"),
            GetNode<TextureRect>("MarginContainer/HBoxContainer/LivesCounter/L2"),
            GetNode<TextureRect>("MarginContainer/HBoxContainer/LivesCounter/L3")
        };
    }
    private void UpdateShield(float value)
    {
        _shieldBar.Value = value;
        _shieldBar.TextureProgress_ = _yellowBar;
        if (value < 40)
        {
            _shieldBar.TextureProgress_ = _redBar;
        }
        else if (value < 70)
        {
            _shieldBar.TextureProgress_ = _greenBar;
        }
    }

    public void ShowMessage(string message)
    {
        var label = GetNode<Label>("MessageLabel");
        label.Text = message;
        label.Show();
        GetNode<Timer>("MessageTimer").Start();
    }

    public void UpdateScore(int score)
    {
        GetNode<Label>("MarginContainer/HBoxContainer/ScoreLabel").Text = score.ToString();
    }

    public void UpdateLives(int value)
    {
        GD.Print(value);
        for (int i = 0; i < 3; i++)
        {
            _livesCounter[i].Visible = value > i;
        }
    }

    public async void GameOver()
    {
        ShowMessage("Game Over");
        await ToSignal(GetNode<Timer>("MessageTimer"), "timeout");
        GetNode<TextureButton>("StartButton").Show();
    }

    private void _on_StartButton_pressed()
    {
        GetNode<TextureButton>("StartButton").Hide();
        EmitSignal("StartGame");
    }

    private void _on_MessageTimer_timeout()
    {
        GetNode<Label>("MessageLabel").Hide();
    }
}
