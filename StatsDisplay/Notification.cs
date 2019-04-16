namespace StatsDisplay
{
	using RoR2;
	using RoR2.UI;
	using System;
	using System.Reflection;
	using TMPro;
	using UnityEngine;
	using UnityEngine.UI;

	public class Notification : MonoBehaviour
	{
		public GameObject RootObject { get; set; }
		public GenericNotification GenericNotification { get; set; }
		public Func<string> GetTitle { get; set; }
		public Func<string> GetDescription { get; set; }
		public Transform Parent { get; set; }

		public static void SetFontSize(LanguageTextMeshController languageTextMeshController, int fontSize)
		{
			Text text = (Text)typeof(LanguageTextMeshController).GetField("text", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(languageTextMeshController);
			TextMesh textMesh = (TextMesh)typeof(LanguageTextMeshController).GetField("textMesh", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(languageTextMeshController);
			TextMeshPro textMeshPro = (TextMeshPro)typeof(LanguageTextMeshController).GetField("textMeshPro", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(languageTextMeshController);
			TextMeshProUGUI textMeshProUGui = (TextMeshProUGUI)typeof(LanguageTextMeshController).GetField("textMeshProUGui", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(languageTextMeshController);

			if (text) text.fontSize = fontSize;
			if (textMesh) textMesh.fontSize = fontSize;
			if (textMeshPro) textMeshPro.fontSize = fontSize;
			if (textMeshProUGui) textMeshProUGui.fontSize = fontSize;
		}

		private void Awake()
		{
			// TODO(kookehs): Figure out HUD transform for canvas and scaling
			Parent = RoR2Application.instance.mainCanvas.transform;
			RootObject = Instantiate(Resources.Load<GameObject>("Prefabs/NotificationPanel2"));
			GenericNotification = RootObject.GetComponent<GenericNotification>();
			GenericNotification.transform.SetParent(Parent);
			GenericNotification.iconImage.enabled = false;
		}

		private void Update()
		{
			if (GenericNotification == null)
			{
				Destroy(this);
				return;
			}

			// TODO(kookehs): Cache text and limit updates.
			typeof(LanguageTextMeshController).GetField("resolvedString", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GenericNotification.titleText, GetTitle());
			typeof(LanguageTextMeshController).GetMethod("UpdateLabel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(GenericNotification.titleText, new object[] { });
			typeof(LanguageTextMeshController).GetField("resolvedString", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(GenericNotification.descriptionText, GetDescription());
			typeof(LanguageTextMeshController).GetMethod("UpdateLabel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(GenericNotification.descriptionText, new object[] { });
		}

		private void OnDestroy()
		{
			Destroy(GenericNotification);
			Destroy(RootObject);
		}

		public void SetIcon(Texture texture)
		{
			GenericNotification.iconImage.enabled = true;
			GenericNotification.iconImage.texture = texture;
		}

		public void SetPosition(Vector3 position)
		{
			RootObject.transform.position = position;
		}

		public void SetSize(Vector2 size)
		{
			GenericNotification.GetComponent<RectTransform>().sizeDelta = size;
		}

		public void SetSize(float x, float y)
		{
			// TODO(kookehs): Figure out how to auto-resize rect transform.
			RectTransform rectTransform = GenericNotification.GetComponent<RectTransform>();
			Vector2 size = rectTransform.sizeDelta;
			
			if (!float.IsNaN(x))
			{
				size.x = x;
			}

			if (!float.IsNaN(y))
			{
				size.y = y;
			}

			rectTransform.sizeDelta = size;
		}
	}
}
