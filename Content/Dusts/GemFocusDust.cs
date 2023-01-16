namespace StarlightRiver.Content.Dusts
{
	public class GemFocusDust : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * (dust.alpha / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is Projectile)
			{
				dust.position = (dust.customData as Projectile).Center + dust.velocity;

				dust.scale *= 0.93f;
				dust.alpha = (int)((dust.customData as Projectile).alpha * dust.scale);
			}

			if (dust.scale < 0.2f)
				dust.active = false;
			return false;
		}
	}
}