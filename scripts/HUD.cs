using Godot;
using System;

public class HUD : CanvasLayer
{
	[Signal]
	public delegate void StartGame();
	
	public void ShowMessage(String text)
	{
		var message = GetNode<Label>("Message");
		message.Text = text;
		message.Show();
		
		GetNode<Timer>("MessageTimer").Start();
	}
	
	async public void ShowGameOver()
	{
		ShowMessage("Game Over");
		var messageTimer = GetNode<Timer>("MessageTimer");
		await ToSignal(messageTimer, "timeout");
		
		var message = GetNode<Label>("Message");
		message.Text = "Dodge the\nCreeps!";
		message.Show();
		
		await ToSignal(GetTree().CreateTimer(1), "timeout");
		GetNode<Button>("StartButton").Show();
	}

	public void SetVisibility(bool visible)
	{
		GetNode<Label>("ScoreLabel").Visible = visible;
		GetNode<Label>("Message").Visible = visible;
		GetNode<Button>("StartButton").Visible = visible;
	}
	
	public void UpdateScore(int score)
	{
		GetNode<Label>("ScoreLabel").Text = score.ToString();
	}
	
	private void OnMessageTimerTimeout()
	{
		GetNode<Label>("Message").Hide();
	}
	
	private void OnStartButtonPressed()
	{
		GetNode<Button>("StartButton").Hide();
		EmitSignal("StartGame");
	}
}



