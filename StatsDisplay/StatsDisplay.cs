namespace StatsDisplay
{
	using RoR2;
	using SeikoML;
	using System.Text;
	using UnityEngine;
	using UnityEngine.SceneManagement;

	public class StatsDisplayMod : ISeikoMod
	{
		public static GameObject RootObject { get; set; }
		public static StatsDisplayHandler StatsDisplayHandler { get; set; }

		public void OnStart()
		{
			RootObject = new GameObject("StatsDisplayMod");
			Object.DontDestroyOnLoad(RootObject);
			StatsDisplayHandler = RootObject.AddComponent<StatsDisplayHandler>();
			Debug.Log("Loaded StatsDisplayMod");
		}
	}

	public class StatsDisplayHandler : MonoBehaviour
	{
		public Api.Notification StatsDisplay { get; set; }
		public CharacterBody CharacterBody { get; set; }

		private void Start()
		{
			SceneManager.sceneUnloaded += OnSceneUnloaded;
		}

		private void Update()
		{
			LocalUser localUser = LocalUserManager.GetFirstLocalUser();

			if (CharacterBody == null && localUser != null)
			{
				CharacterBody = localUser.cachedBody;
			}

			if (StatsDisplay == null && CharacterBody != null)
			{
				StatsDisplay = CharacterBody.gameObject.AddComponent<Api.Notification>();
				StatsDisplay.transform.SetParent(CharacterBody.transform);
				StatsDisplay.SetPosition(new Vector3((float)(Screen.width * 0.25), (float)(Screen.height * 0.25), 0));
				StatsDisplay.GetTitle = () => "STATS";
				StatsDisplay.GetDescription = GetCharacterStats;
				StatsDisplay.GenericNotification.fadeTime = 1f;
				StatsDisplay.GenericNotification.duration = 1800f;
			}

			if (CharacterBody == null && StatsDisplay != null)
			{
				Destroy(StatsDisplay.RootObject);
				Destroy(StatsDisplay.GenericNotification);
				Destroy(StatsDisplay);
			}

			if (localUser != null && localUser.inputPlayer != null && localUser.inputPlayer.GetButton("info"))
			{
				if (StatsDisplay != null && StatsDisplay.RootObject != null)
				{
					StatsDisplay.RootObject.SetActive(true);
				}
			}
			else
			{
				if (StatsDisplay != null && StatsDisplay.RootObject != null)
				{
					StatsDisplay.RootObject.SetActive(false);
				}
			}
		}

		private void OnSceneUnloaded(Scene scene)
		{
			CharacterBody = null;

			if (StatsDisplay != null)
			{
				Destroy(StatsDisplay.RootObject);
				Destroy(StatsDisplay.GenericNotification);
				Destroy(StatsDisplay);
			}
		}

		public string GetCharacterStats()
		{
			if (CharacterBody == null) return string.Empty;
			// TODO(kookehs): Do the math for alignment.
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Damage: {CharacterBody.damage}\t\tCrit: {CharacterBody.crit}%");
			sb.AppendLine($"Attack Speed: {CharacterBody.attackSpeed}\t\tRegen: {CharacterBody.regen}");
			sb.AppendLine($"Move Speed: {CharacterBody.moveSpeed}\t\tJump Count: {CharacterBody.maxJumpCount}");
			return sb.ToString();
		}
	}
}
