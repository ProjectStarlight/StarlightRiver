using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
    public class ExposurePlayer : ModPlayer
    {
        public float ExposureMult;
        public int ExposureAdd; //not actually called exposure in tooltips but functionally it is indentical aside from being flat

        public override void ResetEffects()
        {
            ExposureMult = 0f;
            ExposureAdd = 0;
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            damage = (int)(damage * (1f + ExposureMult)) + ExposureAdd;
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }
    }
}
