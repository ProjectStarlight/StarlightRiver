using System;

namespace StarlightRiver.Content.Dusts
{
	public class Mist : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return lightColor * (0.01f + dust.alpha / 250f) * (float)Math.Sin(dust.fadeIn / 120f * 3.14f);
		}

		public override void OnSpawn(Dust dust)
		{
			dust.scale *= Main.rand.NextFloat(2.5f, 2.9f);
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.rotation = Main.rand.NextFloat(6.28f);
			dust.frame = new Rectangle(0, 0, 36, 36);
			dust.alpha = Main.rand.Next(15);
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			float rotVel = dust.velocity.Y / 40f * (dust.alpha > 7 ? -1 : 1);

			Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 18 * dust.scale;
			dust.scale *= 0.999f;
			Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation + rotVel) * 18 * dust.scale;

			dust.rotation += rotVel;
			dust.position += currentCenter - nextCenter;

			dust.fadeIn++;

			if (dust.fadeIn > 120)
				dust.active = false;
			return false;
		}
	}
}