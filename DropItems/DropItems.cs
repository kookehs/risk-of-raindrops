namespace DropItemsMod
{
	using SeikoML;
	using UnityEngine;
	using UnityEngine.EventSystems;
	using UnityEngine.Networking;
	using RoR2;
	using RoR2.UI;

	public class DropItemHandler : MonoBehaviour, IPointerClickHandler
	{
		public EquipmentIndex EquipmentIndex { get; set; } = EquipmentIndex.None;
		public ItemIndex ItemIndex { get; set; } = ItemIndex.None;
		public Inventory Inventory { get; set; } = null;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (!NetworkServer.active || Inventory == null) return;
			string nameToken;
			Api.Notification notification = Inventory.GetComponent<CharacterMaster>().gameObject.AddComponent<Api.Notification>(); ;
			notification.transform.SetParent(Inventory.GetComponent<CharacterMaster>().GetBody().transform);
			notification.SetPosition(new Vector3((float)(Screen.width * 0.8), (float)(Screen.height * 0.25), 0));
			CharacterBody characterBody = Inventory.GetComponent<CharacterMaster>().GetBody();
			Transform transform = characterBody.transform;

			if (EquipmentIndex != EquipmentIndex.None)
			{
				EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(EquipmentIndex);
				nameToken = equipmentDef.nameToken;
				notification.SetIcon(Resources.Load<Texture>(equipmentDef.pickupIconPath));
				notification.GetTitle = () => "Equipment dropped";
				notification.GetDescription = () => $"{Language.GetString(nameToken)}";
				Inventory.SetEquipmentIndex(EquipmentIndex.None);
				Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscLockbox").DoSpawn(transform.position, Quaternion.identity).GetComponent<ChestBehavior>().SetDropPickup(EquipmentIndex);
				return;
			}

			ItemDef itemDef = ItemCatalog.GetItemDef(ItemIndex);
			nameToken = itemDef.nameToken;
			notification.SetIcon(Resources.Load<Texture>(itemDef.pickupIconPath));
			notification.GetTitle = () => "Item dropped";
			notification.GetDescription = () => $"{Language.GetString(nameToken)}";
			Inventory.RemoveItem(ItemIndex, 1);
			Resources.Load<SpawnCard>("SpawnCards/InteractableSpawnCard/iscLockbox").DoSpawn(transform.position, Quaternion.identity).GetComponent<ChestBehavior>().SetDropPickup(ItemIndex);
		}
	}

	// TODO(kookehs): Turn this into a MonoBehavior and attach it to a GameObject.
	public class DropItemsMod : IKookehsMod
	{
		public void OnStart()
		{
			Debug.Log("Loaded DropItemsMod");
		}

		public void OnUpdate()
		{
			if (Api.GetRun() == null) return;
			ItemIcon[] itemIcons = Api.GetItemIcons();

			foreach (ItemIcon itemIcon in itemIcons)
			{
				if (itemIcon.GetComponent<DropItemHandler>() == null)
				{
					DropItemHandler dropItemHandler = itemIcon.transform.gameObject.AddComponent<DropItemHandler>();
					dropItemHandler.ItemIndex = itemIcon.ItemIndex;
					dropItemHandler.Inventory = itemIcon.rectTransform.parent.GetComponent<ItemInventoryDisplay>().Inventory;
				}
			}

			EquipmentIcon[] equipmentIcons = Api.GetEquipmentIcons();

			foreach (EquipmentIcon equipmentIcon in equipmentIcons)
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
	}
}
