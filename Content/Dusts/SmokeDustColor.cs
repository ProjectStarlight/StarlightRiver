using StarlightRiver.Core.Systems.PixelationSystem;

namespace StarlightRiver.Content.Dusts
{
	public class SmokeDustColor : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "SmokeDustColor";
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.frame = new Rectangle(0, Main.rand.Next(2) * 32, 32, 32);
			dust.rotation = Main.rand.NextFloat(6.28f);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var gray = new Color(25, 25, 25);
			Color black = Color.Black;
			Color ret;

			if (dust.alpha < 120)
				ret = Color.Lerp(dust.color, gray, dust.alpha / 120f);
			else if (dust.alpha < 180)
				ret = Color.Lerp(gray, black, (dust.alpha - 120) / 60f);
			else
				ret = black;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			dust.velocity *= 0.98f;
			dust.velocity.X *= 0.95f;
			dust.color *= 0.98f;

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
			dust.rotation += 0.04f;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}

	public class SmokeDustColor_Alt : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "SmokeDustColor";
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, Main.rand.Next(2), 32, 32);
			dust.rotation = Main.rand.NextFloat(6.28f);
		}

		public override bool PreDraw(Dust dust)
		{
			Texture2D tex = Assets.Dusts.SmokeDustColor.Value;
			Texture2D bloomTex = Assets.Masks.GlowAlpha.Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, 
					dust.color with { A = 0 } * 0.25f * (1f - dust.alpha / 255f), 0f, bloomTex.Size() / 2f, dust.scale * 1.25f, 0f, 0f);
			});

			Rectangle frame = tex.Frame(verticalFrames: 3, frameY: dust.frame.Y);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, frame, dust.color * (1f - dust.alpha / 255f), dust.rotation, frame.Size() / 2f, dust.scale, 0f, 0f);

			return false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.velocity *= 0.98f;
			dust.velocity.X *= 0.98f;
			dust.velocity.Y -= 0.035f;

			dust.position += dust.velocity;

			dust.alpha += 3;
			dust.scale *= 1.015f;

			if (dust.alpha >= 255)
				dust.active = false;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.1f);

			return false;
		}
	}
}