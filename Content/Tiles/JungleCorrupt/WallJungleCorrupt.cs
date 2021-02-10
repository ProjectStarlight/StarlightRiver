using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleCorrupt
{
    public class WallJungleCorrupt : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.JungleCorruptTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.wallHouse[Type] = true;
            dustType = 14;
            drop = mod.ItemType("WallJungleCorruptItem");
            AddMapEntry(new Color(42, 36, 52));
        }

        public override void RandomUpdate(int i, int j)
        {
            for (int x = i - 4; x <= i + 4; x++)
            {
                for (int y = j - 4; y <= j + 4; y++)
                {
                    if (Main.tile[x, y].active())
                    {
                        return;
                    }
                }
            }
            if (!Main.tile[i, j].active())
            {
                if (Main.rand.Next(30) == 0)
                {
                    WorldGen.PlaceTile(i, j, TileType<SporeJungleCorrupt>(), true);
                }
            }
        }
    }
}