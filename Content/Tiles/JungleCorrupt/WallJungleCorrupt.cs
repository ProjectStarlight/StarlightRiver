using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.JungleCorrupt
{
	public class WallJungleCorrupt : ModWall
    {
        public override string Texture => AssetDirectory.JungleCorruptTile + Name;

        public override void SetDefaults()
        {
            Main.wallHouse[Type] = true;
            dustType = 14;
            drop = ItemType<WallJungleCorruptItem>();
            AddMapEntry(new Color(42, 36, 52));
        }

        public override void RandomUpdate(int i, int j)
        {
            for (int x = i - 4; x <= i + 4; x++)
            {
                for (int y = j - 4; y <= j + 4; y++)
                {
                    if (Main.tile[x, y].HasTile)
                    {
                        return;
                    }
                }
            }
            if (!Main.tile[i, j].HasTile)
            {
                if (Main.rand.Next(30) == 0)
                {
                    WorldGen.PlaceTile(i, j, TileType<SporeJungleCorrupt>(), true);
                }
            }
        }
    }
}