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
		Settings.PlayerSpeed = Mathf.Clamp(Settings.PlayerSpeed, Settings.PlayerSpeedLimits[0], Settings.PlayerSpeedLimits[1]);
		Settings.PublishSettingsChangedEvent();
	}

	public void OnEventsEnabledCheckButtonChanged(bool enabled)
	{
		Settings.EventsEnabled = enabled;
		Settings.PublishSettingsChangedEvent();
	}
}
