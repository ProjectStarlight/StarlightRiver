using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Balanced
{
	public class IvoryBar : ModTile
    {
        public override string Texture => AssetDirectory.BalancedTile + Name;

        public override void SetDefaults() => this.QuickSetBar(ItemType<Items.Balanced.IvoryBar>(), 0, new Color(170, 165, 140));
    }

    public class EbonyBar : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)//a
        {
            texture = AssetDirectory.BalancedTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => this.QuickSetBar(ItemType<Items.Balanced.EbonyBar>(), 0, new Color(80, 70, 75));
    }
}