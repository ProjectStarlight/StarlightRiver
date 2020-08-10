using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
	public class VitricYoyoProj : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Vitric Yoyo");
			Main.projFrames[projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Valor);
			projectile.damage = 104;
			projectile.width = projectile.height = 18;
			aiType = ProjectileID.Valor;
		}
		//projectile.ai[1] = shatter timer
		int shattertimer = 0;
		public override void AI()
		{
			if (shattertimer > 0)
			{
				shattertimer--;
				projectile.frame = 1;
				projectile.friendly = false;
			}
			else
			{
				projectile.friendly = true;
				projectile.frame = 0;
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (shattertimer == 0)
			{
				shattertimer = 60;
				for (int i = 0; i < 3; i ++)
				{
					Projectile.NewProjectile(projectile.position, Vector2.Zero, ModContent.ProjectileType<VitricYoyoShard>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.whoAmI, i * 120);
				}
				for (int j = 0; j < 20; j++)
				{
					Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.Glass2>());
				}
			}
		}
	}
}