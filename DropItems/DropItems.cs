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
	using MiniRpcLib.Action;
	using MiniRpcLib;
	using System;
	using BepInEx.Configuration;


	[BepInPlugin("com.kookehs.dropitems", "DropItems", "1.1.2")]
	[BepInDependency(MiniRpcLib.MiniRpcPlugin.Dependency)]
	public class DropItems : BaseUnityPlugin
	{
		internal static List<HUD> CachedHudInstancesList { get; set; }

		IRpcAction<GameObject> SendDropEquipmentRequest { get; set; }
		IRpcAction<Action<NetworkWriter>> SendDropItemRequest { get; set; }

		ConfigWrapper<bool> ClientsCanDrop;
		ConfigWrapper<bool> HostCanDropOthers;
		ConfigWrapper<bool> ClientsCanDropOthers;

		private bool IsAllowedToDrop(NetworkUser user, CharacterMaster master) {
			if (!user.isServer && !ClientsCanDrop.Value)
				return false;
			if (user.master != master) {
				if (user.isServer && !HostCanDropOthers.Value)
					return false;
				else if (!ClientsCanDropOthers.Value)
					return false;
			}
			return true;
		}

		private void Awake()
		{
			ClientsCanDrop = Config.Wrap(section: "", key: "clients_can_drop", description: @"Allows client with the mod installed to drop their own items.", defaultValue: true);
			HostCanDropOthers = Config.Wrap(section: "", key: "host_can_drop_others", description: @"Allows host drop other players' items.", defaultValue: true);
			ClientsCanDropOthers = Config.Wrap(section: "", key: "clients_can_drop_others", description: @"Allows clients with the mod installed to drop other players' items.", defaultValue: false);

			SceneManager.sceneUnloaded += OnSceneUnloaded;
			var miniRpc = MiniRpc.CreateInstance("com.kookehs.dropitems");

			SendDropItemRequest = miniRpc.RegisterAction(Target.Server, (user, x) => {
				var master = x.ReadGameObject();
				var itemid = x.ReadItemIndex();

				var inventory = master.GetComponent<Inventory>();
				var characterMaster = master.GetComponent<CharacterMaster>();
				if (inventory == null || characterMaster == null)
					return;

				if (!IsAllowedToDrop(user, characterMaster))
					return;

				DropItem(inventory, itemid);
			});
			SendDropEquipmentRequest = miniRpc.RegisterAction(Target.Server, (NetworkUser user, GameObject master) => {
				var inventory = master.GetComponent<Inventory>();
				var characterMaster = master.GetComponent<CharacterMaster>();
				if (inventory == null || characterMaster == null)
					return;

				if (!IsAllowedToDrop(user, characterMaster))
					return;

				DropEquipment(inventory);
			});

			Debug.Log("Loaded DropItemsMod");
		}

		private void OnSceneUnloaded(Scene current)
		{
			CachedHudInstancesList = null;
		}

		private void Update()
		{
			if (CachedHudInstancesList == null)
			{
				CachedHudInstancesList = (List<HUD>)typeof(HUD).GetField("instancesList", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			}

			foreach (HUD hud in CachedHudInstancesList)
			{
				if (hud == null) continue;
				FieldInfo fieldInfo = typeof(ItemInventoryDisplay).GetField("itemIcons", BindingFlags.NonPublic | BindingFlags.Instance);
				UpdateHud(hud, fieldInfo);
				UpdateScoreboard(hud, fieldInfo);
			}
		}

		private void UpdateHud(HUD hud, FieldInfo fieldInfo)
		{
			if (hud.itemInventoryDisplay != null)
			{
				if (fieldInfo != null)
				{
					List<ItemIcon> itemIcons = (List<ItemIcon>)fieldInfo.GetValue(hud.itemInventoryDisplay);

					foreach (ItemIcon itemIcon in itemIcons)
					{
						if (itemIcon != null && itemIcon.GetComponent<DropItemController>() == null)
						{
							DropItemController dropItemController = itemIcon.gameObject.AddComponent<DropItemController>();
							Inventory inventory = (Inventory)typeof(ItemInventoryDisplay).GetField("inventory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(hud.itemInventoryDisplay);
							dropItemController.Inventory = inventory;
							dropItemController.ItemIcon = itemIcon;
							dropItemController.SendDropItemToServer = SendDropItemRequest;
						}
					}
				}
			}

			foreach (EquipmentIcon equipmentIcon in hud.equipmentIcons)
			{
				if (equipmentIcon != null && equipmentIcon.GetComponent<DropItemController>() == null)
				{
					if (equipmentIcon.targetInventory == null) continue;
					EquipmentIndex equipmentIndex = (equipmentIcon.displayAlternateEquipment) ? equipmentIcon.targetInventory.alternateEquipmentIndex : equipmentIcon.targetInventory.currentEquipmentIndex;
					if (equipmentIndex != EquipmentIndex.None)
					{
						DropItemController dropItemController = equipmentIcon.gameObject.AddComponent<DropItemController>();
						dropItemController.Inventory = equipmentIcon.targetInventory;
						dropItemController.EquipmentIcon = equipmentIcon;
						dropItemController.SendDropEquipmentToServer = SendDropEquipmentRequest;
					}
				}
			}
		}

		private void UpdateScoreboard(HUD hud, FieldInfo fieldInfo)
		{
			ScoreboardController scoreboardController = hud.scoreboardPanel.GetComponent<ScoreboardController>();

			if (scoreboardController != null)
			{
				UIElementAllocator<ScoreboardStrip> elements = (UIElementAllocator<ScoreboardStrip>)typeof(ScoreboardController)
					.GetField("stripAllocator", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(scoreboardController);

				if (elements != null)
				{
					List<ScoreboardStrip> scoreboardStrips = (List<ScoreboardStrip>)typeof(UIElementAllocator<ScoreboardStrip>)
						.GetField("elementControllerComponentsList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(elements);

					foreach (ScoreboardStrip scoreboardStrip in scoreboardStrips)
					{
						if (scoreboardStrip == null) continue;

						if (fieldInfo != null)
						{
							List<ItemIcon> itemIcons = (List<ItemIcon>)fieldInfo.GetValue(scoreboardStrip.itemInventoryDisplay);

							foreach (ItemIcon itemIcon in itemIcons)
							{
								if (itemIcon != null && itemIcon.GetComponent<DropItemController>() == null)
								{
									DropItemController dropItemController = itemIcon.gameObject.AddComponent<DropItemController>();
									Inventory inventory = (Inventory)typeof(ItemInventoryDisplay).GetField("inventory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(scoreboardStrip.itemInventoryDisplay);
									dropItemController.Inventory = inventory;
									dropItemController.ItemIcon = itemIcon;
									dropItemController.SendDropItemToServer = SendDropItemRequest;
								}
							}
						}

						EquipmentIcon equipmentIcon = scoreboardStrip.equipmentIcon;

						if (equipmentIcon != null && equipmentIcon.GetComponent<DropItemController>() == null)
						{
							if (equipmentIcon.targetInventory == null) continue;
							EquipmentIndex equipmentIndex = (equipmentIcon.displayAlternateEquipment) ? equipmentIcon.targetInventory.alternateEquipmentIndex : equipmentIcon.targetInventory.currentEquipmentIndex;

							if (equipmentIndex != EquipmentIndex.None)
							{
								DropItemController dropItemController = equipmentIcon.gameObject.AddComponent<DropItemController>();
								dropItemController.Inventory = equipmentIcon.targetInventory;
								dropItemController.EquipmentIcon = equipmentIcon;
								dropItemController.SendDropEquipmentToServer = SendDropEquipmentRequest;
							}
						}
					}
				}
			}
		}

		public static void DropItem(Inventory inventory, ItemIndex itemIndex) {
			CharacterBody characterBody = inventory.GetComponent<CharacterMaster>().GetBody();
			if (characterBody == null || characterBody.healthComponent == null || characterBody.healthComponent.alive == false) return;
			Transform transform = characterBody.transform;

			if (inventory.GetItemCount(itemIndex) != 0) {
				inventory.RemoveItem(itemIndex, 1);
				PickupDropletController.CreatePickupDroplet(new PickupIndex(itemIndex), transform.position, Vector3.up * 20f + transform.forward * 10f);
			}
		}

		public static void DropEquipment(Inventory inventory) {
			CharacterBody characterBody = inventory.GetComponent<CharacterMaster>().GetBody();
			if (characterBody == null || characterBody.healthComponent == null || characterBody.healthComponent.alive == false) return;
			Transform transform = characterBody.transform;

			EquipmentIndex equipmentIndex = inventory.GetEquipmentIndex();
			inventory.SetEquipmentIndex(EquipmentIndex.None);

			if (equipmentIndex != EquipmentIndex.None) {
				PickupDropletController.CreatePickupDroplet(new PickupIndex(equipmentIndex), transform.position, Vector3.up * 20f + transform.forward * 10f);
			}
		}
	}

	public class DropItemController : MonoBehaviour, IPointerClickHandler
	{
		public static List<DropItemController> InstancesList { get; set; } = new List<DropItemController>();
		public static bool ShouldDestroy { get; set; }
		public Inventory Inventory { get; set; } = null;
		public ItemIcon ItemIcon { get; set; } = null;
		public EquipmentIcon EquipmentIcon { get; set; } = null;
		public IRpcAction<Action<NetworkWriter>> SendDropItemToServer { get; set; }
		public IRpcAction<GameObject> SendDropEquipmentToServer { get; set; }

		private void Awake()
		{
			// TODO(kookehs): Map instances of this to players.
			InstancesList.Add(this);
		}

		private void LateUpdate()
		{
			if (ShouldDestroy)
			{
				foreach (DropItemController dropItemController in InstancesList)
				{
					Destroy(dropItemController);
				}

				InstancesList.Clear();
				ShouldDestroy = false;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (ShouldDestroy) return;

			CharacterBody characterBody = Inventory.GetComponent<CharacterMaster>().GetBody();
			if (characterBody == null || characterBody.healthComponent == null || characterBody.healthComponent.alive == false) return;

			if (Inventory == null || (ItemIcon == null && EquipmentIcon == null))
			{
				ShouldDestroy = true;
				return;
			}

			// TODO: Send notification to the player that got his item dropped.
			Notification notification = characterBody.gameObject.AddComponent<Notification>();
			notification.transform.SetParent(characterBody.transform);
			notification.SetPosition(new Vector3((float)(Screen.width * 0.8), (float)(Screen.height * 0.25), 0));
			Transform transform = characterBody.transform;

			if (EquipmentIcon != null)
			{
				EquipmentIndex equipmentIndex = (EquipmentIcon.displayAlternateEquipment) ? EquipmentIcon.targetInventory.alternateEquipmentIndex : EquipmentIcon.targetInventory.currentEquipmentIndex;

				if (equipmentIndex != EquipmentIndex.None)
				{
					EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
					notification.SetIcon(Resources.Load<Texture>(equipmentDef.pickupIconPath));
					notification.GetTitle = () => "Equipment dropped";
					notification.GetDescription = () => $"{Language.GetString(equipmentDef.nameToken)}";
					SendDropEquipmentToServer.Invoke(characterBody.masterObject);

					// TODO: Only send ShouldDestroy on server ACK. Maybe use an RPCFunc?
					ShouldDestroy = true;
					return;
				}
			}

			int[] itemStacks = (int[])typeof(Inventory).GetField("itemStacks", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Inventory);
			ItemIndex itemIndex = (ItemIndex)typeof(ItemIcon).GetField("itemIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ItemIcon);

			if (itemStacks[(int)itemIndex] > 0)
			{
				ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
				notification.SetIcon(Resources.Load<Texture>(itemDef.pickupIconPath));
				notification.GetTitle = () => "Item dropped";
				notification.GetDescription = () => $"{Language.GetString(itemDef.nameToken)}";

				print("Sending DropItem request");
				SendDropItemToServer.Invoke(x => {
					x.Write(characterBody.masterObject);
					x.Write(itemIndex);
				});
			}

			// TODO: Only send ShouldDestroy on server ACK. Maybe use an RPCFunc?
			if (itemStacks[(int)itemIndex] <= 0)
			{
				ShouldDestroy = true;
			}
		}
	}
}
