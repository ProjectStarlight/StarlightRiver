using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
	public class VitricYoyoShard : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Yoyo");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
		}
		public override void SetDefaults()
		{
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.hostile = false;
			projectile.friendly = true;
			projectile.damage = 13;
			projectile.width = projectile.height = 11;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
			projectile.timeLeft = 60;

		}
		public override void AI()
		{
			Projectile parent = Main.projectile[(int)projectile.ai[0]];
			if (!parent.active)
			{
				projectile.active = false;
			}
				double dist = (900 - ((projectile.timeLeft - 30) * (projectile.timeLeft - 30))) / 11;
				double deg = (float)(dist * 3) + (float)(projectile.ai[1]);
				double rad = deg * (Math.PI / 180); //Convert degrees to radians

				projectile.position.Y = parent.Center.Y - (int)(Math.Sin(rad) * dist) - projectile.height / 2;
				projectile.position.X = parent.Center.X - (int)(Math.Cos(rad) * dist) - projectile.width / 2;

				projectile.rotation = (float)((dist * 9) + (projectile.ai[1])) * (float)(Math.PI / 180);
		}
	}
}