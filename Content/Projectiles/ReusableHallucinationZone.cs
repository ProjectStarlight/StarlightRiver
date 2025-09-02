using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Items.Crimson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Projectiles
{
	internal class ReusableHallucinationZone : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public ref float Radius => ref Projectile.ai[0];
		public ref float Duration => ref Projectile.ai[1];
		public ref float Timer => ref Projectile.ai[2];

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawProjectileHallucinations;
		}

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 2;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;

			if (Timer < Duration)
				Projectile.timeLeft = 2;

			if (Timer < 20)
				Projectile.Opacity = Timer / 20f;

			if (Timer > Duration - 20)
				Projectile.Opacity = 1f - (Timer - (Duration - 20) / 20f);
		}

		private void DrawProjectileHallucinations(SpriteBatch batch)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					Texture2D tex = Assets.Misc.StarView.Value;
					batch.Draw(tex, proj.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * proj.Opacity, 0, tex.Size() / 2f, (proj.ModProjectile as ReusableHallucinationZone).Radius * proj.Opacity * 7f / tex.Width, 0, 0);
				}
			}
		}
	}
}