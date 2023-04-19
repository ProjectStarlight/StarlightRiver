﻿namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class LavaSpew : ModDust
	{
		public override string Texture => AssetDirectory.VitricBoss + Name;

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return Color.White;
		}

		public override void OnSpawn(Dust dust)
		{
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, Main.rand.NextBool() ? 0 : 72, 80, 72);
		}

		public override bool Update(Dust dust)
		{
			if (dust.fadeIn == 0)
			{
				dust.rotation = dust.velocity.ToRotation() + MathHelper.PiOver2;
				dust.position -= new Vector2(40, 36).RotatedBy(dust.rotation);
			}

			dust.frame.X = (int)(dust.fadeIn / 30f * 8) * 80;

			dust.fadeIn++;

			Lighting.AddLight(dust.position, new Vector3(1, 0.6f, 0.1f) * 0.8f);

			if (dust.fadeIn > 30)
				dust.active = false;
			return false;
		}
	}
}