using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	internal partial class StarlightItem : GlobalItem
    {
        public delegate void GetHealLifeDelegate(Item item, Player player, bool quickHeal, ref int healValue);
        public static event GetHealLifeDelegate GetHealLifeEvent;
        public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
        {
            GetHealLifeEvent?.Invoke(item, player, quickHeal, ref healValue);
        }

        public delegate void ModifyWeaponDamageDelegate(Item item, Player player, ref float add, ref float mult, ref float flat);
        public static event ModifyWeaponDamageDelegate ModifyWeaponDamageEvent;
		public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float mult, ref float flat)
		{
            ModifyWeaponDamageEvent?.Invoke(item, player, ref add, ref mult, ref flat);
		}

        public delegate void GetWeaponCritDelegate(Item item, Player player, ref int crit);
        public static event GetWeaponCritDelegate GetWeaponCritEvent;
		public override void GetWeaponCrit(Item item, Player player, ref int crit)
		{
            GetWeaponCritEvent?.Invoke(item, player, ref crit);
		}
	}
}
