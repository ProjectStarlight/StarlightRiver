namespace StarlightRiver.Content.Dusts
{
	class BigFire : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return Color.White;
		}

		public override void OnSpawn(Dust dust)
		{
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 18, 24);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is null)
			{
				dust.position -= new Vector2(9, 9) * dust.scale;
				dust.customData = 1;
			}

			dust.position += dust.velocity;

			if (dust.alpha % 64 == 0)
				dust.frame.Y += 24;

			Lighting.AddLight(dust.position, new Color(255, 200, 50).ToVector3() * 0.5f);

			dust.alpha += 16;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
}