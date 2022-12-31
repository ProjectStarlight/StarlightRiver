namespace StarlightRiver.Content.Dusts
{
	public class BreacherDust : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;
		public override void OnSpawn(Dust dust)
		{
			dust.noLight = false;
		}
		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.scale *= 0.98f;
			dust.velocity *= 0.98f;
			dust.alpha += 10;
			if (dust.alpha >= 255)
				dust.active = false;
			var color = new Color(255, 50, 180);
			Lighting.AddLight(dust.position, color.ToVector3() * 0.1f);
			return false;
		}
	}

	class BreacherDustStar : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Aurora";
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			dust.fadeIn = 60;
			dust.frame = new Rectangle(0, 0, 100, 100);

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

			dust.fadeIn--;
			dust.position += dust.velocity;
			dust.velocity *= 0.94f;

			dust.shader.UseColor(dust.color);
			dust.alpha += 10;
			if (dust.alpha >= 255)
				dust.active = false;
			return false;
		}
	}

	class BreacherDustLine : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "RoarLine";
		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			float curveOut = Curve(dust.fadeIn / 20f);
			var color = Color.Lerp(dust.color, Color.Cyan, dust.fadeIn / 30f);
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
			dust.frame = new Rectangle(0, 0, 8, 128);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			if (dust.color == Color.Transparent)
				dust.position -= new Vector2(4, 32) * dust.scale;

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.position += dust.velocity;
			dust.shader.UseColor(dust.color);
			dust.velocity *= 0.8f;
			dust.fadeIn++;
			dust.scale *= 0.98f;

			if (dust.fadeIn > 20)
				dust.active = false;
			return false;
		}
	}

	public class BreacherDustFastFade : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "BreacherDust";
		public override void OnSpawn(Dust dust)
		{
			dust.noLight = false;
		}
		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.scale *= 0.98f;
			dust.velocity *= 0.98f;
			dust.alpha += 20;
			if (dust.alpha >= 255)
				dust.active = false;
			var color = new Color(255, 50, 180);
			Lighting.AddLight(dust.position, color.ToVector3() * 0.1f);
			return false;
		}
	}
}