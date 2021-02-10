using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    class BossAltar : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + "BossAltar";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 15, 7, DustType<Dusts.Stone>(), SoundID.Tink, false, new Color(200, 200, 180), false, true, "[PH] OG Boss Altar");
    }
}
