using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.JungleBloody
{
    public class GrassJungleBloody : ModTile
    {
        public int x = 0;
        public int y = 0;

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;
            TileID.Sets.Grass[Type] = true;
            TileID.Sets.GrassSpecial[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            SetModTree(new TreeJungleBloody());
            drop = ItemID.MudBlock;
            AddMapEntry(new Color(143, 45, 45));
            soundType = SoundID.Dig;
            dustType = 125;
        }

        public override void RandomUpdate(int i, int j)//grappling hook breaks the grass, its running killtile for some reason?
        {
            x = Main.rand.Next(-4, 4);
            y = Main.rand.Next(-4, 4);
            //Main.NewText("tick");

            if (Main.tile[i + x, j + y].active() && Main.hardMode)//spread
            {
                if (Main.tile[i + x, j + y].type == TileID.JungleGrass)
                {
                    //Main.NewText("Tile at: " + i + ", " + j + ". x/y: " + x + ", " + y + ". Placing at: " + (i + x) + ", " + (j + y));
                    WorldGen.PlaceTile(i + x, j + y, TileType<GrassJungleBloody>(), true, true);
                }
                else if (Main.tile[i + x, j + y].type == TileID.Mud)
                {
                    if (!Main.tileSolid[Main.tile[i + x + 1, j + y].type] || !Main.tileSolid[Main.tile[i + x - 1, j + y].type] || !Main.tileSolid[Main.tile[i + x, j + y + 1].type] || !Main.tileSolid[Main.tile[i + x, j + y - 1].type])
                    {
                        WorldGen.PlaceTile(i + x, j + y, TileType<GrassJungleBloody>(), true, true);
                    }
                }
                else if (Main.tile[i + x, j + y].type == TileID.Stone)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileID.Crimstone, true, true);
                }
                else if (Main.tile[i + x, j + y].type == TileID.Grass)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileID.FleshGrass, true, true);
                }
                else if (Main.tile[i + x, j + y].type == TileID.Sand)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileID.Crimsand, true, true);
                }
                else if (Main.tile[i + x, j + y].type == TileID.IceBlock)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileID.FleshIce, true, true);
                }
            }

            if (!Main.tile[i, j + 1].active() && Main.tile[i, j].slope() == 0)//vines (Maybe add the corruption thorns too?)
            {
                if (Main.rand.Next(5) == 0)
                {
                    WorldGen.PlaceTile(i, j + 1, TileType<VineJungleBloody>(), true);
                }
            }

            if (!Main.tile[i, j - 1].active() && Main.tile[i, j].slope() == 0)//grass
            {
                if (Main.rand.Next(5) == 0)
                {
                    WorldGen.PlaceTile(i, j - 1, TileType<TallgrassJungleBloody>(), true);
                }
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            effectOnly = true;
            WorldGen.PlaceTile(i, j, TileID.Mud, false, true);
        }

        /*public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.rand.Next(600) == 0 && !Main.tile[i, j + 1].active() && Main.tile[i, j].slope() == 0)
            {
                Dust.NewDustPerfect(new Vector2(i, j) * 16, mod.DustType("Bloody2"), new Vector2(0, 0.6f));
            }
        }*/
    }

    public class VineJungleBloody : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileType<GrassJungleBloody>(),
                TileType<VineJungleBloody>()
            };
            TileObjectData.addTile(Type);
            soundType = SoundID.Grass;
            dustType = 14;
            AddMapEntry(new Color(122, 38, 38));
        }

        public override void RandomUpdate(int i, int j)
        {
            if (!Main.tile[i, j + 1].active() && Main.tile[i, j - 9].type != Type)
            {
                if (Main.rand.Next(1) == 0)
                {
                    WorldGen.PlaceTile(i, j + 1, TileType<VineJungleBloody>(), true);
                }
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!Main.tile[i, j - 1].active())
            {
                WorldGen.KillTile(i, j, false, false, false);
                WorldGen.SquareTileFrame(i, j, true);
            }
            else if (Main.tile[i, j - 1].type != TileType<GrassJungleBloody>() && Main.tile[i, j - 1].type != TileType<VineJungleBloody>())
            {
                WorldGen.KillTile(i, j, false, false, false);
                WorldGen.SquareTileFrame(i, j, true);
            }
            return true;
        }
    }

    public class TallgrassJungleBloody : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.RandomStyleRange = 9;
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileType<GrassJungleBloody>()
            };
            TileObjectData.addTile(Type);
            soundType = SoundID.Grass;
            dustType = 14;
            AddMapEntry(new Color(122, 38, 38));
        }
    }
}