using Terraria.DataStructures;

namespace StarlightRiver.Core
{
	public class ExposurePlayer : ModPlayer
	{
		public float exposureMult;
		public int exposureAdd; //not actually called exposure in tooltips but functionally it is indentical aside from being flat

		public override void ResetEffects()
		{
			exposureMult = 0f;
			exposureAdd = 0;
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
		{
			damage = (int)(damage * (1f + exposureMult)) + exposureAdd;
			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
		}
	}
}
