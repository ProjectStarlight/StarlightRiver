using System;
using StarlightRiver.Core.Loaders;

namespace StarlightRiver.Content.Dusts
{
	public class LavaSpark : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Masks/GlowSoft";

		public override void OnSpawn(Dust dust)
		{
			//dust.noGravity = true;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 64, 64);
			dust.fadeIn = 0;

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(ShaderLoader.GetShader("GlowingDust"), "GlowingDustPass");
			dust.shader.UseColor(Color.Transparent);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * (float)Math.Sin(dust.fadeIn / 60f * 3.14f);
		}

		public override bool Update(Dust dust)
		{
			dust.fadeIn++;

			if (dust.customData is null)
			{
				dust.position -= Vector2.One * 32 * dust.scale;
				dust.customData = true;
			}

			dust.shader.UseColor(dust.color * (float)Math.Sin(dust.fadeIn / 60f * 3.14f));

			dust.position += dust.velocity;

			if (!dust.noGravity)
				dust.velocity.Y += 0.08f;

			dust.velocity.X *= 0.995f;
			dust.scale *= 0.99f;

			if (dust.fadeIn >= 60)
				dust.active = false;

			return false;
		}
	}
}