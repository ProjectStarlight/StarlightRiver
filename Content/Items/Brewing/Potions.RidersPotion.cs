using StarlightRiver.Buffs;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Items.Herbology.Potions
{
    internal class RidersPotion : QuickPotion
    {
        public RidersPotion() : base("Riders Potion", "Increases crit chance while on a mount", 36000, BuffType<RidersPotionBuff>(), 2)
        {
        }
    }
}