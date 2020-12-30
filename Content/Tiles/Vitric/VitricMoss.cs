using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Vitric
{
    internal class VitricMoss : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Dusts.GlassNoGravity>(), SoundID.Dig, new Color(190, 255, 245), ItemID.Eggnog);

            TileID.Sets.DrawsWalls[Type] = true;
            Main.tileMerge[Type][TileType<VitricSpike>()] = true;
            Main.tileMerge[Type][mod.TileType("AncientSandstone")] = true;
            Main.tileMerge[Type][mod.TileType("VitricSand")] = true;
            Main.tileMerge[Type][mod.TileType("VitricSoftSand")] = true;
            TileID.Sets.Grass[Type] = true;
            SetModCactus(new VitricCactus());
            AddMapEntry(new Color(172, 131, 105));
        }

        public override void RandomUpdate(int i, int j)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x1 = -1; x1 <= 1; x1++)
                {
                    int tileX = i + x1;
                    int tileY = j + y;
                    if (!WorldGen.InWorld(i, j, 0)) 
                        continue;
                    if (Main.tile[tileX, tileY].type == (ushort)mod.TileType("VitricSand") && Main.rand.Next(3) == 0)
                    {
                        Main.tile[tileX, tileY].type = (ushort)TileType<VitricMoss>();
                        WorldGen.SquareTileFrame(tileX, tileY, true);
                    }
                }
            }

            if (!Main.tile[i, j + 1].active() && Main.rand.Next(10) == 0)
                WorldGen.PlaceTile(i, j + 1, TileType<VitricVine>());
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!effectOnly)
            {
                fail = true;
                Main.tile[i, j].type = (ushort)mod.TileType("VitricSand");
                WorldGen.SquareTileFrame(i, j, true);
                Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, mod.DustType("Air3"), 0f, 0f, 0, new Color(121, 121, 121), 1f);
            }
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Texture2D moss = GetTexture(AssetDirectory.VitricTile + "VitricMoss_Glow");
            Tile t = Main.tile[i, j];
            Color col = Lighting.GetColor(i, j);
            Color realCol = new Color(((col.R / 255f) * 1.4f) + 0.1f, ((col.G / 255f) * 1.4f) + 0.1f, ((col.B / 255f) * 1.4f) + 0.1f);
            spriteBatch.Draw(moss, ((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition, new Rectangle(t.frameX, t.frameY, 16, 16), realCol, 0f, new Vector2(), 1f, SpriteEffects.None, 0f);
        }
    }
}