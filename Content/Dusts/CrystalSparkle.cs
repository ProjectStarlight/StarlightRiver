namespace StarlightRiver.Content.Dusts
{
	class CrystalSparkle : ModDust
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

			if (dust.alpha % 64 == 56)
				dust.frame.Y += 14;

			Lighting.AddLight(dust.position, Color.Cyan.ToVector3() * 0.02f);

			dust.alpha += 8;

			if (dust.alpha > 255)
				dust.active = false;
			return false;
		}
	}
}