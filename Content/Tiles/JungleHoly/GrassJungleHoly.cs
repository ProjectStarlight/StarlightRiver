using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleHoly
{
    public class GrassJungleHoly : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.JungleHolyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.Grass[Type] = true;
            TileID.Sets.GrassSpecial[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            drop = ItemID.MudBlock;
            SetModTree(new TreeJungleHoly());
            AddMapEntry(new Color(64, 193, 147));
            soundType = SoundID.Dig;
            dustType = 125;
        }

        public override void RandomUpdate(int i, int j)//grappling hook breaks the grass, its running killtile for some reason?
        {
            var x = Main.rand.Next(-4, 4);
            var y = Main.rand.Next(-4, 4);
            //Main.NewText("tick");

            if (Main.tile[i + x, j + y].active() && Main.hardMode)//spread
            {
                if (Main.tile[i + x, j + y].type == TileID.JungleGrass)
                {
                    //Main.NewText("Tile at: " + i + ", " + j + ". x/y: " + x + ", " + y + ". Placing at: " + (i + x) + ", " + (j + y));
                    WorldGen.PlaceTile(i + x, j + y, TileType<GrassJungleHoly>(), true, true);
                }
                else if (Main.tile[i + x, j + y].type == TileID.Mud)
                {
                    if (!Main.tileSolid[Main.tile[i + x + 1, j + y].type] || !Main.tileSolid[Main.tile[i + x - 1, j + y].type] || !Main.tileSolid[Main.tile[i + x, j + y + 1].type] || !Main.tileSolid[Main.tile[i + x, j + y - 1].type])
                    {
                        WorldGen.PlaceTile(i + x, j + y, TileType<GrassJungleHoly>(), true, true);
                    }
                }
                else if (Main.tile[i + x, j + y].type == TileID.Stone)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileID.Pearlstone, true, true);
                }
                else if (Main.tile[i + x, j + y].type == TileID.Grass)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileID.HallowedGrass, true, true);
                }
                else if (Main.tile[i + x, j + y].type == TileID.Sand)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileID.Pearlsand, true, true);
                }
                else if (Main.tile[i + x, j + y].type == TileID.IceBlock)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileID.HallowedIce, true, true);
                }
            }

            if (!Main.tile[i, j + 1].active() && Main.tile[i, j].slope() == 0)//vines (Maybe add the corruption thorns too?)
            {
                if (Main.rand.Next(5) == 0)
                {
                    WorldGen.PlaceTile(i, j + 1, TileType<VineJungleHoly>(), true);
                }
            }

            if (!Main.tile[i, j - 1].active() && Main.tile[i, j].slope() == 0)//grass
            {
                if (Main.rand.Next(5) == 0)
                {
                    WorldGen.PlaceTile(i, j - 1, TileType<TallgrassJungleHoly>(), true);
                }
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            effectOnly = true;
            WorldGen.PlaceTile(i, j, TileID.Mud, false, true);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.064f / 1.5f;
            g = 0.193f / 1.5f;
            b = 0.147f / 1.5f;
        }

        /*public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.rand.Next(600) == 0 && !Main.tile[i, j + 1].active() && Main.tile[i, j].slope() == 0)
            {
                Dust.NewDustPerfect(new Vector2(i, j) * 16, mod.DustType("Holy2"), new Vector2(0, 0.6f));
            }
        }*/
    }
}