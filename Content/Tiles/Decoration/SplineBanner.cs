using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Decoration
{
    public abstract class SplineBanner : ModTile
    {
        private readonly string texture;
        private readonly string glowTexture;

        protected SplineBanner(string texturePath, string glowTexturePath = "")
        {
            texture = texturePath;
            glowTexture = glowTexturePath;
        }

        public virtual void PostDrawSpline(SpriteBatch spriteBatch, int index, Vector2 pos, Vector2 drawpos, Color color, float colorMultiplier) { }

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return true;
        }

        public override void SetDefaults()
        {
            Main.tileBlockLight[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Vector2 endpoint = new Vector2(tile.frameX, tile.frameY); //I utilize the tile's frame to store the endpoint here to not have to use a TE

            if (endpoint != Vector2.Zero) DrawSpline(spriteBatch, i, j, endpoint * 16 + Vector2.One * 8, 1);
            else DrawSpline(spriteBatch, i, j, (Main.MouseWorld / 16).ToPoint16().ToVector2() * 16 + Vector2.One * 8, 0.2f);
        }

        private void DrawSpline(SpriteBatch spriteBatch, int i, int j, Vector2 end, float colorMult)
        {
            Texture2D tex = GetTexture(texture);
            Vector2 oldPos = new Vector2(i, j) * 16;
            float max = Vector2.Distance(end, new Vector2(i, j) * 16) / (tex.Size().Length() / 5);

            if (max != 0)
                for (int k = 0; k < max; k++)
                {
                    float off = -Math.Abs((i * 16 - end.X) / 6f) + (float)Math.Cos(StarlightWorld.rottime + i % 6) * 2;
                    float sin = (float)Math.Sin(StarlightWorld.rottime + i % 6);

                    Vector2 pos;
                    if (i < end.X / 16) pos = Vector2.CatmullRom(new Vector2(i + sin, j + off) * 16, end, new Vector2(i, j) * 16, new Vector2(i - sin, j + off) * 16, k / max);
                    else pos = Vector2.CatmullRom(new Vector2(i + sin, j + off) * 16, new Vector2(i, j) * 16, end, new Vector2(i - sin, j + off) * 16, k / max);

                    Color color = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) * colorMult;

                    pos += Helper.TileAdj * 16;
                    float rot = (pos - oldPos).ToRotation() + 1.57f;

                    if (k > 0)
                    {
                        spriteBatch.Draw(tex, pos - Main.screenPosition, new Rectangle(k % 3 * tex.Width / 3, 0, tex.Height, tex.Width / 3), color, rot, tex.Size() / 2, 1, 0, 0);

                        if (glowTexture != "")
                        {
                            Texture2D tex2 = GetTexture(glowTexture);
                            spriteBatch.Draw(tex2, pos - Main.screenPosition, new Rectangle(k % 3 * tex.Width / 3, 0, tex.Height, tex.Width / 3), Color.White * colorMult, rot, tex.Size() / 2, 1, 0, 0);
                        }

                        PostDrawSpline(spriteBatch, k, pos - Helper.TileAdj * 16, pos, color, colorMult);
                    }

                    oldPos = pos;
                }
        }
    }

    public abstract class SplineBannerItem : ModItem
    {
        private readonly int placeType;
        private readonly string name;

        private Tile target;

        public override string Texture => AssetDirectory.DecorationTile + Name;

        protected SplineBannerItem(string displayName, int place)
        {
            name = displayName;
            placeType = place;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(name);
            Tooltip.SetDefault("Left click to place a banner\nthen left click again to set its endpoint");
        }

        public override void SetDefaults()
        {
            item.useTime = 10;
            item.useAnimation = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
        }

        public override bool UseItem(Player player)
        {
            if (target != null)
            {
                target.frameX = (short)(Main.MouseWorld.X / 16);
                target.frameY = (short)(Main.MouseWorld.Y / 16);
                target = null;
            }
            else
            {
                WorldGen.PlaceTile((int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16), placeType);

                Tile t = Framing.GetTileSafely((int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16));
                if (t.type == placeType) target = t;
            }

            return true;
        }
    }
}
