using System;

namespace StarlightRiver.Content.Dusts
{
	public class Star : ModDust
	{
		float GetProgress(Dust dust)
		{
			return dust.fadeIn / 45f - (float)Math.Pow(dust.fadeIn, 2) / 8100f;
		}

		public override string Texture => AssetDirectory.Dust + Name;

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			dust.fadeIn = 180;
			dust.frame = new Rectangle(0, 0, 2, 2);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			int off = (int)dust.fadeIn / 4;
			float sin2 = (float)Math.Sin(StarlightWorld.visualTimer + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(StarlightWorld.visualTimer + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			return color * GetProgress(dust) * 0.4f;
		}

		public override bool Update(Dust dust)
		{
			dust.fadeIn--;

			dust.rotation += Main.rand.NextFloat(0.15f);
			dust.scale = GetProgress(dust);

			if (dust.fadeIn <= 0)
				dust.active = false;

			if (dust.dustIndex % 5 == 0)
			{
				for (int k = 0; k < 8; k++)
					Lighting.AddLight(dust.position + new Vector2(5 * k, 50 * k), GetAlpha(dust, Color.White).Value.ToVector3() * 0.5f);
			}

			return false;
		}
	}
}