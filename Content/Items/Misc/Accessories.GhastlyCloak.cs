using StarlightRiver.Buffs;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Accessories
{
    public class GhastlyCloak : SmartAccessory
    {
        public GhastlyCloak() : base("Ghastly Cloak", "Avoiding damage cloaks you, increasing most stats.") { }
        public override void SafeUpdateEquip(Player player)
        {
            StarlightPlayer modplayer = player.GetModPlayer<StarlightPlayer>();
            if (modplayer.Timer - modplayer.LastHit >= 1200)
            {
                if (!player.HasBuff(BuffType<GhastlyCloakBuff>())) //activation thing
                {
                    Main.PlaySound(SoundID.Item123, player.position);
                    for (int i = 0; i <= 30; i++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, 62);
                    }
                }
                player.AddBuff(BuffType<GhastlyCloakBuff>(), 2, false);
            }
        }
    }
}