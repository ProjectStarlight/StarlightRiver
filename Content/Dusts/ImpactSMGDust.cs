using System;

namespace StarlightRiver.Content.Dusts
{
	//literally recolored coach gun dust
	public class ImpactSMGDust : ModDust
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
			var gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 80)
			{
				ret = Color.Lerp(new Color(255, 50, 50), new Color(150, 50, 50), dust.alpha / 80f);
			}
			else if (dust.alpha < 140)
			{
				ret = Color.Lerp(new Color(150, 50, 50), gray, (dust.alpha - 80) / 80f);
			}
			else
			{
				ret = gray;
			}

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (Math.Abs(dust.velocity.X) > 3)
				dust.velocity.X *= 0.85f;
			else
				dust.velocity.X *= 0.92f;

			if (dust.velocity.Y > -2)
				dust.velocity.Y -= 0.1f;
			else
				dust.velocity.Y *= 0.92f;

			if (dust.velocity.Y > 0)
				dust.velocity.Y *= 0.85f;
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

	public class ImpactSMGDustTwo : ModDust
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
			var gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 80)
			{
				ret = Color.Lerp(new Color(255, 50, 50), new Color(160, 50, 50), dust.alpha / 80f);
			}
			else if (dust.alpha < 140)
			{
				ret = Color.Lerp(new Color(160, 50, 50), gray, (dust.alpha - 80) / 80f);
			}
			else
			{
				ret = gray;
			}

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (Math.Abs(dust.velocity.X) > 3)
				dust.velocity.X *= 0.85f;
			else
				dust.velocity.X *= 0.92f;

			if (dust.velocity.Y > -2)
				dust.velocity.Y -= 0.1f;
			else
				dust.velocity.Y *= 0.92f;

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

	public class ImpactSMGDustThree : ModDust
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
			var gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 40)
				ret = Color.Lerp(new Color(255, 50, 50), new Color(160, 50, 50), dust.alpha / 40f);
			else if (dust.alpha < 80)
				ret = Color.Lerp(new Color(160, 50, 50), gray, (dust.alpha - 40) / 40f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.85f;
			else
				dust.velocity *= 0.92f;

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
}