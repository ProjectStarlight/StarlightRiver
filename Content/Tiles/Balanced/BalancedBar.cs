using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Balanced
{
    public class IvoryBar : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.BalancedTile + name;
            return base.Autoload(ref name, ref texture);
        }

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