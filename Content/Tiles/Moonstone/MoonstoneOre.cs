using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StarlightRiver.Content.Tiles.Moonstone
{
    public class MoonstoneOre : ModTile
    {
        public override bool Autoload(ref string name, ref string texture) {
            texture = AssetDirectory.MoonstoneTile + name;
            return base.Autoload(ref name, ref texture); }

        public override void SetDefaults() =>
            this.QuickSet(0, DustType<Dusts.Electric>(), SoundID.Tink, new Color(156, 172, 177), ItemType<Items.Moonstone.MoonstoneOre>(), true, true, "Aluminum Ore");

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            //Utils.DrawBorderString(spriteBatch, temp.ToString(), (new Vector2(i + 12, j + 7 - (i % 4)) * 16) - Main.screenPosition, Color.White, 0.75f);

            bool a()//remove
            {
                Main.NewText("a");
                return true;
            }

            if (!Main.tile[i, j - 1].active())
            {
                Color overlayColor = new Color(0.12f, 0.135f, 0.23f, 0f) * (((float)Math.Sin(Main.GameUpdateCount * 0.02f) + 4) / 4);
                float heightScale = ((float)Math.Sin(Main.GameUpdateCount * 0.025f) / 8) + 1;

                bool emptyLeft;
                bool emptyRight;
                Texture2D midTex;
                float yOffsetLeft = 0;
                float yOffsetRight = 0;

                switch (Main.tile[i, j].slope())
                {
                    case 1:// '\' slope
                        Tile tileLeft0 = Main.tile[i - 1, j];
                        Tile tileRight0 = Main.tile[i + 1, j + 1];

                        emptyLeft = !((tileLeft0.active() && tileLeft0.type == Type && !Main.tile[i - 1, j - 1].active()) || 
                            (Main.tile[i - 1, j - 1].slope() == 1 && Main.tile[i - 1, j - 1].type == Type && !Main.tile[i - 1, j - 2].active()));

                        emptyRight = !tileRight0.active() || tileRight0.type != Type || tileRight0.slope() == 2 || Main.tile[i + 1, j].active();

                        midTex = GetTexture(AssetDirectory.MoonstoneTile + "GlowSlopeRight");
                        yOffsetLeft = 1f;
                        break;

                    case 2:// '/' slope
                        Tile tileLeft1 = Main.tile[i - 1, j + 1];
                        Tile tileRight1 = Main.tile[i + 1, j];

                        emptyLeft = !tileLeft1.active() || tileLeft1.type != Type || tileLeft1.slope() == 1 || Main.tile[i - 1, j].active();

                        emptyRight = !((tileRight1.active() && tileRight1.type == Type && !Main.tile[i + 1, j - 1].active()) || 
                            (Main.tile[i + 1, j - 1].slope() == 2 && Main.tile[i + 1, j - 1].type == Type && !Main.tile[i + 1, j - 2].active()));

                        midTex = GetTexture(AssetDirectory.MoonstoneTile + "GlowSlopeLeft");
                        yOffsetRight = 1f;
                        break;

                    default:
                        Tile tileLeft2 = Main.tile[i - 1, j];
                        Tile tileRight2 = Main.tile[i + 1, j];

                        emptyLeft = !((tileLeft2.active() && tileLeft2.type == Type && tileLeft2.slope() != 1 &&!Main.tile[i - 1, j - 1].active()) ||
                            (Main.tile[i - 1, j - 1].slope() == 1 && Main.tile[i - 1, j - 1].type == Type && !Main.tile[i - 1, j - 2].active()));

                        emptyRight = !((tileRight2.active() && tileRight2.type == Type && tileRight2.slope() != 2 && !Main.tile[i + 1, j - 1].active()) || 
                            (Main.tile[i + 1, j - 1].slope() == 2 && Main.tile[i + 1, j - 1].type == Type && !Main.tile[i + 1, j - 2].active()));

                        midTex = GetTexture(AssetDirectory.MoonstoneTile + "GlowMid");
                        break;
                }

                if (emptyLeft)
                    if (emptyRight) //solo
                        spriteBatch.Draw(GetTexture(AssetDirectory.MoonstoneTile + "GlowSolo"), (new Vector2(i + 12, j + 7.5f + yOffsetLeft + yOffsetRight) * 16) - Main.screenPosition, overlayColor);
                    else            //left
                        spriteBatch.Draw(GetTexture(AssetDirectory.MoonstoneTile + "GlowLeft"), (new Vector2(i + 12, j + 7.5f + yOffsetLeft) * 16) - Main.screenPosition, overlayColor);
                else if (emptyRight)//right
                    spriteBatch.Draw(GetTexture(AssetDirectory.MoonstoneTile + "GlowRight"), (new Vector2(i + 12, j + 7.5f + yOffsetRight) * 16) - Main.screenPosition, overlayColor);
                else                //both
                    spriteBatch.Draw(midTex, (new Vector2(i + 12, j + 7.5f) * 16) - Main.screenPosition, overlayColor);


                Texture2D glowLines = GetTexture(AssetDirectory.MoonstoneTile + "GlowLines");
                int realX = i * 16;
                int realY = (int)((j + yOffsetLeft + yOffsetRight) * 16);
                int realWidth = glowLines.Width - 1;//1 pixel offset since the texture has a empty row of pixels on the side, this is also accounted for elsewhere below
                Color drawColor = overlayColor * 0.3f;

                float val = ((Main.GameUpdateCount / 3 + realY) % realWidth);
                int offset = (int)(val + (realX % realWidth) - realWidth);

                spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 102 - (int)Main.screenPosition.Y, 16, glowLines.Height), new Rectangle(offset + 1, 0, 16, (int)(glowLines.Height * heightScale)), drawColor);

                if (offset < 0)
                {
                    int rectWidth = Math.Min(-offset, 16);
                    spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 102 - (int)Main.screenPosition.Y, rectWidth, glowLines.Height), new Rectangle(offset + 1 + realWidth, 0, rectWidth, (int)(glowLines.Height * heightScale)), drawColor);
                }
            }

            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Vector2 pos = new Vector2(i, j) * 16;
            Lighting.AddLight(pos, new Vector3(0.1f, 0.32f, 0.5f) * 0.35f);
            //Dust.NewDustDirect(pos, 16, 16, ModContent.DustType<Content.Dusts.MoonstoneShimmer>(), 0, 0, 0, Color.White, 0.05f);
            if(Main.rand.Next(50) == 0)
                if(!Main.tile[i, j - 1].active())
                    Dust.NewDustPerfect(pos + new Vector2(Main.rand.NextFloat(0, 16), Main.rand.NextFloat(-16, -8)), 
                        ModContent.DustType<Content.Dusts.MoonstoneShimmer>(), new Vector2(Main.rand.NextFloat(-0.02f, 0.02f), -Main.rand.NextFloat(0.05f, 0.18f)), 0, new Color(0.2f, 0.2f, 0.25f, 0f), Main.rand.NextFloat(0.25f, 0.5f));
        }

        public override void FloorVisuals(Player player) => 
            player.AddBuff(BuffType<Buffs.Overcharge>(), 120);
    }
}