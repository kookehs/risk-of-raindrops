namespace StatsDisplay
{
	using BepInEx;
	using RoR2;
	using System.Text;
	using UnityEngine;
	using UnityEngine.SceneManagement;

	[BepInPlugin("com.kookehs.statsdisplay", "StatsDisplay", "1.0")]

	public class StatsDisplay : BaseUnityPlugin
	{
		public Notification Notification { get; set; }
		public CharacterBody CachedCharacterBody { get; set; }

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
				Notification.SetPosition(new Vector3((float)(Screen.width * 0.25), (float)(Screen.height * 0.25), 0));
				Notification.GetTitle = () => "STATS";
				Notification.GetDescription = GetCharacterStats;
				Notification.GenericNotification.fadeTime = 1f;
				Notification.GenericNotification.duration = 86400f;
				Notification.SetSize(float.NaN, 150f);
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
			// TODO(kookehs): Use BepInEx config for customization.
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Crit: {CachedCharacterBody.crit}%");
			sb.AppendLine($"Damage: {CachedCharacterBody.damage}");
			sb.AppendLine($"Attack Speed: {CachedCharacterBody.attackSpeed}");
			sb.AppendLine($"Regen: {CachedCharacterBody.regen}");
			sb.AppendLine($"Move Speed: {CachedCharacterBody.moveSpeed}");
			sb.AppendLine($"Jump Count: {CachedCharacterBody.maxJumpCount}");
			return sb.ToString();
		}
	}
}
