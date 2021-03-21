using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Items.Armor
{
    public static class ArmorHelper
    {
        public static bool IsSetEquipped(this ModItem item, Player player) => item.IsArmorSet(player.armor[0], player.armor[1], player.armor[2]);
        public static void QuickDrawHelmet(PlayerDrawInfo info, string texture, Color color, float scale, Vector2 offset)
        {
            Texture2D tex = GetTexture(texture);
            Main.playerDrawData.Add(new DrawData(tex, (info.position - Main.screenPosition + offset).ToPoint16().ToVector2(), null, color * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f), info.drawPlayer.headRotation, tex.Size() * 0.5f, scale, info.spriteEffects, 0));
        }
        /// <summary>
        /// version that uses the player frame, sheet must be the same as player sheet
        /// </summary>
        /// <param name="info"></param>
        /// <param name="texture"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        /// <param name="offset"></param>
        public static void QuickDrawHelmetFramed(PlayerDrawInfo info, string texture, Color color, float scale, Vector2 offset)
        {
            Main.NewText(info.drawPlayer.leftTimer);
            Texture2D tex = GetTexture(texture);
            int frame = (int)(info.drawPlayer.legFrame.Y/*TODO*/ * 0.01785714286f);//(int)((info.drawPlayer.legFrame.Y / 1120f) * 20);
            Vector2 pos = (info.position - Main.screenPosition + offset).ToPoint16().ToVector2();
            int height = (int)(tex.Height * 0.05f);//tex.Height / 20
            Main.playerDrawData.Add(new DrawData(tex, pos, new Rectangle(0, frame * height, tex.Width, height), color * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f), info.drawPlayer.headRotation, new Vector2(tex.Width * 0.5f, tex.Height * 0.025f), scale, info.spriteEffects, 0));
        }
    }
}
