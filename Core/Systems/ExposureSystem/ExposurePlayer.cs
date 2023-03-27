using Terraria;
using Terraria.DataStructures;

namespace StarlightRiver.Core.Systems.ExposureSystem
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

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			damage = (int)(damage * (1f + exposureMult)) + exposureAdd;
			return base.ModifyHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
		}
	}
}