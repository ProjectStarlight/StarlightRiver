using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Misc
{
    public class FiftyFiveLeafClover : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public FiftyFiveLeafClover() : base("Fifty Five Leaf Clover", "Critical strike chance increases up to 20% over 10 seconds\nThis effect will reset when you take damage") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreHurtEvent += PreHurtAccessory;
            StarlightPlayer.ResetEffectsEvent += ResetEffectsAccessory;

            return true;
        }

        public override void SafeUpdateEquip(Player player)
        {
            StarlightPlayer starlightPlayer = player.GetModPlayer<StarlightPlayer>();

            starlightPlayer.FiftyFiveLeafClover = (int)MathHelper.Clamp(starlightPlayer.FiftyFiveLeafClover + 1, 0, 600);

            float progress = starlightPlayer.FiftyFiveLeafClover / 600f;

            player.BoostAllDamage(0, (int)(progress * 20));
        }

        private void ResetEffectsAccessory(StarlightPlayer starlightPlayer)
        {
            if (!Equipped(starlightPlayer.player))
            {
                starlightPlayer.FiftyFiveLeafClover = 0;
            }
        }

        private bool PreHurtAccessory(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            player.GetModPlayer<StarlightPlayer>().FiftyFiveLeafClover = 0;

            return true;
        }
    }
}