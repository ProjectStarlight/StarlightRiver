using System;

namespace StarlightRiver.Content.Dusts
{
	public static class SkullBusterDustColors
	{
		public static Color first = Color.Aqua;
		public static Color second = new(120, 130, 255);
		public static Color third = new(25, 25, 25);
	}
	public class SkullbusterDust : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = SkullBusterDustColors.third;
			Color ret;
			if (dust.alpha < 80)
				ret = Color.Lerp(SkullBusterDustColors.first, SkullBusterDustColors.second, dust.alpha / 80f);
			else if (dust.alpha < 180)
				ret = Color.Lerp(SkullBusterDustColors.second, gray, (dust.alpha - 80) / 100f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (Math.Abs(dust.velocity.X) > 3)
				dust.velocity.X *= 0.9f;
			else
				dust.velocity.X *= 0.95f;

			if (dust.velocity.Y > -2)
				dust.velocity.Y -= 0.05f;
			else
				dust.velocity.Y *= 0.95f;

			if (dust.velocity.Y > 0)
				dust.velocity.Y *= 0.92f;

			if (dust.alpha > 100)
			{
				dust.scale += 0.01f;
				dust.alpha += 2;
			}
			else
			{
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
				dust.scale *= 0.985f;
				dust.alpha += 4;
			}

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}

	public class SkullbusterDustShrinking : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = SkullBusterDustColors.third;
			Color ret;
			if (dust.alpha < 80)
				ret = Color.Lerp(SkullBusterDustColors.first, SkullBusterDustColors.second, dust.alpha / 80f);
			else if (dust.alpha < 180)
				ret = Color.Lerp(SkullBusterDustColors.second, gray, (dust.alpha - 80) / 100f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (Math.Abs(dust.velocity.X) > 3)
				dust.velocity.X *= 0.9f;
			else
				dust.velocity.X *= 0.95f;

			if (dust.velocity.Y > -2)
				dust.velocity.Y -= 0.05f;
			else
				dust.velocity.Y *= 0.95f;

			if (dust.velocity.Y > 3.5f)
				dust.velocity.Y = 3.5f;

			if (dust.alpha > 100)
			{
				dust.scale *= 0.975f;
				dust.alpha += 2;
			}
			else
			{
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
				dust.scale *= 0.985f;
				dust.alpha += 4;
			}

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}

	public class SkullbusterDustFastFade : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = SkullBusterDustColors.third;
			Color ret;
			if (dust.alpha < 40)
				ret = Color.Lerp(SkullBusterDustColors.first, SkullBusterDustColors.second, dust.alpha / 40f);
			else if (dust.alpha < 80)
				ret = Color.Lerp(SkullBusterDustColors.second, gray, (dust.alpha - 40) / 40f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.9f;
			else
				dust.velocity *= 0.95f;

			if (dust.alpha > 60)
			{
				dust.scale += 0.01f;
				dust.alpha += 6;
			}
			else
			{
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
				dust.scale *= 0.985f;
				dust.alpha += 4;
			}

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}

	class SkullbusterDustGlow : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Keys/GlowVerySoft";

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			float curveOut = Curve(1 - dust.fadeIn / 40f);
			var color = Color.Lerp(dust.color, SkullBusterDustColors.second, dust.fadeIn / 30f);
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
			dust.velocity *= 2;
			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			if (dust.color == Color.Transparent)
				dust.position -= Vector2.One * 32 * dust.scale;

			//dust.rotation += dust.velocity.Y * 0.1f;
			dust.position += dust.velocity;
			dust.velocity *= 0.95f;
			dust.shader.UseColor(dust.color);
			dust.scale *= 0.97f;
			dust.fadeIn++;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

			if (dust.fadeIn > 40)
				dust.active = false;

			return false;
		}
	}

	public class SkullbusterDustFastSlowdown : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = SkullBusterDustColors.third;
			Color ret;
			if (dust.alpha < 80)
				ret = Color.Lerp(SkullBusterDustColors.first, SkullBusterDustColors.second, dust.alpha / 80f);
			else if (dust.alpha < 140)
				ret = Color.Lerp(SkullBusterDustColors.second, gray, (dust.alpha - 80) / 80f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (Math.Abs(dust.velocity.X) > 7)
				dust.velocity.X *= 0.85f;
			else
				dust.velocity.X *= 0.92f;

			if (dust.velocity.Y > -2)
				dust.velocity.Y -= 0.05f;

			if (dust.velocity.Y > 3.5f)
				dust.velocity.Y = 3.5f;
			else
				dust.velocity.Y *= 0.95f;

			if (dust.alpha > 100)
			{
				dust.scale += 0.01f;
				dust.alpha += 2;
			}
			else
			{
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);
				dust.scale *= 0.985f;
				dust.alpha += 4;
			}

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
}