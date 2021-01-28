using Godot;
using System;

public class Settings : Node
{
	//Emitted whenever a setting changes
	public static event Action SettingsChanged;
	
	//Emitted when the player gets coins
	public static event Action<int> CoinsChanged;
	
	//Emitted when a powerup is collected
	public static event Action PowerupCollected;
	
	public static void PublishSettingsChangedEvent()
	{
		SettingsChanged?.Invoke();
	}

	public static void PublishCoinsChangedEvent(int change)
	{
		CoinsChanged?.Invoke(change);
	}

	public static void PublishPowerupCollectedEvent()
	{
		PowerupCollected?.Invoke();
	}
	
	//Setting Values
	public static int PlayerSpeed = 400;

	public static int Volume = 0;

	public static bool EventsEnabled = true;
}
