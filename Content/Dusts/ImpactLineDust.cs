using StarlightRiver.Core.Systems.PixelationSystem;

namespace StarlightRiver.Content.Dusts
{
	public class ImpactLineDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity *= 0.9f;
			dust.rotation = dust.velocity.ToRotation();

			dust.alpha += 10;

			dust.alpha = (int)(dust.alpha * 1.01f);

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.Dusts.ImpactLineDust.Value;

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * 0.25f * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);

			return false;
		}
	}

	public class ImpactLineDustGlow : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity *= 0.9f;
			dust.rotation = dust.velocity.ToRotation();

			dust.alpha += 10;

			dust.alpha = (int)(dust.alpha * 1.01f);

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.Dusts.ImpactLineDust.Value;

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);

			float glowScale = dust.scale * 0.75f;

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * lerper, dust.rotation, tex.Size() / 2f, new Vector2(glowScale * lerper, glowScale), 0f, 0f);

			return false;
		}
	}

	public class PixelatedImpactLineDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity *= 0.9f;
			dust.rotation = dust.velocity.ToRotation();

			dust.alpha += 10;

			dust.alpha = (int)(dust.alpha * 1.01f);

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Texture2D tex = Assets.Dusts.ImpactLineDust.Value;

				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);

				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * 0.25f * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);
			});

			return false;
		}
	}

	public class PixelatedImpactLineDustGlow : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity *= 0.9f;
			dust.rotation = dust.velocity.ToRotation();

			dust.alpha += 10;

			dust.alpha = (int)(dust.alpha * 1.01f);

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.Dusts.ImpactLineDust.Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);

				float glowScale = dust.scale * 0.75f;

				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * lerper, dust.rotation, tex.Size() / 2f, new Vector2(glowScale * lerper, glowScale), 0f, 0f);
			});

			return false;
		}
	}

	// these are starting to be a lot but this is a pixelated impact line dust which fades from one color to another
	public class PixelatedImpactLineDustGlowLerp : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity *= 0.9f;
			dust.rotation = dust.velocity.ToRotation();

			dust.alpha += 10;

			dust.alpha = (int)(dust.alpha * 1.01f);

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = Assets.Dusts.ImpactLineDust.Value;

			if (dust.customData is Color fadeColor)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
				{
					Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.Lerp(dust.color, fadeColor, 1f - lerper) * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);

					float glowScale = dust.scale * 0.75f;

					Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * lerper, dust.rotation, tex.Size() / 2f, new Vector2(glowScale * lerper, glowScale), 0f, 0f);
				});
			}

			return false;
		}
	}
}