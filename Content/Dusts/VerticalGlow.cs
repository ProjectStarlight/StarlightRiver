using System;

namespace StarlightRiver.Content.Dusts
{
	class VerticalGlow : ModDust
	{
		public override string Texture => AssetDirectory.MoonstoneTile + "GlowShort";
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			dust.fadeIn = 600;
			dust.frame = new Rectangle(0, 0, 35, 51);
			dust.scale = 0;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var col = Vector3.Lerp(dust.color.ToVector3(), Color.White.ToVector3(), dust.scale / 0.5f);
			return new Color(col.X, col.Y, col.Z, 0) * ((255 - dust.alpha) / 255f) * 0.45f;
		}

		public override bool Update(Dust dust)
		{
			Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 0.2f);

			var scalePoint = new Vector2(11.66f, 36);

			Vector2 currentCenter = scalePoint * dust.scale;
			float fade = dust.fadeIn / 10;
			dust.scale = (fade / 15f - (float)Math.Pow(fade, 2) / 900f) * 1.7f;//last number is the dust scale
			Vector2 nextCenter = scalePoint * dust.scale;

			dust.position += currentCenter - nextCenter;

			dust.fadeIn--;
			dust.position += dust.velocity * 0.25f;

			if (dust.fadeIn <= 0)
				dust.active = false;
			return false;
		}
	}
}
