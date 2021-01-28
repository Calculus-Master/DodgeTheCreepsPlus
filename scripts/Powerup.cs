using Godot;
using System;

public class Powerup : Area2D
{
    public override void _Ready()
    {
        Hide();
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
    }

    public void Spawn(Vector2 pos)
    {
        Position = pos;
        Show();
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", false);
    }

    public void Despawn()
    {
        Hide();
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
    }

    public void OnPowerupAreaEntered(int area_id, Area2D area, int area_shape, int self_shape)
    {
        Settings.PublishPowerupCollectedEvent();
    }
}
