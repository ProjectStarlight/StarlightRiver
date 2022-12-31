namespace StarlightRiver.Content.Dusts
{
	class FireSparkle : ModDust
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
			dust.frame = new Rectangle(0, 0, 14, 14);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is null)
			{
				dust.position -= new Vector2(7, 7) * dust.scale;
				dust.customData = 1;
			}

			if (dust.alpha % 36 == 0)
				dust.frame.Y += 16;

			Lighting.AddLight(dust.position, new Color(255, 200, 50).ToVector3() * 0.5f);

			dust.alpha += 18;

			if (dust.alpha > 255)
				dust.active = false;
			return false;
		}
	}
}
