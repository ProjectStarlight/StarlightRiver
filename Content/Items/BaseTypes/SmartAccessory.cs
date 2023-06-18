using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Achievements;

namespace StarlightRiver.Content.Items.BaseTypes
{
	public abstract class SmartAccessory : ModItem
	{
		public static int ACCESSORY_START_INDEX = 3;
		public static int ACCESSORY_END_INDEX = 9;
		public static int VANITY_ACCESSORY_START_INDEX = 13;
		public static int VANITY_ACCESSORY_END_INDEX = 19;
		public static int DEFAULT_ACCESSORY_SLOT_COUNT = 5;

		/// <summary>
		/// For use with simulated accessories, the accessory simulating this one is it's parent.
		/// This is a list for if you are simulating the same accessory from multiple sources,
		/// so it is only lost when all are unequipped and can persist if only one is.
		///  </summary>
		public List<Item> parents = new();

		/// <summary>
		/// If the item should dissappear if all of its parents are gone
		/// </summary>
		public bool isChild;

		private readonly string name;
		private readonly string tooltip;

		/// <summary>
		/// Override this and add the types of SmartAccessories you want this one to simulate. Note that simulated accessories are reset on unequip/reload, so you will need to save any persistent data on the parent accessory.
		/// </summary>
		public virtual List<int> ChildTypes => new();

		protected SmartAccessory(string name, string tooltip) : base()
		{
			this.name = name;
			this.tooltip = tooltip;
		}

		/// <summary>
		/// If this accessory is equipped in a normal slot on the given player or simulated by another accessory.
		/// </summary>
		/// <param name="Player">The player to check for the item on</param>
		/// <returns>If the item is equipped or simulated.</returns>
		public bool Equipped(Player player)
		{
			for (int k = ACCESSORY_START_INDEX; k <= ACCESSORY_END_INDEX; k++)
			{
				if (player.IsItemSlotUnlockedAndUsable(k))
				{
					if (player.armor[k].type == Item.type)
						return true;
				}
			}

			AccessorySimulationPlayer mp = player.GetModPlayer<AccessorySimulationPlayer>();
			if (mp.simulatedAccessories.Any(n => n.type == Item.type))
				return true;

			return false;
		}

		/// <summary>
		/// Gets the instance of the accessory thats equipped in a player's normal slots or that is being simulated by other accessories.
		/// </summary>
		/// <param name="player">The player to get the equipped instance from.</param>
		/// <returns>The SmartAccessory instance if one is found, null if the item is not equipped or simulated.</returns>
		public SmartAccessory GetEquippedInstance(Player player)
		{
			return GetEquippedInstance(player, Item.type);
		}

		/// <summary>
		/// Gets the instance of an equipped accessory based on it's type in a given player's normal slots, or being simulated by other accessories.
		/// </summary>
		/// <param name="player">The player to get the equipped instance from.</param>
		/// <param name="type">The type of accessory to look for, this should be the ID of an item extending SmartAccessory</param>
		/// <returns>The SmartAccessory instance if one is found, null if the item is not equipped or simulated.</returns>
		public static SmartAccessory GetEquippedInstance(Player player, int type)
		{
			int accessoryCount = DEFAULT_ACCESSORY_SLOT_COUNT + player.GetAmountOfExtraAccessorySlotsToShow();

			for (int k = ACCESSORY_START_INDEX; k <= ACCESSORY_END_INDEX; k++)
			{
				if (player.armor[k].type == type && player.IsItemSlotUnlockedAndUsable(k))
					return player.armor[k].ModItem as SmartAccessory;
			}

			AccessorySimulationPlayer mp = player.GetModPlayer<AccessorySimulationPlayer>();
			return mp.simulatedAccessories.FirstOrDefault(n => n.type == type)?.ModItem as SmartAccessory;
		}

		/// <summary>
		/// Gets the instance of the accessory directly or simulated in a vanity slot
		/// </summary>
		/// <param name="player">The player to get the equipped instance from.</param>
		/// <returns>The SmartAccessory instance if one is found, null if the item is not equipped or simulated in a vanity slot.</returns>
		public SmartAccessory GetVisualInstance(Player player)
		{
			return GetVisualInstance(player, Item.type);
		}

		/// <summary>
		/// Gets the instance of the accessory directly or simulated in a vanity slot
		/// </summary>
		/// <param name="player">The player to get the equipped instance from.</param>
		/// <param name="type">The type of accessory to look for, this should be the ID of an item extending SmartAccessory</param>
		/// <returns>The SmartAccessory instance if one is found, null if the item is not equipped or simulated in a vanity slot.</returns>
		public static SmartAccessory GetVisualInstance(Player player, int type)
		{
			for (int k = VANITY_ACCESSORY_START_INDEX; k <= VANITY_ACCESSORY_END_INDEX; k++)
			{
				if (player.armor[k].type == type && player.IsItemSlotUnlockedAndUsable(k))
					return player.armor[k].ModItem as SmartAccessory;
			}
			
			AccessorySimulationPlayer mp = player.GetModPlayer<AccessorySimulationPlayer>();
			return mp.simulatedAccessories.FirstOrDefault(n => n.type == type)?.ModItem as SmartAccessory;
		}

		/// <summary>
		/// Adds an item to be simulated.
		/// </summary>
		/// <param name="itemType"></param>
		/// <param name="player"></param>
		private void Simulate(int itemType, Player player)
		{
			AccessorySimulationPlayer mp = player.GetModPlayer<AccessorySimulationPlayer>();

			Item existingSimulacrum = mp.simulatedAccessories.FirstOrDefault(n => n.type == itemType);
			var simulacrumModItem = existingSimulacrum?.ModItem as SmartAccessory;

			if (existingSimulacrum is null) //There isnt a simulation for this accessory yet, lets add one!
			{
				var item = new Item();
				item.SetDefaults(itemType);

				mp.simulatedAccessories.Add(item);
				simulacrumModItem = item.ModItem as SmartAccessory;
				simulacrumModItem.Equip(player, item);
				simulacrumModItem.parents.Add(Item);
				simulacrumModItem.isChild = true;
				return;
			}

			if (!simulacrumModItem.parents.Contains(Item)) //Add this item to the parents if there is one being simulated already, but it isnt already parented to this
				simulacrumModItem.parents.Add(Item);
		}

		public void Equip(Player player, Item item)
		{
			foreach (int type in ChildTypes)
				Simulate(type, player);

			OnEquip(player, item);
		}

		/// <summary>
		/// Effects which happen when the accessory is equipped. Generally should be kept to things such as resetting data.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="context"></param>
		public virtual void OnEquip(Player player, Item item) { }

		public virtual void SafeSetDefaults() { }

		public virtual void SafeUpdateEquip(Player Player) { }

		public virtual bool SafeCanAccessoryBeEquippedWith(Item equipped, Item incoming, Player player) { return true; }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(name);
			Tooltip.SetDefault(tooltip);
		}

		public sealed override void SetDefaults()
		{
			SafeSetDefaults();
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
		}

		public sealed override void UpdateEquip(Player player)
		{
			if (isChild)
			{
				var toRemove = new List<Item>(); //Removes unequipped accessories from parents

				foreach (Item item in parents)
				{
					var modItem = item.ModItem as SmartAccessory;
					if (modItem is null || !modItem.Equipped(player))
						toRemove.Add(item);
				}

				toRemove.ForEach(n => parents.Remove(n));
				toRemove.Clear();

				if (parents.Count == 0) //Destroy this simulation if it has no viable parents
					Item.TurnToAir();
			}

			SafeUpdateEquip(player);
		}

		public sealed override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
		{
			if (equippedItem.ModItem is SmartAccessory && incomingItem.ModItem is SmartAccessory)
			{
				var equipped = equippedItem.ModItem as SmartAccessory;
				var incoming = incomingItem.ModItem as SmartAccessory;

				if (equipped.ChildTypes.Contains(incoming.Type) || incoming.ChildTypes.Contains(equipped.Type)) //Prevents equipping up or down a simulation chain
					return false;

				/*var mp = player.GetModPlayer<AccessorySimulationPlayer>();

                if (mp.simulatedAccessories.Any(n => n.type == incomingItem.type))
                    return false;*/
			}

			return SafeCanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
		}
	}

	public class AccessorySimulationPlayer : ModPlayer
	{
		public List<Item> simulatedAccessories = new();

		public int[] accsLastFrame = new int[20];

		public override void OnEnterWorld()
		{
			simulatedAccessories.Clear();
			for (int k = SmartAccessory.ACCESSORY_START_INDEX; k <= SmartAccessory.ACCESSORY_END_INDEX; k++)
			{
				(Player.armor[k].ModItem as SmartAccessory)?.Equip(Player, Player.armor[k]);
				accsLastFrame[k] = Player.armor[k].type;
			}
		}

		public override void UpdateEquips()
		{
			//iterate over equipped accessories so we can discover if any have changed in order to equip this
			//done this way so we can capture all the various ways terraria has for modifying equip slots cloned/not cloned quick swap, loadout swap etc.
			for (int k = SmartAccessory.ACCESSORY_START_INDEX; k <= SmartAccessory.ACCESSORY_END_INDEX; k++)
			{
				if (Player.armor[k].type != accsLastFrame[k] && Player.IsItemSlotUnlockedAndUsable(k))
				{
					accsLastFrame[k] = Player.armor[k].type;
					(Player.armor[k].ModItem as SmartAccessory)?.Equip(Player, Player.armor[k]);
				}
			}

			simulatedAccessories.RemoveAll(n => n.IsAir);

			foreach (Item item in simulatedAccessories)
			{
				ModItem modItem = item.ModItem;
				modItem.UpdateAccessory(Player, true);
				modItem.UpdateEquip(Player);
			}
		}
	}
}