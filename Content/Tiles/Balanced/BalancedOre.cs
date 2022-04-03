using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Balanced
{
	internal class OreEbony : ModTile
    {
        public override string Texture => AssetDirectory.BalancedTile + Name;

        public override void SetDefaults() => this.QuickSet(0, DustID.Stone, SoundID.Tink, new Color(80, 80, 90), ItemType<Items.Balanced.OreEbony>(), true, true, "Ebony Ore");
    }

    internal class OreIvory : ModTile
    {
        public override string Texture => AssetDirectory.BalancedTile + Name;

        public override void SetDefaults() => this.QuickSet(100, DustID.Stone, SoundID.Tink, new Color(255, 255, 220), ItemType<Items.Balanced.OreIvory>(), true, true, "Ivory Ore"); 
    }
}