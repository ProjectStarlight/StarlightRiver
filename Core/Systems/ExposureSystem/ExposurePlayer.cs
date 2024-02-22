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
			modifiers.FinalDamage *= 1 + exposureMult;
			modifiers.FinalDamage += exposureAdd;
		}
	}
}