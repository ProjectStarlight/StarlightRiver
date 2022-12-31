namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class PowerupDust : ModDust
	{
		public override string Texture => AssetDirectory.Keys + "GlowVerySoft";

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			float curveOut = Curve(1 - dust.fadeIn / 40f);
			var color = Color.Lerp(dust.color, new Color(255, 100, 0), dust.fadeIn / 30f);
			dust.color = color * (curveOut + 0.4f);
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
			dust.frame = new Rectangle(0, 0, 64, 64);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			if (dust.color == Color.Transparent)
				dust.position -= Vector2.One * 32 * dust.scale;

			//dust.rotation += dust.velocity.Y * 0.1f;
			dust.position += dust.velocity;

			dust.shader.UseColor(dust.color);

			dust.fadeIn++;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

			if (dust.fadeIn > 40)
				dust.active = false;
			return false;
		}
	}
}
