using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Items;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Permafrost
{
    class PermafrostSnow : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(100, 255, 255), ItemType<PermafrostSnowItem>());

        public override void RandomUpdate(int i, int j)
        {
            if(WorldGen.InWorld(i, j - 1))
            {
                if(!Framing.GetTileSafely(i, j-1).active()) WorldGen.PlaceTile(i, j-1, TileType<Decoration.SnowGrass>());
            }
        }
    }

    class PermafrostSnowItem : QuickTileItem
    {
        public PermafrostSnowItem() : base("Permafrost Snow", "", TileType<PermafrostSnow>(), ItemRarityID.White) { }
    }
}
