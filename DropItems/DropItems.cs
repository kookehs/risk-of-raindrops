namespace DropItems
{
	using BepInEx;
	using RoR2;
	using RoR2.UI;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.SceneManagement;
	using UnityEngine.Networking;

	[BepInPlugin("com.kookehs.dropitems", "DropItems", "1.0")]

	public class DropItems : BaseUnityPlugin
	{
		internal static List<HUD> CachedHudInstancesList { get; set; }

		private void Awake()
		{
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			Debug.Log("Loaded DropItemsMod");
		}

		private void OnSceneUnloaded(Scene current)
		{
			CachedHudInstancesList = null;
		}

		private void Update()
		{
			if (!NetworkServer.active) return;

			if (CachedHudInstancesList == null)
			{
				CachedHudInstancesList = (List<HUD>)typeof(HUD).GetField("instancesList", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			}

			foreach (HUD hud in CachedHudInstancesList)
			{
				if (hud == null || hud.itemInventoryDisplay == null) continue;
				FieldInfo fieldInfo = typeof(ItemInventoryDisplay).GetField("itemIcons", BindingFlags.NonPublic | BindingFlags.Instance);
				if (fieldInfo == null) continue;
				List<ItemIcon> itemIcons = (List<ItemIcon>)fieldInfo.GetValue(hud.itemInventoryDisplay);

				foreach (ItemIcon itemIcon in itemIcons)
				{
					if (itemIcon != null && itemIcon.GetComponent<DropItemController>() == null)
					{
						DropItemController dropItemController = itemIcon.transform.gameObject.AddComponent<DropItemController>();
						ItemIndex itemIndex = (ItemIndex)typeof(ItemIcon).GetField("itemIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(itemIcon);
						dropItemController.ItemIndex = itemIndex;
						Inventory inventory = (Inventory)typeof(ItemInventoryDisplay).GetField("inventory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(hud.itemInventoryDisplay);
						dropItemController.Inventory = inventory;
					}
				}

				foreach (EquipmentIcon equipmentIcon in hud.equipmentIcons)
				{
					if (equipmentIcon != null && equipmentIcon.GetComponent<DropItemController>() == null)
					{
						if (equipmentIcon.targetEquipmentSlot == null || equipmentIcon.targetEquipmentSlot.equipmentIndex == EquipmentIndex.None) continue;
						DropItemController dropItemController = equipmentIcon.transform.gameObject.AddComponent<DropItemController>();
						dropItemController.EquipmentIndex = equipmentIcon.targetEquipmentSlot.equipmentIndex;
						dropItemController.Inventory = equipmentIcon.targetInventory;
					}
				}
			}
		}
	}

	public class DropItemController : MonoBehaviour, IPointerClickHandler
	{
		public EquipmentIndex EquipmentIndex { get; set; } = EquipmentIndex.None;
		public ItemIndex ItemIndex { get; set; } = ItemIndex.None;
		public Inventory Inventory { get; set; } = null;

		public void OnPointerClick(PointerEventData eventData)
		{
			// TODO(kookehs): Add multiplayer support.
			if (!NetworkServer.active || Inventory == null) return;

			int[] itemStacks = (int[])typeof(Inventory).GetField("itemStacks", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Inventory);

			if (itemStacks[(int)ItemIndex] <= 0)
			{
				Destroy(this);
				return;
			}

			CharacterBody characterBody = Inventory.GetComponent<CharacterMaster>().GetBody();
			Notification notification = characterBody.gameObject.AddComponent<Notification>();
			notification.transform.SetParent(characterBody.transform);
			notification.SetPosition(new Vector3((float)(Screen.width * 0.8), (float)(Screen.height * 0.25), 0));
			Transform transform = characterBody.transform;

			if (EquipmentIndex != EquipmentIndex.None)
			{
				EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(EquipmentIndex);
				notification.SetIcon(Resources.Load<Texture>(equipmentDef.pickupIconPath));
				notification.GetTitle = () => "Equipment dropped";
				notification.GetDescription = () => $"{Language.GetString(equipmentDef.nameToken)}";
				Inventory.SetEquipmentIndex(EquipmentIndex.None);
				PickupDropletController.CreatePickupDroplet(new PickupIndex(ItemIndex), transform.position, Vector3.up * 20f + transform.forward * 10f);
				return;
			}

			ItemDef itemDef = ItemCatalog.GetItemDef(ItemIndex);
			notification.SetIcon(Resources.Load<Texture>(itemDef.pickupIconPath));
			notification.GetTitle = () => "Item dropped";
			notification.GetDescription = () => $"{Language.GetString(itemDef.nameToken)}";
			Inventory.RemoveItem(ItemIndex, 1);
			PickupDropletController.CreatePickupDroplet(new PickupIndex(ItemIndex), transform.position, Vector3.up * 20f + transform.forward * 10f);
		}
	}
}
