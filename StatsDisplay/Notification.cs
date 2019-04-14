namespace StatsDisplay
{
	using RoR2;
	using RoR2.UI;
	using System;
	using System.Reflection;
	using UnityEngine;

	public class Notification : MonoBehaviour
	{
		public GameObject RootObject { get; set; }
		public GenericNotification GenericNotification { get; set; }
		public Func<string> GetTitle { get; set; }
		public Func<string> GetDescription { get; set; }
		public Transform Parent { get; set; }

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
