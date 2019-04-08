namespace StatsDisplay
{
	using RoR2;
	using RoR2.UI;
	using SeikoML;
	using System.Text;
	using UnityEngine;

	public class StatsDisplayMod : IKookehsMod
	{
		public void OnStart()
		{
			Debug.Log("Loaded StatsDisplayMod");
		}

		public void OnUpdate()
		{
			if (Api.GetRun() == null) return;
			HUD hud = Object.FindObjectOfType<HUD>();

			if (hud.GetComponent<StatsDisplayHandler>() == null)
			{
				StatsDisplayHandler statsDisplayHandler = hud.gameObject.AddComponent<StatsDisplayHandler>();
				statsDisplayHandler.CharacterBody = hud.localUserViewer.cachedBody;
			}
		}
	}

	public class StatsDisplayHandler : MonoBehaviour
	{
		public Api.Notification StatsDisplay { get; set; }
		public CharacterBody CharacterBody { get; set; }

		private void Awake()
		{
			if (StatsDisplay == null)
			{
				CharacterMaster characterMaster = FindObjectOfType<CharacterMaster>();
				StatsDisplay = characterMaster.gameObject.AddComponent<Api.Notification>(); ;
				StatsDisplay.transform.SetParent(characterMaster.GetBody().transform);
				StatsDisplay.SetPosition(new Vector3((float)(Screen.width * 0.25), (float)(Screen.height * 0.25), 0));
				StatsDisplay.GetTitle = () => "STATS";
				StatsDisplay.GetDescription = GetCharacterStats;
				StatsDisplay.GenericNotification.fadeTime = 1f;
				StatsDisplay.GenericNotification.duration = 1800f;
			}
		}

		public string GetCharacterStats()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Damage: {CharacterBody.damage}\t\tCrit %: {CharacterBody.crit}");
			sb.AppendLine($"Attack Speed: {CharacterBody.attackSpeed}\tMove Speed: {CharacterBody.moveSpeed}");
			sb.AppendLine($"Acceleration: {CharacterBody.acceleration}\tJump Count: {CharacterBody.maxJumpCount}");
			return sb.ToString();
		}
	}
}
