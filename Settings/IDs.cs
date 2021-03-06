﻿using System;
using Zedarus.ToolKit.Events;

namespace Zedarus.ToolKit.Settings
{
	public class IDs
	{
		public const int EVENT_OFFSET = 3000;

		public struct Events
		{
			public const int DisableMusicDuringAd = EVENT_OFFSET + 1;
			public const int EnableMusicAfterAd = EVENT_OFFSET + 2;
			public const int AdsDisabled = EVENT_OFFSET + 3;
			public const int RemoteDataReceived = EVENT_OFFSET + 4;
			public const int AchievementUnlocked = EVENT_OFFSET + 5;
			public const int AchievementRestored = EVENT_OFFSET + 6;
			public const int AudioStateUpdated = EVENT_OFFSET + 7;
			public const int SetLanguage = EVENT_OFFSET + 8;
			public const int CloudSyncFinished = EVENT_OFFSET + 9;
			public const int DisplayAdPlacement = EVENT_OFFSET + 10;
		}

		private static bool _initialized = false;
		public static void Init()
		{
			if (!_initialized)
			{
				EventManager.RegisterEvent(Events.DisableMusicDuringAd);
				EventManager.RegisterEvent(Events.EnableMusicAfterAd);
				EventManager.RegisterEvent(Events.AdsDisabled);
				EventManager.RegisterEvent(Events.RemoteDataReceived);
				EventManager.RegisterEvent(Events.AchievementUnlocked);
				EventManager.RegisterEvent(Events.AchievementRestored);
				EventManager.RegisterEvent(Events.AudioStateUpdated);
				EventManager.RegisterEvent(Events.SetLanguage);
				EventManager.RegisterEvent(Events.CloudSyncFinished);
				EventManager.RegisterEvent(Events.DisplayAdPlacement);
				_initialized = true;
			}
		}
	}
}

