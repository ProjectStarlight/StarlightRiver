namespace StarlightRiver.Content.Bosses.SquidBoss
{
	internal class AuroraWaterSplash : ModProjectile
	{
		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override void SetDefaults()
		{
			Projectile.width = 72;
			Projectile.height = 106;
			Projectile.timeLeft = 40;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}
}