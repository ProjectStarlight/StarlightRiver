using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.PixelationSystem;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class PowerupDust : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "ImpactLineDust";

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			float curveOut = Curve(1 - dust.fadeIn / 40f);
			var color = Color.Lerp(dust.color, new Color(255, 100, 0), dust.fadeIn / 30f);
			dust.color = color * (curveOut + 0.4f);
			dust.color.A = 0;
			return dust.color;
		}

		float Curve(float input) //shrug it works, just a cubic regression for a nice looking curve
		{
			return -2.65f + 19.196f * input - 32.143f * input * input + 15.625f * input * input * input;
		}

		public override void OnSpawn(Dust dust)
		{
			dust.color = Color.Transparent;
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.scale *= 0.3f;
			dust.frame = new Rectangle(0, 0, 512, 512);

			if (ShaderLoader.GetShader("GlowingDust").Value != null)
				dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(ShaderLoader.GetShader("GlowingDust"), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			//dust.rotation += dust.velocity.Y * 0.1f;
			dust.position += dust.velocity;

			dust.shader?.UseColor(dust.color);

			dust.fadeIn++;
			dust.rotation = dust.velocity.ToRotation();

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

			if (dust.fadeIn > 40)
				dust.active = false;
			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			if (dust.fadeIn > 30)
				return false;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Texture2D tex = Assets.Dusts.ImpactLineDust.Value;

				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, GetAlpha(dust, Color.White) ?? Color.White, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);
				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * 0.25f * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);
			});

			return false;
		}
	}
}