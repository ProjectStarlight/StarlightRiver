using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

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
    }
}
