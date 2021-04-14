using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class AuroraIce : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/AuroraIce";
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, -1, -1, new Color(100, 255, 255), ItemType<AuroraIceItem>());
            Main.tileFrameImportant[Type] = true;

            Main.tileMerge[TileType<PermafrostIce>()][Type] = true;
            Main.tileMerge[Type][TileType<PermafrostIce>()] = true;
            TileID.Sets.DrawsWalls[Type] = true;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (!fail && tile.frameY < 4 * 18)
            {
                for (int k = 0; k < 3; k++)
                {
                    float off = i + j;
                    float time = Main.GameUpdateCount / 600f * 6.28f;

                    float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
                    float cos = (float)Math.Cos(time + off * 0.2f);
                    Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
                    if (color.R < 80) color.R = 80;
                    if (color.G < 80) color.G = 80;

                    Dust d = Dust.NewDustPerfect(new Vector2(i, j) * 16, DustType<Dusts.Crystal>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3), 0, color * Main.rand.NextFloat(0.7f, 0.9f), Main.rand.NextFloat(0.4f, 0.6f));
                    d.fadeIn = Main.rand.NextFloat(-0.1f, 0.1f);

                    Dust d2 = Dust.NewDustPerfect(new Vector2(i, j) * 16 + new Vector2(Main.rand.Next(16), Main.rand.Next(16)), DustType<Dusts.Aurora>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 100, color, 0);
                    d2.customData = Main.rand.NextFloat(0.25f, 0.5f);
                }

                Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact.SoundId, i * 16, j * 16, SoundID.DD2_WitherBeastCrystalImpact.Style, 0.2f, -0.8f);

                if (checkIce(i - 1, j) || checkIce(i, j - 1) || checkIce(i + 1, j) || checkIce(i, j + 1))
                {
                    tile.frameY += 4 * 18;
                    fail = true;
                }
                else
                {
                    tile.active(false);
                }

                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                        Tile tile2 = Framing.GetTileSafely(i + x, j + y);

                        if (tile2.active() && tile2.type == Type && tile2.frameY < 4 * 18)
                        {
                            WorldGen.KillTile(i + x, j + y);
                        }
                    }
            }
            else noItem = true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            //TODO: this is gross, change it later?
            if (checkIce(i - 1, j) || checkIce(i, j - 1) || checkIce(i + 1, j) || checkIce(i, j + 1))
                Framing.GetTileSafely(i, j).slope(0);

            return false;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (tile.frameY >= 4 * 18 || checkIce(i - 1, j) || checkIce(i, j - 1) || checkIce(i + 1, j) || checkIce(i, j + 1))
            {
                float offBack = (float)Math.Sin((i + j) * 0.2f) * 300 + (float)Math.Cos(j * 0.15f) * 200;

                float sinBack = (float)Math.Sin(StarlightWorld.rottime + offBack * 0.01f * 0.2f);
                float cosBack = (float)Math.Cos(StarlightWorld.rottime + offBack * 0.01f);
                Color colorBack = new Color(100 * (1 + sinBack) / 255f, 140 * (1 + cosBack) / 255f, 180 / 255f);
                Color light = Lighting.GetColor(i, j) * 0.68f; //why am I multiplying by a constant here? because terraria lighting falloff is consistent and this is close enough that it's visually indecipherable

                Color final = new Color(light.R + (int)(colorBack.R * 0.1f), light.G + (int)(colorBack.G * 0.1f), light.B + (int)(colorBack.B * 0.1f));

                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Permafrost/AuroraIceUnder"), (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.frameX, tile.frameY % (4 * 18), 16, 16), final);
            }

            if (tile.frameY >= 4 * 18) return;

            int off = i + j;
            float time = Main.GameUpdateCount / 600f * 6.28f;

            float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
            float cos = (float)Math.Cos(time + off * 0.2f);
            Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
            if (color.R < 80) color.R = 80;
            if (color.G < 80) color.G = 80;

            spriteBatch.Draw(Main.tileTexture[tile.type], (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.frameX, tile.frameY, 16, 16), color * 0.3f);

            spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Permafrost/AuroraIce"), (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.frameX, tile.frameY, 16, 16), Color.Lerp(color, Color.White, 0.2f) * 0.1f);
            spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Permafrost/AuroraIceGlow2"), (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.frameX, tile.frameY, 16, 16), Color.Lerp(color, Color.White, 0.4f) * 0.4f);
            spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Permafrost/AuroraIceGlow"), (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(tile.frameX, tile.frameY, 16, 16), Color.Lerp(color, Color.White, 0.7f) * 0.8f);

            Lighting.AddLight(new Vector2(i, j) * 16, color.ToVector3() * 0.35f);

            if (Main.rand.Next(24) == 0)
            {
                Dust d = Dust.NewDustPerfect(new Vector2(i, j) * 16 + new Vector2(Main.rand.Next(16), Main.rand.Next(16)), DustType<Dusts.Aurora>(), Vector2.Zero, 100, color, 0);
                d.customData = Main.rand.NextFloat(0.25f, 0.5f);
            }
        }

        bool checkIce(int x, int y) => Framing.GetTileSafely(x, y).type == TileType<PermafrostIce>();
    }
    //TODO: Move all this to a more sane place, im really tired tonight and cant be assed to put braincells into organizing this. Thanks in advance future me.
    class AuroraIceItem : QuickMaterial
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraIceItem";

        public AuroraIceItem() : base("Frozen Aurora Chunk", "A preserved piece of the night sky", 999, Item.sellPrice(0, 0, 5, 0), ItemRarityID.White) { }
    }

    class AuroraIceBar : QuickMaterial
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraIceBar";

        public AuroraIceBar() : base("Frozen Aurora Bar", "A preserved selection of the night sky", 99, Item.sellPrice(0, 0, 25, 0), ItemRarityID.Blue) { }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ItemType<AuroraIceItem>(), 3);
            r.AddTile(TileID.Furnaces);
            r.SetResult(this);
            r.AddRecipe();
        }
    }
}
