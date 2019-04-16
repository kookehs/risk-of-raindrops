namespace StatsDisplay
{
	using BepInEx;
	using BepInEx.Configuration;
	using RoR2;
	using RoR2.Stats;
	using System.Reflection;
	using System.Text;
	using UnityEngine;
	using UnityEngine.SceneManagement;

	[BepInPlugin("com.kookehs.statsdisplay", "StatsDisplay", "1.1")]

	public class StatsDisplay : BaseUnityPlugin
	{
		public static ConfigWrapper<string> Title { get; private set; }
		public static ConfigWrapper<string> StatsToDisplay { get; private set; }
		public static ConfigWrapper<string> StatsToDisplayNames { get; private set; }
		public static ConfigWrapper<string> TitleFontSize { get; private set; }
		public static ConfigWrapper<string> DescriptionFontSize { get; private set; }
		public static ConfigWrapper<string> X { get; private set; }
		public static ConfigWrapper<string> Y { get; private set; }
		public static ConfigWrapper<string> Width { get; private set; }
		public static ConfigWrapper<string> Height { get; private set; }
		public static Rect Rect { get; set; }

		public Notification Notification { get; set; }
		public CharacterBody CachedCharacterBody { get; set; }
		public string[] CachedStatsToDisplay { get; set; }
		public string[] CachedStatsToDisplayNames { get; set; }
		public int CachedTitleFontSize { get; set; }
		public int CachedDescriptionFontSize { get; set; }

		public StatsDisplay()
		{
			// TODO(kookehs): Reload on edit.
			Config.ConfigReloaded += OnConfigReloaded;
			OnConfigReloaded(null, null);
		}

		private void OnConfigReloaded(object sender, System.EventArgs e)
		{
			const string defaultTitle = "STATS";
			Title = Config.Wrap("Display", "Title", "Text to display for the title.", defaultTitle);

			const string defaultStats = "crit,damage,attackSpeed,armor,regen,moveSpeed,maxJumpCount,experience";
			StatsToDisplay = Config.Wrap("Display", "StatsToDisplay", "A comma-separated list of stats to display based on CharacterBody properties.", defaultStats);
			CachedStatsToDisplay = StatsToDisplay.Value.Split(',');

			const string defaultStatsName = "Crit,Damage,Attack Speed,Armor,Regen,Move Speed,Jump Count,Experience";
			StatsToDisplayNames = Config.Wrap("Display", "StatsToDisplayNames", "A comma-separated list of names for the stats.", defaultStatsName);
			CachedStatsToDisplayNames = StatsToDisplayNames.Value.Split(',');

			if (CachedStatsToDisplay.Length != CachedStatsToDisplayNames.Length) Debug.Log("Length of StatsToDisplay and StatsToDisplayNames do not match.");

			const string defaultTitleFontSize = "18";
			TitleFontSize = Config.Wrap("Display", "TitleFontSize", "The font size of the title.", defaultTitleFontSize);
			if (!int.TryParse(TitleFontSize.Value, out int titleFontSize)) Debug.Log("Invalid font size for title");
			CachedTitleFontSize = titleFontSize;

			const string defaultDescriptionFontSize = "14";
			DescriptionFontSize = Config.Wrap("Display", "DescriptionFontSize", "The font size of the description", defaultDescriptionFontSize);
			if (!int.TryParse(DescriptionFontSize.Value, out int descriptionFontSize)) Debug.Log("Invalid font size for description");
			CachedDescriptionFontSize = descriptionFontSize;

			const string defaultX = "10";
			X = Config.Wrap("Display", "X", "The X position as percent of screen width of the stats display.", defaultX);
			if (!float.TryParse(X.Value, out float x)) Debug.Log("Invalid X value");

			const string defaultY = "35";
			Y = Config.Wrap("Display", "Y", "The Y position as percent of screen height of the stats display.", defaultY);
			if (!float.TryParse(Y.Value, out float y)) Debug.Log("Invalid Y value");

			const string defaultWidth = "250";
			Width = Config.Wrap("Display", "Width", "The width of the stats display.", defaultWidth);
			if (!float.TryParse(Width.Value, out float width)) Debug.Log("Invalid Width value");

			const string defaultHeight = "250";
			Height = Config.Wrap("Display", "Height", "The height of the stats display.", defaultHeight);
			if (!float.TryParse(Height.Value, out float height)) Debug.Log("Invalid Height value");

			Rect = new Rect(x, y, width, height);
		}

		private void Awake()
		{
			Debug.Log("Loaded StatsDisplayMod");
		}

		private void Update()
		{
			LocalUser localUser = LocalUserManager.GetFirstLocalUser();

			if (CachedCharacterBody == null && localUser != null)
			{
				CachedCharacterBody = localUser.cachedBody;
			}

			if (Notification == null && CachedCharacterBody != null)
			{
				Notification = CachedCharacterBody.gameObject.AddComponent<Notification>();
				Notification.transform.SetParent(CachedCharacterBody.transform);
				Notification.SetPosition(new Vector3((float)(Screen.width * Rect.x / 100), (float)(Screen.height * Rect.y / 100), 0));
				Notification.GetTitle = () => Title.Value;
				Notification.GetDescription = GetCharacterStats;
				Notification.GenericNotification.fadeTime = 1f;
				Notification.GenericNotification.duration = 86400f;
				Notification.SetSize(Rect.width, Rect.height);
				Notification.SetFontSize(Notification.GenericNotification.titleText, CachedTitleFontSize);
				Notification.SetFontSize(Notification.GenericNotification.descriptionText, CachedDescriptionFontSize);
			}

			if (CachedCharacterBody == null && Notification != null)
			{
				Destroy(Notification);
			}

			if (localUser != null && localUser.inputPlayer != null && localUser.inputPlayer.GetButton("info"))
			{
				if (Notification != null && Notification.RootObject != null)
				{
					Notification.RootObject.SetActive(true);
				}
			}
			else
			{
				if (Notification != null && Notification.RootObject != null)
				{
					Notification.RootObject.SetActive(false);
				}
			}
		}

		private void OnSceneUnloaded(Scene scene)
		{
			CachedCharacterBody = null;

			if (Notification != null)
			{
				Destroy(Notification);
			}
		}

		public string GetCharacterStats()
		{
			if (CachedCharacterBody == null) return string.Empty;
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < CachedStatsToDisplay.Length; i++)
			{
				object stat = typeof(CharacterBody).GetProperty(CachedStatsToDisplay[i],
					BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).GetValue(CachedCharacterBody);
				sb.AppendLine($"{CachedStatsToDisplayNames[i]}: {stat}");
			}

			RunReport runReport = RunReport.Generate(Run.instance, GameResultType.Unknown);

			for (int i = 0; i < runReport.playerInfoCount; i++)
			{
				RunReport.PlayerInfo playerInfo = runReport.GetPlayerInfo(i);

				if (playerInfo.isLocalPlayer)
				{
					sb.AppendLine($"Kills: {playerInfo.statSheet.GetStatValueULong(StatDef.totalKills)}");
					sb.AppendLine($"Damage Dealt: {playerInfo.statSheet.GetStatValueULong(StatDef.totalDamageDealt)}");
					sb.AppendLine($"Gold Collected: {playerInfo.statSheet.GetStatValueULong(StatDef.goldCollected)}");
					sb.AppendLine($"Stages Completed: {playerInfo.statSheet.GetStatValueULong(StatDef.totalStagesCompleted)}");
					break;
				}
			}

			return sb.ToString();
		}
	}
}
