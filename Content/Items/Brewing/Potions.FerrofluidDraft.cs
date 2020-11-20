using StarlightRiver.Buffs;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Herbology.Potions
{
    internal class FerrofluidDraft : QuickPotion
    {
        public FerrofluidDraft() : base("Ferrofluid Draft", "Turns you into a magnet for items", 36000, BuffType<FerrofluidDraftBuff>(), 3)
        {
        }
    }
}