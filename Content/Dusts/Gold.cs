using System;

namespace StarlightRiver.Content.Dusts
{
	public class GoldNoMovement : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Gold";

		public override void OnSpawn(Dust dust)
		{
			dust.velocity *= 0.3f;
			dust.noGravity = true;
			dust.noLight = false;
			dust.scale *= 3f;
			dust.color.R = 255;
			dust.color.G = 220;
			dust.color.B = 100;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			dust.rotation += 0.05f;

			dust.scale *= 0.97f;

			if (dust.scale < 0.2f)
				dust.active = false;

			return false;
		}
	}

	public class GoldWithMovement : GoldNoMovement
	{
		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.rotation += 0.05f;

			dust.scale *= 0.92f;

			if (dust.scale < 0.3f)
				dust.active = false;

			return false;
		}
	}

	public class GoldSlowFade : GoldNoMovement
	{
		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.fadeIn++;

			dust.color = Lighting.GetColor((dust.position / 16).ToPoint());

			dust.scale *= 0.999f;
			dust.alpha = 155 + (int)(dust.fadeIn > 300 ? (dust.fadeIn - 300) / 300 * 100 : (300 - dust.fadeIn) / 300 * 100);

			if (dust.fadeIn > 600)
				dust.active = false;

			return false;
		}
	}
}