namespace DropItemsMod
{
	using RoR2;
	using RoR2.UI;
	using SeikoML;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.Networking;

	public class DropItemsMod : ISeikoMod
	{
		public static GameObject RootObject { get; set; }
		public static DropItemHandler DropItemHandler { get; set; }

		public void OnStart()
		{
			RootObject = new GameObject("DropItemsMod");
			Object.DontDestroyOnLoad(RootObject);
			DropItemHandler = RootObject.AddComponent<DropItemHandler>();
			DropItemHandler.IsMaster = true;
			Debug.Log("Loaded DropItemsMod");
		}
	}

	public class DropItemHandler : MonoBehaviour, IPointerClickHandler
	{
		public EquipmentIndex EquipmentIndex { get; set; } = EquipmentIndex.None;
		public ItemIndex ItemIndex { get; set; } = ItemIndex.None;
		public Inventory Inventory { get; set; } = null;
		public bool ItemDropped { get; set; } = false;
		public bool IsMaster { get; set; } = false;

		private void Update()
		{
			if (!NetworkServer.active || !IsMaster) return;

			// TODO(kookehs): Needs to be optimized.
			foreach (ItemIcon itemIcon in Api.GetItemIcons())
			{
				if (itemIcon.GetComponent<DropItemHandler>() == null)
				{
					DropItemHandler dropItemHandler = itemIcon.transform.gameObject.AddComponent<DropItemHandler>();
					dropItemHandler.ItemIndex = itemIcon.ItemIndex;
					dropItemHandler.Inventory = itemIcon.rectTransform.parent.GetComponent<ItemInventoryDisplay>().Inventory;
				}
			}

			foreach (EquipmentIcon equipmentIcon in Api.GetEquipmentIcons())
			{
				if (equipmentIcon.GetComponent<DropItemHandler>() == null)
				{
					if (equipmentIcon.targetEquipmentSlot == null || equipmentIcon.targetEquipmentSlot.equipmentIndex == EquipmentIndex.None) return;
					DropItemHandler dropItemHandler = equipmentIcon.transform.gameObject.AddComponent<DropItemHandler>();
					dropItemHandler.EquipmentIndex = equipmentIcon.targetEquipmentSlot.equipmentIndex;
					dropItemHandler.Inventory = equipmentIcon.targetInventory;
				}
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!NetworkServer.active || Inventory == null || IsMaster) return;

			if (ItemDropped)
			{
				return;
			}

			ItemDropped = true;
			CharacterBody characterBody = Inventory.GetComponent<CharacterMaster>().GetBody();
			Api.Notification notification = characterBody.gameObject.AddComponent<Api.Notification>(); ;
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
				PickupDropletController.CreatePickupDroplet(new PickupIndex(EquipmentIndex), transform.position, Vector3.up * 20f + transform.forward * 10f);
				Destroy(this);
				return;
			}

			ItemDef itemDef = ItemCatalog.GetItemDef(ItemIndex);
			notification.SetIcon(Resources.Load<Texture>(itemDef.pickupIconPath));
			notification.GetTitle = () => "Item dropped";
			notification.GetDescription = () => $"{Language.GetString(itemDef.nameToken)}";
			Inventory.RemoveItem(ItemIndex, 1);
			PickupDropletController.CreatePickupDroplet(new PickupIndex(ItemIndex), transform.position, Vector3.up * 20f + transform.forward * 10f);
			Destroy(this);
		}
	}
}
