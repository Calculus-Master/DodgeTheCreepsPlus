using Godot;
using System;

public class SettingsMenu : ColorRect
{
	public void OnVolumeSliderChanged(float volume)
	{
		Settings.Volume = 0 - (int)(100 - volume);
		Settings.PublishSettingsChangedEvent();
	}

	public void OnPlayerSpeedSliderChanged(float playerSpeed)
	{
		Settings.PlayerSpeed = 400 + (int) (15 * (playerSpeed - 50));
		Settings.PublishSettingsChangedEvent();
	}

	public void OnEventsEnabledCheckButtonChanged(bool enabled)
	{
		Settings.EventsEnabled = enabled;
		Settings.PublishSettingsChangedEvent();
	}
}
