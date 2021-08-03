using StarlightRiver.Core;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Slime
{
	public class SlimeItem : QuickMaterial
    {
        public override string Texture => AssetDirectory.SlimeItem + Name;
        public SlimeItem() : base("Slime glob", "sticks to your hands", 999, Item.sellPrice(0, 0, 0, 2), ItemRarityID.Green) { }
    }
}