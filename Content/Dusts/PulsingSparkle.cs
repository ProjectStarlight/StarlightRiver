using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Dusts
{
	internal class PulsingSparkle : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Aurora";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			dust.fadeIn = 120;
			dust.frame = new Rectangle(0, 0, 100, 100);
			dust.scale = 0;

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
			dust.shader.UseColor(Color.Transparent);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var col = Vector3.Lerp(dust.color.ToVector3(), Color.White.ToVector3(), dust.scale / (dust.customData is null ? 0.5f : (float)dust.customData));
			return new Color(col.X, col.Y, col.Z) * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 2.5f);

			Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 50 * dust.scale;

			dust.scale = (float)(0.5f + 0.3f * Math.Sin(dust.fadeIn * 0.2f)) * (dust.fadeIn / 120f) * (dust.customData is null ? 0.5f : (float)dust.customData) * 0.3f;

			Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 50 * dust.scale;

			dust.position += currentCenter - nextCenter;

			dust.fadeIn--;
			dust.position += dust.velocity * 0.25f;

			dust.shader.UseColor(dust.color);

			if (dust.fadeIn <= 0)
				dust.active = false;
			return false;
		}
	}
}