using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Items.Accessories.EarlyPreHardmode
{
    public class FiftyFiveLeafClover : SmartAccessory
    {
        public FiftyFiveLeafClover() : base("Fifty Five Leaf Clover", "Not taking damage gradually increases critical strike chance with a cap on +20% after 10 seconds\nResets after taking damage") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreHurtEvent += PreHurtAccessory;
            StarlightPlayer.ResetEffectsEvent += ResetEffectsAccessory;
            return true;
        }
        public override void SafeUpdateEquip(Player player)
        {
            StarlightPlayer slp = player.GetModPlayer<StarlightPlayer>();
            slp.FiftyFiveLeafClover = (short)MathHelper.Clamp(slp.FiftyFiveLeafClover + 1, 0, 600);
            Helper.BoostAllDamage(player, 0, (int)(((float)slp.FiftyFiveLeafClover / 600f) * 20f));
        }

        private void ResetEffectsAccessory(StarlightPlayer slp)
        {
            if (!Equipped(slp.player))
                slp.FiftyFiveLeafClover = 0;
        }

        private bool PreHurtAccessory(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            player.GetModPlayer<StarlightPlayer>().FiftyFiveLeafClover = 0;
            return true;
        }

    }
}