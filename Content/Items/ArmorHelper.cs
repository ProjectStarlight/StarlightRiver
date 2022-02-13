using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Armor
{
	public static class ArmorHelper
    {
        public static bool IsSetEquipped(this ModItem item, Player player) => item.IsArmorSet(player.armor[0], player.armor[1], player.armor[2]);

        public static void QuickDrawArmor(PlayerDrawInfo info, string texture, Color color, float scale, Vector2 offset)
        {
            Texture2D tex = GetTexture(texture);
            Main.playerDrawData.Add(new DrawData(tex, (info.position - Main.screenPosition + offset).ToPoint16().ToVector2(), null, color * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f), info.drawPlayer.headRotation, tex.Size() * 0.5f, scale, info.spriteEffects, 0));
        }

        /// <summary>
        /// uses the player frame as the frame in the passed sheet, frame position is normalized to the passed sheet height.  
        /// is color is null immune fade is applied by default
        /// </summary>
        /// <param name="info"></param>
        /// <param name="texture"></param>
        /// <param name="scale"></param>
        /// <param name="offset"></param>
        /// <param name="color"></param>
        /// <param name="immuneFade"></param>
        public static void QuickDrawHeadFramed(PlayerDrawInfo info, string texture, float scale, Vector2 offset, Color? color = null, bool immuneFade = false)//TODO fix framing (uses leg frame since head is always zero)
        {
            Texture2D tex = GetTexture(texture);
            int frame = (int)(info.drawPlayer.legFrame.Y/*TODO*/ * 0.01785714286f);//(int)((frame / 1120f) * 20);
            Vector2 pos = ((info.position + info.drawPlayer.headPosition) - Main.screenPosition + offset).ToPoint16().ToVector2();
            int height = (int)(tex.Height * 0.05f);//tex.Height / 20
            Color drawColor = color ?? info.upperArmorColor;
            Main.playerDrawData.Add(new DrawData(tex, pos, new Rectangle(0, frame * height, tex.Width, height), immuneFade ? drawColor * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f) : drawColor, info.drawPlayer.headRotation, new Vector2(tex.Width * 0.5f, tex.Height * 0.025f), scale, info.spriteEffects, 0));
        }

        /// <summary>
        /// uses the player frame as the frame in the passed sheet, frame position is normalized to the passed sheet height.  
        /// is color is null immune fade is applied by default
        /// </summary>
        /// <param name="info"></param>
        /// <param name="texture"></param>
        /// <param name="scale"></param>
        /// <param name="offset"></param>
        /// <param name="color"></param>
        /// <param name="immuneFade"></param>
        public static void QuickDrawBodyFramed(PlayerDrawInfo info, string texture, float scale, Vector2 offset, Color? color = null, bool immuneFade = false)
        {
            Texture2D tex = GetTexture(texture);
            int frame = (int)(info.drawPlayer.bodyFrame.Y * 0.01785714286f);//(int)((frame / 1120f) * 20);
            Vector2 pos = ((info.position + info.drawPlayer.bodyPosition) - Main.screenPosition + offset).ToPoint16().ToVector2();
            int height = (int)(tex.Height * 0.05f);//tex.Height / 20
            Color drawColor = color ?? info.middleArmorColor;
            Main.playerDrawData.Add(new DrawData(tex, pos, new Rectangle(0, frame * height, tex.Width, height), immuneFade ? drawColor * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f) : drawColor, info.drawPlayer.bodyRotation, new Vector2(tex.Width * 0.5f, tex.Height * 0.025f), scale, info.spriteEffects, 0));
        }

        /// <summary>
        /// uses the player frame as the frame in the passed sheet, frame position is normalized to the passed sheet height.  
        /// is color is null immune fade is applied by default
        /// </summary>
        /// <param name="info"></param>
        /// <param name="texture"></param>
        /// <param name="scale"></param>
        /// <param name="offset"></param>
        /// <param name="color"></param>
        /// <param name="immuneFade"></param>
        public static void QuickDrawLegsFramed(PlayerDrawInfo info, string texture, float scale, Vector2 offset, Color? color = null, bool immuneFade = false)
        {
            Texture2D tex = GetTexture(texture);
            int frame = (int)(info.drawPlayer.legFrame.Y * 0.01785714286f);//(int)((frame / 1120f) * 20);
            Vector2 pos = ((info.position + info.drawPlayer.legPosition) - Main.screenPosition + offset).ToPoint16().ToVector2();
            int height = (int)(tex.Height * 0.05f);//tex.Height / 20
            Color drawColor = color ?? info.lowerArmorColor;
            Main.playerDrawData.Add(new DrawData(tex, pos, new Rectangle(0, frame * height, tex.Width, height), immuneFade ? drawColor * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f) : drawColor, info.drawPlayer.legRotation, new Vector2(tex.Width * 0.5f, tex.Height * 0.025f), scale, info.spriteEffects, 0));
        }
    }
}
