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

        public delegate bool AltFunctionUseDelegate(Item item, Player player);
        public static event AltFunctionUseDelegate AltFunctionUseEvent;
		public override bool AltFunctionUse(Item item, Player player)
		{
            if(AltFunctionUseEvent != null)
			{
                bool result = false;
                foreach (AltFunctionUseDelegate del in AltFunctionUseEvent.GetInvocationList())
				{
                    result |= del(item, player);
				}
                return result;
			}
			return base.AltFunctionUse(item, player);
		}

		public override void Unload()
		{
            GetHealLifeEvent = null;
            ModifyWeaponDamageEvent = null;
            GetWeaponCritEvent = null;
            PickAmmoEvent = null;
            OnPickupEvent = null;
            CanUseItemEvent = null;
		}
	}
}
