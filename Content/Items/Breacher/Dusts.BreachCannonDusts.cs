﻿namespace StarlightRiver.Content.Items.Breacher
{
	public class BreachImpactGlow : Dusts.Glow
	{
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 64, 64);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.85f;
			return base.Update(dust);
		}
	}

	class BreachImpactSpark : Dusts.BuzzSpark
	{
		public override void OnSpawn(Dust dust)
		{
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 5, 50);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/ShrinkingDust").Value), "ShrinkingDustPass");
		}

		public override bool Update(Dust dust)
		{
			dust.fadeIn++;
			return base.Update(dust);
		}
	}
}