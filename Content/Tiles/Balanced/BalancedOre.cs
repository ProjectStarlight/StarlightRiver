using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Balanced;

namespace StarlightRiver.Content.Tiles.Balanced
{
    internal class OreEbony : ModTile
    {
        public override bool Autoload(ref string name, ref string texture) {
            texture = AssetDirectory.BalancedTile + name;
            return base.Autoload(ref name, ref texture); }

        public override void SetDefaults() => this.QuickSet(0, DustID.Stone, SoundID.Tink, new Color(80, 80, 90), ItemType<Items.Balanced.OreEbony>(), true, true, "Ebony Ore");
    }

    internal class OreIvory : ModTile
    {
        public override bool Autoload(ref string name, ref string texture) {
            texture = AssetDirectory.BalancedTile + name;
            return base.Autoload(ref name, ref texture); }

        public override void SetDefaults() => this.QuickSet(100, DustID.Stone, SoundID.Tink, new Color(255, 255, 220), ItemType<Items.Balanced.OreIvory>(), true, true, "Ivory Ore"); 
    }
}