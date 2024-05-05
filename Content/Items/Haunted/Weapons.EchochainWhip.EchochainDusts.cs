namespace StarlightRiver.Content.Items.Haunted
{
	public class EchochainBurstDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
			dust.scale *= 0.045f;
			dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			dust.customData = dust.scale;
		}

		public override bool Update(Dust dust)
		{
			dust.alpha += 15;
			dust.scale += 0.01f;
			dust.scale *= 1.01f;
			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			float? originalScale = dust.customData as float?;

			float scale = MathHelper.Lerp(originalScale.Value, originalScale.Value * 5f, EaseBuilder.EaseCircularInOut.Ease(1f - lerper));

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Dust + Name).Value;
			Texture2D bloomTex = Assets.Keys.GlowAlpha.Value;

			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(135, 255, 10, 0) * 0.25f * lerper, 0f, bloomTex.Size() / 2f, scale * 20f, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, new Color(135, 255, 10, 0) * 0.75f * lerper, dust.rotation, tex.Size() / 2f, scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, new Color(90, 255, 130, 0) * lerper, dust.rotation, tex.Size() / 2f, scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, new Color(255, 255, 150, 0) * lerper, dust.rotation, tex.Size() / 2f, scale, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(90, 255, 130, 0) * 0.25f * lerper, 0f, bloomTex.Size() / 2f, scale * 20f, 0f, 0f);

			return false;
		}
	}

	public class EchochainChainDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
			dust.customData = dust.scale;
		}

		public override bool Update(Dust dust)
		{
			dust.alpha += 12;
			dust.rotation = dust.velocity.ToRotation() + MathHelper.ToRadians(90f);
			dust.position += dust.velocity;
			dust.velocity *= 0.98f;
			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			float? originalScale = dust.customData as float?;

			float scale = MathHelper.Lerp(originalScale.Value, originalScale.Value * 1.25f, EaseBuilder.EaseCircularInOut.Ease(1f - lerper));

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Dust + Name).Value;
			Texture2D bloomTex = Assets.Keys.GlowAlpha.Value;

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White * lerper, dust.rotation, tex.Size() / 2f, scale, 0f, 0f);
			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(130, 255, 50, 0) * 0.5f * lerper, 0f, bloomTex.Size() / 2f, scale * 0.5f, 0f, 0f);

			return false;
		}
	}
}