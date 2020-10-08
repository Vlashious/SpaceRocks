using Godot;
using System;

public class HUD : CanvasLayer
{
    [Signal] delegate void StartGame();
    private TextureRect[] _livesCounter;
    public override void _Ready()
    {
        _livesCounter = new TextureRect[]
        {
            GetNode<TextureRect>("MarginContainer/HBoxContainer/LivesCounter/L1"),
            GetNode<TextureRect>("MarginContainer/HBoxContainer/LivesCounter/L2"),
            GetNode<TextureRect>("MarginContainer/HBoxContainer/LivesCounter/L3")
        };
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
