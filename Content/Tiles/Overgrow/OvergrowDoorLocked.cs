using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class OvergrowDoorLocked : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = true;

            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 7;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0);

            TileObjectData.addTile(Type);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameX > 100) tile.active(false);
            else tile.active(true);
        }

        public override bool NewRightClick(int i, int j)
        {
            if (Keys.Key.Use<Keys.OvergrowKey>())
            {
                for (int x = i - 2; x < i + 2; x++)
                    for (int y = j - 7; y < j + 7; y++)
                        if (Main.tile[x, y].type == Type)
                            Main.tile[x, y].frameX += 36;

                CombatText.NewText(new Rectangle(i * 16, j * 16, 1, 1), new Color(255, 255, 200), Main.tile[i, j].frameX / 36 + "/3");
                if (Main.tile[i, j].frameX > 100)
                    StarlightWorld.Flag(WorldFlags.OvergrowBossOpen);
            }
            return true;
        }
    }
}