using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	internal partial class StarlightItem : GlobalItem
    {
        public delegate void GetHealLifeDelegate(Item Item, Player Player, bool quickHeal, ref int healValue);
        public static event GetHealLifeDelegate GetHealLifeEvent;
        public override void GetHealLife(Item Item, Player Player, bool quickHeal, ref int healValue)
        {
            GetHealLifeEvent?.Invoke(Item, Player, quickHeal, ref healValue);
        }

        public delegate void ModifyWeaponDamageDelegate(Item Item, Player Player, ref StatModifier statModifier);
        public static event ModifyWeaponDamageDelegate ModifyWeaponDamageEvent;
		public override void ModifyWeaponDamage(Item Item, Player Player, ref StatModifier statModifier)
		{
            ModifyWeaponDamageEvent?.Invoke(Item, Player, ref statModifier);
		}

        public delegate void GetWeaponCritDelegate(Item Item, Player Player, ref float crit);
        public static event GetWeaponCritDelegate GetWeaponCritEvent;
		public override void ModifyWeaponCrit(Item Item, Player Player, ref float crit)
		{
			GetWeaponCritEvent?.Invoke(Item, Player, ref crit);
		}

        public delegate void PickAmmoDelegate(Item weapon, Item ammo, Player Player, ref int type, ref float speed, ref StatModifier damage, ref float knockback);
        public static event PickAmmoDelegate PickAmmoEvent;
		public override void PickAmmo(Item weapon, Item ammo, Player Player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
		{
            PickAmmoEvent?.Invoke(weapon, ammo, Player, ref type, ref speed, ref damage, ref knockback);
		}

        public delegate bool OnPickupDelegate(Item Item, Player Player);
        public static event OnPickupDelegate OnPickupEvent;
		public override bool OnPickup(Item Item, Player Player)
		{
            if (OnPickupEvent != null)
            {
                bool result = true;
                foreach (OnPickupDelegate del in OnPickupEvent.GetInvocationList())
                {
                    result &= del(Item, Player);
                }
                return result;
            }
            return base.OnPickup(Item, Player);
        }

		public delegate bool CanUseItemDelegate(Item Item, Player Player);
        public static event CanUseItemDelegate CanUseItemEvent;
		public override bool CanUseItem(Item Item, Player Player)
		{
            if (CanUseItemEvent != null)
            {
                bool result = true;
                foreach (CanUseItemDelegate del in CanUseItemEvent.GetInvocationList())
                {
                    result &= del(Item, Player);
                }
                return result;
            }
            return true;
        }

        public delegate bool CanEquipAccessoryDelegate(Item item, Player player, int slot, bool modded);
        public static event CanEquipAccessoryDelegate CanEquipAccessoryEvent;
        public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
        {
            if (CanEquipAccessoryEvent != null)
            {
                bool result = true;
                foreach (CanEquipAccessoryDelegate del in CanEquipAccessoryEvent.GetInvocationList())
                {
                    result &= del(item, player, slot, modded);
                }
                return result;
            }
            return true;
        }

        public delegate bool CanAccessoryBeEquippedWithDelegate(Item equippedItem, Item incomingItem, Player player);
        public static event CanAccessoryBeEquippedWithDelegate CanAccessoryBeEquippedWithEvent;
        public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
        {
            if (CanAccessoryBeEquippedWithEvent != null)
            {
                bool result = true;
                foreach (CanAccessoryBeEquippedWithDelegate del in CanAccessoryBeEquippedWithEvent.GetInvocationList())
                {
                    result &= del(equippedItem, incomingItem, player);
                }
                return result;
            }
            return true;
        }

        public delegate void ModifyItemLootDelegate(Item item, ItemLoot itemLoot);
        public static event ModifyItemLootDelegate ModifyItemLootEvent;
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            ModifyItemLootEvent?.Invoke(item, itemLoot);
        }

        public override void Unload()
		{
            GetHealLifeEvent = null;
            ModifyWeaponDamageEvent = null;
            GetWeaponCritEvent = null;
            PickAmmoEvent = null;
            OnPickupEvent = null;
            CanUseItemEvent = null;
            CanEquipAccessoryEvent = null;
            CanAccessoryBeEquippedWithEvent = null;
            ModifyItemLootEvent = null;
		}
	}
}
