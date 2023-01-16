namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberProj_White : LightsaberProj
	{
		protected override Vector3 BladeColor => new Color(200, 200, 255).ToVector3();

		private bool spawnedSecond = false;

		protected override void RightClickBehavior()
		{
			Owner.GetModPlayer<LightsaberPlayer>().whiteCooldown = 1200;

			if (UneasedProgress > 0.5f && !spawnedSecond)
			{
				var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Owner.Center, Vector2.Zero, ModContent.ProjectileType<LightsaberProj_White>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI);
				(proj.ModProjectile as LightsaberProj_White).frontHand = false;
				(proj.ModProjectile as LightsaberProj_White).spawnedSecond = true;
				(proj.ModProjectile as LightsaberProj_White).rightClicked = true;
				spawnedSecond = true;
			}

			hide = false;
			canHit = true;

			if (thrown)
				ThrownBehavior();
			else
				HeldBehavior();

			if (Projectile.ai[0] >= 1 && Main.mouseRight)
				Projectile.ai[0] = 0;
		}
	}
}