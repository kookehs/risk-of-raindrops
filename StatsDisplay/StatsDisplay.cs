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

	[BepInPlugin("com.kookehs.statsdisplay", "StatsDisplay", "1.2")]

	public class StatsDisplay : BaseUnityPlugin
	{
		public static ConfigWrapper<string> Title { get; private set; }
		public static ConfigWrapper<string> StatsToDisplay { get; private set; }
		public static ConfigWrapper<string> StatsToDisplayNames { get; private set; }
		public static ConfigWrapper<int> TitleFontSize { get; private set; }
		public static ConfigWrapper<int> DescriptionFontSize { get; private set; }
		public static ConfigWrapper<int> X { get; private set; }
		public static ConfigWrapper<int> Y { get; private set; }
		public static ConfigWrapper<int> Width { get; private set; }
		public static ConfigWrapper<int> Height { get; private set; }
		public static ConfigWrapper<bool> Persistent { get; private set; }

		public Notification Notification { get; set; }
		public CharacterBody CachedCharacterBody { get; set; }
		public string[] CachedStatsToDisplay { get; set; }
		public string[] CachedStatsToDisplayNames { get; set; }

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

			const int defaultTitleFontSize = 18;
			TitleFontSize = Config.Wrap("Display", "TitleFontSize", "The font size of the title.", defaultTitleFontSize);

			const int defaultDescriptionFontSize = 14;
			DescriptionFontSize = Config.Wrap("Display", "DescriptionFontSize", "The font size of the description.", defaultDescriptionFontSize);

			const int defaultX = 10;
			X = Config.Wrap("Display", "X", "The X position as percent of screen width of the stats display.", defaultX);

			const int defaultY = 35;
			Y = Config.Wrap("Display", "Y", "The Y position as percent of screen height of the stats display.", defaultY);

			const int defaultWidth = 250;
			Width = Config.Wrap("Display", "Width", "The width of the stats display.", defaultWidth);

			const int defaultHeight = 250;
			Height = Config.Wrap("Display", "Height", "The height of the stats display.", defaultHeight);

			const bool defaultPersistent = false;
			Persistent = Config.Wrap("Display", "Persistent", "Whether the stats display always shows or only on Info Screen.", defaultPersistent);
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
				Notification.SetPosition(new Vector3((float)(Screen.width * X.Value / 100f), (float)(Screen.height * Y.Value / 100f), 0));
				Notification.GetTitle = () => Title.Value;
				Notification.GetDescription = GetCharacterStats;
				Notification.GenericNotification.fadeTime = 1f;
				Notification.GenericNotification.duration = 86400f;
				Notification.SetSize(Width.Value, Height.Value);
				Notification.SetFontSize(Notification.GenericNotification.titleText, TitleFontSize.Value);
				Notification.SetFontSize(Notification.GenericNotification.descriptionText, DescriptionFontSize.Value);
			}

			if (CachedCharacterBody == null && Notification != null)
			{
				Destroy(Notification);
			}

			if (Notification != null && Notification.RootObject != null)
			{
				if (Persistent.Value || (localUser != null && localUser.inputPlayer != null && localUser.inputPlayer.GetButton("info")))
				{
					Notification.RootObject.SetActive(true);
					return;
				}

				Notification.RootObject.SetActive(false);
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
