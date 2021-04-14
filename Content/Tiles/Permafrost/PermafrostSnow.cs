using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class PermafrostSnow : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/PermafrostSnow";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(100, 255, 255), ItemType<PermafrostSnowItem>());

        public override void RandomUpdate(int i, int j)
        {
            if (WorldGen.InWorld(i, j - 1))
            {
                if (!Framing.GetTileSafely(i, j - 1).active()) WorldGen.PlaceTile(i, j - 1, TileType<Decoration.SnowGrass>());
            }
        }
    }

    class PermafrostSnowItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/PermafrostSnowItem";

        public PermafrostSnowItem() : base("Permafrost Snow", "", TileType<PermafrostSnow>(), ItemRarityID.White) { }
    }
}
