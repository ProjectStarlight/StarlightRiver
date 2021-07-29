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
            Tile tile = Main.tile[i, j];
            if(!Main.tile[i, j - 1].active())
            {
                //spriteBatch.Draw(GetTexture(AssetDirectory.MoonstoneTile + "GlowMidSoft"), (new Vector2(i + 12, j + 8) * 16) - Main.screenPosition, new Color(0.5f, 0.5f, 0.5f, 0f));
                Texture2D glowLines = GetTexture(AssetDirectory.MoonstoneTile + "GlowLines");
                int realX = i * 16;
                int realY = j * 16;//a
                int realWidth = glowLines.Width - 1;//1 pixel offset since the texture has a empty row of pixels on the side, this is also accounted for elsewhere below
                Color drawColor = new Color(0.35f, 0.4f, 0.5f, 0f);

                float val = ((Main.GameUpdateCount / 3 + realY) % realWidth);
                int offset = (int)(val + (realX % realWidth) - realWidth);

                spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 128 - (int)Main.screenPosition.Y, 16, glowLines.Height), new Rectangle(offset + 1, 0, 16, glowLines.Height), drawColor);

                //Utils.DrawBorderString(spriteBatch, temp.ToString(), (new Vector2(i + 12, j + 7 - (i % 4)) * 16) - Main.screenPosition, Color.White, 0.75f);

                if (offset < 0)
                {
                    int rectWidth = Math.Min(-offset, 16);
                    spriteBatch.Draw(glowLines, new Rectangle(realX + 192 - (int)Main.screenPosition.X, realY + 128 - (int)Main.screenPosition.Y, rectWidth, glowLines.Height), new Rectangle(offset + 1 + realWidth, 0, rectWidth, glowLines.Height), drawColor);
                }
            }
            //TODO reimplement this without relying on the frame
            //else if (tile.frameY == 54 && tile.frameX < 91)
            //{
            //    if (tile.frameX % 36 == 0)
            //    {
            //        spriteBatch.Draw(GetTexture(AssetDirectory.MoonstoneTile + "GlowMidSoft"), (new Vector2(i + 12, j + 8) * 16) - Main.screenPosition, new Color(0.5f, 0.5f, 0.5f, 0f));

            //    }
            //    else
            //    {
            //        spriteBatch.Draw(GetTexture(AssetDirectory.MoonstoneTile + "GlowMidSoft"), (new Vector2(i + 12, j + 8) * 16) - Main.screenPosition, new Color(0.5f, 0.5f, 0.5f, 0f));

            //    }
            //}

            return true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Lighting.AddLight(new Vector2(i + 0.5f, j + 0.5f) * 16, new Vector3(0.1f, 0.32f, 0.5f) * 0.35f);

        }

        public override void FloorVisuals(Player player) => 
            player.AddBuff(BuffType<Buffs.Overcharge>(), 120);
    }
}