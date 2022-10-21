using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Geomancer
{
	class GeoRainbowDust : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Keys/GlowVerySoft";

		public override void OnSpawn(Dust dust)
		{
			dust.color = Color.Transparent;
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 64, 64);
			dust.velocity *= 2;
			dust.shader = new ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
			dust.alpha = Main.rand.Next(100);
		}

		public override bool Update(Dust dust)
		{
			if (dust.color == Color.Transparent)
				dust.position -= Vector2.One * 32 * dust.scale;

			//dust.rotation += dust.velocity.Y * 0.1f;
			dust.position += dust.velocity;

			dust.velocity *= 0.98f;

			dust.color = Main.hslToRgb((dust.alpha / 100f + (float)Main.timeForVisualEffects * 0.02f) % 1f, 1, 0.5f);
			dust.shader.UseColor(dust.color);

			dust.fadeIn++;

			dust.scale *= 0.96f;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.4f * dust.scale);

			if (dust.fadeIn > 70)
				dust.active = false;

			return false;
		}
	}
}