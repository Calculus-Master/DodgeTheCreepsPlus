using Godot;
using System;

public class Main : Node
{
	[Export] public PackedScene Mob;
	
	private int _score;
	private bool _scoreMultiplier;
	private bool _gameRunning;
	private bool _powerupActive;
	private bool _powerupExists;
	private bool _eventValid;
	private bool _pauseActive;
	
	private Random _random = new Random();
	
	public override void _Ready()
	{
		Settings.SettingsChanged += UpdateValuesFromSettings;
		Settings.CoinsChanged += UpdateCoins;
		Settings.PowerupCollected += ActivatePowerup;

		UpdateValuesFromSettings();
		
		GetNode<Player>("Player").SetSkin(Player.PlayerSkins.REGULAR);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") && !_gameRunning)
		{
			bool isOpen = GetNode<SettingsMenu>("SettingsMenu").Visible;
			
			GetNode<SettingsMenu>("SettingsMenu").Visible = !isOpen;
			GetNode<HUD>("HUD").SetVisibility(isOpen);
		}
	}

	public override void _ExitTree()
	{
		Settings.SettingsChanged -= UpdateValuesFromSettings;
		Settings.CoinsChanged -= UpdateCoins;
		Settings.PowerupCollected -= ActivatePowerup;
	}

	private void UpdateCoins(int change)
	{
		Player.coins = Mathf.Max(0, Player.coins + change);
	}

	private float RandRange(float min, float max)
	{
		return (float)_random.NextDouble() * (max - min) + min;
	}
	
	public void GameOver()
	{
		GetNode<Timer>("MobTimer").Stop();
		GetNode<Timer>("ScoreTimer").Stop();
		
		GetNode<Timer>("PowerupTimer").Stop();
		GetNode<Powerup>("Powerup").Despawn();
		
		GetNode<HUD>("HUD").ShowGameOver();
		GetTree().CallGroup("mobs", "queue_free");
		
		GetNode<AudioStreamPlayer>("Music").Stop();
		GetNode<AudioStreamPlayer>("DeathSound").Play();

		_gameRunning = false;
	}
	
	public void NewGame()
	{
		_score = 0;
		_scoreMultiplier = false;
		_gameRunning = true;
		_powerupActive = false;
		_powerupExists = false;

		GetNode<SettingsMenu>("SettingsMenu").Visible = false;
		
		var player = GetNode<Player>("Player");
		var startPosition = GetNode<Position2D>("StartPosition");
		
		player.Start(startPosition.Position);
		GetNode<Timer>("StartTimer").Start();
		
		var hud = GetNode<HUD>("HUD");
		hud.UpdateScore(_score);
		hud.ShowMessage("Get Ready!");
		
		GetNode<AudioStreamPlayer>("Music").Play();
	}

	private void UpdateValuesFromSettings()
	{
		//Update volumes
		GetNode<AudioStreamPlayer>("Music").VolumeDb = Settings.Volume;
		GetNode<AudioStreamPlayer>("DeathSound").VolumeDb = Settings.Volume;
	}
	
	private void OnMobTimerTimeout()
	{
		//If the respective Powerup is activated, don't spawn mobs
		if (_pauseActive) return;
		
		// Choose a random location on Path2D.
		var mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");
		mobSpawnLocation.Offset = _random.Next();
		
		// Create a Mob instance and add it to the scene.
		var mobInstance = (RigidBody2D)Mob.Instance();
		AddChild(mobInstance);
		
		// Set the mob's direction perpendicular to the path direction.
		float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;
		
		// Set the mob's position to a random location.
		mobInstance.Position = mobSpawnLocation.Position;
		
		// Add some randomness to the direction.
		direction += RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
		mobInstance.Rotation = direction;
		
		// Choose the velocity.
		int div = 3;
		int mult = 2;
		float min = 150f + ((_score - (_score % div)) * mult);
		float max = 250f + ((_score - (_score % div)) * mult);
		
		//GD.Print("Score: " + _score + ", Min: " + min + ", Max: " + max);
		
		mobInstance.LinearVelocity = new Vector2(RandRange(min, max), 0).Rotated(direction);
	}

	private enum PowerupType
	{
		AVOID_DEATH,
		EXTRA_SCORE,
		PAUSE,
		CLEAR
	}
	
	private void ActivatePowerup()
	{
		_powerupActive = true;
		_powerupExists = false;

		bool needsTimer = true;
		GetNode<Powerup>("Powerup").Despawn();

		var HUD = GetNode<HUD>("HUD");
		PowerupType chosen;

		var r = RandRange(0, 10);
		if (r < 1) chosen = PowerupType.CLEAR;
		else if (r < 3) chosen = PowerupType.AVOID_DEATH;
		else if (r < 5) chosen = PowerupType.PAUSE;
		else chosen = PowerupType.EXTRA_SCORE;

		switch (chosen)
		{
			case PowerupType.AVOID_DEATH:
			{
				GetNode<Player>("Player").DeathSkips++;
				HUD.ShowMessage("Powerup Collected: Skip Death!");
				needsTimer = false;
			} break;
			case PowerupType.EXTRA_SCORE:
			{
				_scoreMultiplier = true;
				HUD.ShowMessage("Powerup Collected: 2x Score!");
				GetNode<Timer>("PowerupTimer").WaitTime = 10;
			} break;
			case PowerupType.PAUSE:
			{
				_pauseActive = true;
				HUD.ShowMessage("Powerup Collected: Mob Spawns Disabled!");
				GetNode<Timer>("PowerupTimer").WaitTime = 2;
			} break;
			case PowerupType.CLEAR:
			{
				GetTree().CallGroup("mobs", "queue_free");
				HUD.ShowMessage("Powerup Collected: Mobs Cleared!");
				needsTimer = false;
			} break;
		}

		if (needsTimer) GetNode<Timer>("PowerupTimer").Start();
		else _powerupActive = false;
	}

	private void OnPowerupTimerTimeout()
	{
		_powerupActive = false;
		
		_scoreMultiplier = false;

		_pauseActive = false;
		
		GetNode<HUD>("HUD").ShowMessage("Powerup Expired!");
		GetNode<Timer>("PowerupTimer").WaitTime = 5;
	}
	
	private void OnScoreTimerTimeout()
	{
		_score = _scoreMultiplier ? _score + 2 : _score + 1;
		Settings.PublishCoinsChangedEvent(_scoreMultiplier ? 2 : 1);

		if(!_powerupExists && !_powerupActive && RandRange(0, 10) < 4)
		{
			ColorRect screen = GetNode<ColorRect>("ColorRect");
			float x = screen.RectSize.x;
			float y = screen.RectSize.y;
		
			GetNode<Powerup>("Powerup").Spawn(new Vector2(RandRange(2, x), RandRange(2, y)));

			_powerupExists = true;
		}
		
		var HUD = GetNode<HUD>("HUD");
		HUD.UpdateScore(_score);

		//Random Events go here
		if(_score % 5 != 0 || !Settings.EventsEnabled) return;
		else if(RandRange(0, 100) < 10)
		{
			HUD.ShowMessage("Mob Spawns Increased!");
			GetNode<Timer>("MobTimer").WaitTime = GetNode<Timer>("MobTimer").WaitTime / 2;
		}
		else if(RandRange(0, 100) < 10)
		{
			HUD.ShowMessage("Mob Spawns Decreased!");
			GetNode<Timer>("MobTimer").WaitTime = GetNode<Timer>("MobTimer").WaitTime * 2;
		}
		else if(RandRange(0, 1000) < 10)
		{
			HUD.ShowMessage("2x Score!");
			_scoreMultiplier = true;
			
			GetNode<Timer>("EventTimer").Start();
		}
		else if(RandRange(0, 100) < 10)
		{
			HUD.ShowMessage("Speed Change!");
			Settings.PlayerSpeed = (int)(Settings.PlayerSpeed * (_random.NextDouble() * 2));
			
			GD.Print("New Speed: " + Settings.PlayerSpeed);
		}
	}
	
	private void OnEventTimerTimeout()
	{
		_scoreMultiplier = false;
	}
	
	private void OnStartTimerTimeout()
	{
		GetNode<Timer>("MobTimer").Start();
		GetNode<Timer>("ScoreTimer").Start();

		_gameRunning = true;
	}
}
