namespace StatsDisplay
{
	using BepInEx.Configuration;
	using R2API;
	using RoR2;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using UnityEngine;
	using UnityEngine.Events;

	public static class FrogtownInterface
	{
		private static ConfigFile _config;
		private static StatsDisplay _statsDisplay;
		private static List<string> _availableCharProperties;
		private static List<string> _availableStatProperties;
		private static GUIStyle _alignRight;

		public static void Init(StatsDisplay statsDisplay, ConfigFile config)
		{
			//Soft dependency on mod loader
			_config = config;
			_statsDisplay = statsDisplay;
			Type modDetailsType = null;
			Type frogtownSharedType = null;
			foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (!a.FullName.StartsWith("FrogtownShared,")) continue;
				var allTypes = a.GetTypes();
				foreach (var t in allTypes)
				{
					switch (t.Name)
					{
						case "FrogtownModDetails":
							modDetailsType = t;
							break;
						case "FrogtownShared":
							frogtownSharedType = t;
							break;
					}
				}

				break;
			}

			try
			{
				if (modDetailsType == null || frogtownSharedType == null) return;
				//Will be set back to true by the manager when it initializes
				statsDisplay.Enabled = false;

				var obj = Activator.CreateInstance(modDetailsType, "com.kookehs.statsdisplay");
				obj.SetFieldValue("description",
					"Displays character stats on Info Screen.", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
				obj.SetFieldValue("OnGUI", new UnityAction(() => { OnSettingsGui(); }), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
				obj.SetFieldValue("afterToggle", new UnityAction(() =>
				{
					statsDisplay.Enabled = obj.GetPropertyValue<bool>("enabled", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
				}), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

				var register = frogtownSharedType.GetMethod("RegisterMod", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
				register?.Invoke(null, new[] { obj });

				InitSettings();
			}
			catch (Exception e)
			{
				Debug.Log("Failed to initialize mod manager features");
				Debug.Log(e.Message);
				Debug.Log(e.StackTrace);
			}
		}

		private static void InitSettings()
		{
			//Build list of settings that can be controlled in the UI
			_availableCharProperties = new List<string>();
			var allProps = typeof(CharacterBody).GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (var prop in allProps)
			{
				_availableCharProperties.Add(prop.Name);
			}

			//I'm having trouble getting these with reflection but ideally we would.
			_availableStatProperties = new List<string>()
			{
				"totalGamesPlayed",
				"totalTimeAlive",
				"totalKills",
				"totalDeaths",
				"totalDamageDealt",
				"totalDamageTaken",
				"totalHealthHealed",
				"highestDamageDealt",
				"highestLevel",
				"goldCollected",
				"maxGoldCollected",
				"totalDistanceTraveled",
				"totalItemsCollected",
				"highestItemsCollected",
				"totalStagesCompleted",
				"highestStagesCompleted",
				"totalPurchases",
				"highestPurchases",
				"totalGoldPurchases",
				"highestGoldPurchases",
				"totalBloodPurchases",
				"highestBloodPurchases",
				"totalLunarPurchases",
				"highestLunarPurchases",
				"totalTier1Purchases",
				"highestTier1Purchases",
				"totalTier2Purchases",
				"highestTier2Purchases",
				"totalTier3Purchases",
				"highestTier3Purchases",
				"totalDronesPurchased",
				"totalGreenSoupsPurchased",
				"totalRedSoupsPurchased",
				"suicideHermitCrabsAchievementProgress",
				"firstTeleporterCompleted"
			};
		}

		private static void OnSettingsGui()
		{
			if(_alignRight == null)
			{
				_alignRight = new GUIStyle(GUI.skin.label);
				_alignRight.alignment = TextAnchor.MiddleRight;
			}

			var existingProps = new HashSet<string>();
			var labels = StatsDisplay.CharacterBodyStats.Value.Split(',');
			var names = StatsDisplay.CharacterBodyStatsNames.Value.Split(',');
			int removeProp = -1;

			GUILayout.Label("Character Body Stats");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Property", GUILayout.Width(200));
			GUILayout.Label("Display", GUILayout.Width(200));
			GUILayout.Label("", GUILayout.Width(100)); //move up
			GUILayout.Label("", GUILayout.Width(100)); //move down
			GUILayout.Label("", GUILayout.Width(100)); //remove
			GUILayout.EndHorizontal();

			bool characterChangeMade = false;
			for (int i = 0; i < labels.Length; i++)
			{
				if (string.IsNullOrEmpty(labels[i])) continue;
				GUILayout.BeginHorizontal();
				GUILayout.Label(labels[i], GUILayout.Width(200));
				existingProps.Add(labels[i]);
				var newName = GUILayout.TextField(names[i], GUILayout.Width(200)).Replace(",", "");
				if(newName != names[i])
				{
					names[i] = newName;
					characterChangeMade = true;
				}
				if (i > 0)
				{
					if (GUILayout.Button("Up", GUILayout.Width(75)))
					{
						Swap(names, i, i - 1);
						Swap(labels, i, i - 1);
						characterChangeMade = true;
					}
					GUILayout.Space(21);
				}
				else
				{
					GUILayout.Space(100);
				}
				if (i < labels.Length - 1)
				{
					if (GUILayout.Button("Down", GUILayout.Width(75)))
					{
						Swap(names, i, i + 1);
						Swap(labels, i, i + 1);
						characterChangeMade = true;
					}
					GUILayout.Space(21);
				}
				else
				{
					GUILayout.Space(100);
				}

				if (GUILayout.Button("Remove", GUILayout.Width(75)))
				{
					removeProp = i;
					characterChangeMade = true;
				}

				GUILayout.EndHorizontal();
			}
			GUILayout.Space(16);
			string propToAdd = "";

			GUILayout.Label("Add Field");
			int col = 0;
			foreach (var prop in _availableCharProperties)
			{
				if (existingProps.Contains(prop)) continue;
				if (col == 0) GUILayout.BeginHorizontal();
				var disp = AddSpaces(prop);
				if (GUILayout.Button(new GUIContent(disp, disp), GUILayout.Width(175)))
				{
					propToAdd = prop;
					characterChangeMade = true;
				}

				if (col == 4) GUILayout.EndHorizontal();
				col = (col+1) % 5;
			}
			if (col != 0) GUILayout.EndHorizontal();

			if (characterChangeMade)
			{
				StatsDisplay.CharacterBodyStats.Value = JoinExcept(labels, removeProp);
				StatsDisplay.CharacterBodyStatsNames.Value = JoinExcept(names, removeProp);
				if (!string.IsNullOrEmpty(propToAdd))
				{
					StatsDisplay.CharacterBodyStats.Value += StatsDisplay.CharacterBodyStats.Value?.Length > 0 ? "," : "";
					StatsDisplay.CharacterBodyStats.Value += propToAdd;
					StatsDisplay.CharacterBodyStatsNames.Value += StatsDisplay.CharacterBodyStatsNames.Value?.Length > 0 ? "," : "";
					StatsDisplay.CharacterBodyStatsNames.Value += AddSpaces(propToAdd);
				}
			}
			/////////////////////////////////////////////////////////////
			existingProps.Clear();
			labels = StatsDisplay.StatSheetStats.Value.Split(',');
			names = StatsDisplay.StatSheetStatsNames.Value.Split(',');
			removeProp = -1;

			GUILayout.Label("Stat Sheet Stats");
			GUILayout.BeginHorizontal();
			GUILayout.Label("Property", GUILayout.Width(200));
			GUILayout.Label("Display", GUILayout.Width(200));
			GUILayout.Label("", GUILayout.Width(100)); //move up
			GUILayout.Label("", GUILayout.Width(100)); //move down
			GUILayout.Label("", GUILayout.Width(100)); //remove
			GUILayout.EndHorizontal();

			bool statChangeMade = false;
			for (int i = 0; i < labels.Length; i++)
			{
				if (string.IsNullOrEmpty(labels[i])) continue;
				GUILayout.BeginHorizontal();
				GUILayout.Label(labels[i], GUILayout.Width(200));
				existingProps.Add(labels[i]);
				var newName = GUILayout.TextField(names[i], GUILayout.Width(200)).Replace(",", "");
				if (newName != names[i])
				{
					names[i] = newName;
					statChangeMade = true;
				}
				if (i > 0)
				{
					if (GUILayout.Button("Up", GUILayout.Width(75)))
					{
						Swap(names, i, i - 1);
						Swap(labels, i, i - 1);
						statChangeMade = true;
					}
					GUILayout.Space(21);
				}
				else
				{
					GUILayout.Space(100);
				}
				if (i < labels.Length - 1)
				{
					if (GUILayout.Button("Down", GUILayout.Width(75)))
					{
						Swap(names, i, i + 1);
						Swap(labels, i, i + 1);
						statChangeMade = true;
					}
					GUILayout.Space(21);
				}
				else
				{
					GUILayout.Space(100);
				}

				if (GUILayout.Button("Remove", GUILayout.Width(75)))
				{
					removeProp = i;
					statChangeMade = true;
				}

				GUILayout.EndHorizontal();
			}
			GUILayout.Space(16);

			propToAdd = "";
			GUILayout.Label("Add Field");
			col = 0;
			foreach (var prop in _availableStatProperties)
			{
				if (existingProps.Contains(prop)) continue;
				if (col == 0) GUILayout.BeginHorizontal();
				var disp = AddSpaces(prop);
				if (GUILayout.Button(new GUIContent(disp, disp), GUILayout.Width(175)))
				{
					propToAdd = prop;
					statChangeMade = true;
				}

				if (col == 4) GUILayout.EndHorizontal();
				col = (col + 1) % 5;
			}

			if (col != 0) GUILayout.EndHorizontal();
			col = 0;

			if (statChangeMade)
			{
				StatsDisplay.StatSheetStats.Value = JoinExcept(labels, removeProp);
				StatsDisplay.StatSheetStatsNames.Value = JoinExcept(names, removeProp);
				if (!string.IsNullOrEmpty(propToAdd))
				{
					StatsDisplay.StatSheetStats.Value += StatsDisplay.StatSheetStats.Value?.Length > 0 ? "," : "";
					StatsDisplay.StatSheetStats.Value += propToAdd;
					StatsDisplay.StatSheetStatsNames.Value += StatsDisplay.StatSheetStatsNames.Value?.Length > 0 ? "," : "";
					StatsDisplay.StatSheetStatsNames.Value += AddSpaces(propToAdd);
				}
			}

			bool interfaceChange = false;
			GUILayout.Label("Interface");

			if (Slider(StatsDisplay.TitleFontSize, 10, 36)) interfaceChange = true;
			if (Slider(StatsDisplay.DescriptionFontSize, 10, 36)) interfaceChange = true;
			if (Slider(StatsDisplay.X, 1, 100)) interfaceChange = true;
			if (Slider(StatsDisplay.Y, 1, 100)) interfaceChange = true;
			if (Slider(StatsDisplay.Width, 100, 500)) interfaceChange = true;
			if (Slider(StatsDisplay.Height, 100, 500)) interfaceChange = true;

			var newPersist = GUILayout.Toggle(StatsDisplay.Persistent.Value, new GUIContent(StatsDisplay.Persistent.Definition.Key, StatsDisplay.Persistent.Definition.Description));
			if(newPersist != StatsDisplay.Persistent.Value)
			{
				StatsDisplay.Persistent.Value = newPersist;
				interfaceChange = true;
			}

			if (characterChangeMade || statChangeMade || interfaceChange)
			{
				_config.Save();
				_statsDisplay.ReloadFromConfig();
			}
		}

		private static bool Slider(ConfigWrapper<int> config, int min, int max)
		{
			bool change = false;
			GUILayout.BeginHorizontal();
			GUILayout.Label(new GUIContent(config.Definition.Key + ":  " + config.Value, config.Definition.Description), GUILayout.Width(225));
			GUILayout.Label(min.ToString(), GUILayout.Width(25));
			var newVal = (int)Mathf.Round(GUILayout.HorizontalSlider(config.Value, min, max, GUILayout.Width(300)));
			GUILayout.Label(max.ToString(), GUILayout.Width(25));
			if (newVal != config.Value)
			{
				config.Value = newVal;
				change = true;
			}
			GUILayout.EndHorizontal();
			return change;
		}

		private static string JoinExcept(string[] strs, int except)
		{
			string result = "";
			for(int i = 0; i < strs.Length; i++)
			{
				if(i == except)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(result))
				{
					result += ",";
				}
				result += strs[i];
			}
			return result;
		}

		private static string AddSpaces(string label)
		{
			label = Regex.Replace(label, "([A-Z])", " $1");
			label = label[0].ToString().ToUpper() + label.Substring(1, label.Length - 1);
			return label;
		}

		private static void Swap(object[] arr, int i1, int i2)
		{
			object tmp = arr[i1];
			arr[i1] = arr[i2];
			arr[i2] = tmp;
		}
	}
}