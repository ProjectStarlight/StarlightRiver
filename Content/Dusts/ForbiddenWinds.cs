using StarlightRiver.Core.Loaders;
using System;

namespace StarlightRiver.Content.Dusts
{
	public class ForbiddenWindsCooldownDust : ModDust
	{
		private int timer = 0;

		public override string Texture => AssetDirectory.Dust + "Air";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			timer = 30;
			dust.color.R = 170;
			dust.color.G = 235;
			dust.color.B = 255;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			if (dust.customData is Player Player)
			{
				return dust.color * (1 - Vector2.Distance(dust.position, Player.Center) / 50f);
			}

			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is Player Player)
			{
				dust.rotation = Vector2.Distance(dust.position, Player.Center) * 0.1f;
				dust.position += dust.velocity;

				dust.velocity = Vector2.Normalize(dust.position - Player.Center) * -4;
				dust.scale *= 0.95f;
				timer--;
				if (timer == 0 || Vector2.Distance(dust.position, Player.Center) < 1)
				{
					dust.active = false;
				}
			}
			else
			{
				dust.velocity *= 0.95f;
			}

			return false;
		}
	}

	public class ForbiddenWindsTrail : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Masks/GlowSoft";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 64, 64);
			dust.position -= Vector2.One * 32;

			if (ShaderLoader.GetShader("GlowingDust").Value != null)
				dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(ShaderLoader.GetShader("GlowingDust"), "GlowingDustPass");
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.fadeIn <= 0 ? new Color(120, 255, 255) * (dust.alpha / 255f) : Color.Transparent;
		}

		public override bool Update(Dust dust)
		{
			dust.scale = (2f - Math.Abs(dust.fadeIn) / 30f) * 0.15f;
			Vector2 currentCenter = dust.position + Vector2.One * 32 * dust.scale;
			dust.fadeIn -= 3;
			dust.scale = (2f - Math.Abs(dust.fadeIn) / 30f) * 0.15f;
			Vector2 nextCenter = dust.position + Vector2.One * 32 * dust.scale;

			dust.position += currentCenter - nextCenter;

			dust.alpha = 150 - (int)(Math.Abs(dust.fadeIn) / 60f * 150);
			dust.position += dust.velocity;
			dust.velocity *= 0.9f;

			dust.color = dust.fadeIn <= 0 ? new Color(100, 220, 255) * (dust.alpha / 255f) : Color.Transparent;
			dust.shader?.UseColor(dust.color);

			if (dust.fadeIn <= -60)
				dust.active = false;
			return false;
		}
	}
}