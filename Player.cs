using Godot;
using System;

public class Player : Area2D
{
	[Signal] public delegate void Hit();

	public static int coins;
	
	[Export] public int DeathSkips;
	
	private Vector2 _screenSize;
	
	public override void _Ready()
	{
		_screenSize = GetViewport().Size;
		
		Hide();
	}
	
	public override void _Process(float delta)
	{
		var velocity = new Vector2();
		
		if(Input.IsActionPressed("ui_right"))
		{
			velocity.x += 1;
		}
		
		if(Input.IsActionPressed("ui_left"))
		{
			velocity.x -= 1;
		}
		
		if(Input.IsActionPressed("ui_down"))
		{
			velocity.y += 1;
		}
		
		if(Input.IsActionPressed("ui_up"))
		{
			velocity.y -= 1;
		}
		
		var animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		
		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * Settings.PlayerSpeed;
			animatedSprite.Play();
		}
		else
		{
			animatedSprite.Stop();
		}
		
		Position += velocity * delta;
		Position = new Vector2
		(
			x: Mathf.Clamp(Position.x, 0, _screenSize.x),
			y: Mathf.Clamp(Position.y, 0, _screenSize.y)
		);
		
		if(velocity.x != 0)
		{
			animatedSprite.Animation = "walk";
			animatedSprite.FlipV = false;
			animatedSprite.FlipH = velocity.x < 0;
		}
		else if(velocity.y != 0)
		{
			animatedSprite.Animation = "up";
			animatedSprite.FlipV = velocity.y > 0;
		}
	}
	
	public void ToggleCollision()
	{
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", !GetNode<CollisionShape2D>("CollisionShape2D").Disabled);
	}
	
	public void OnPlayerBodyEntered(PhysicsBody2D body)
	{
		if(DeathSkips > 0)
		{
			DeathSkips--;
			body.QueueFree();
			return;
		}
		
		Hide();
		EmitSignal("Hit");
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
	}

	public void Start(Vector2 pos)
	{
		Position = pos;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}
}
