using Terraria;
using Terraria.ID;

using StarlightRiver.Core;

namespace StarlightRiver.Items.Slime
{
    public class SlimeItem : QuickMaterial
    {
        public SlimeItem() : base("Slime glob", "sticks to your hands", 999, Item.sellPrice(0, 0, 0, 2), ItemRarityID.Green) { }
    }
}