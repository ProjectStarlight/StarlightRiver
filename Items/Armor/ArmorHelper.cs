using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Armor
{
    public static class ArmorHelper
    {
        public static bool IsSetEquipped(this ModItem item, Player player) => item.IsArmorSet(player.armor[0], player.armor[1], player.armor[2]);
        public static void QuickDrawHelmet(PlayerDrawInfo info, string texture, Color color, float scale, Vector2 offset)
        {
            Texture2D tex = GetTexture(texture);
            Main.playerDrawData.Add(new DrawData(tex, info.position - Main.screenPosition + offset, null, color * ((255 - info.drawPlayer.immuneAlpha) / 255f), info.drawPlayer.headRotation, tex.Size() / 2, scale, info.spriteEffects, 0));
        }
    }
}
