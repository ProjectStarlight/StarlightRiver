using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Dusts;
using System;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class ExplosiveFlare : ModProjectile
    {
		public override void SetDefaults()
		{
			projectile.width = 10;
			projectile.height = 16;

			projectile.ranged = true;
			projectile.friendly = true;
			projectile.aiStyle = 1;
			aiType = 163;
			projectile.penetrate = -1;
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Flare");
			Main.projFrames[projectile.type] = 2;
		}

		int enemyID;
		bool stuck = false;
		int explosionTimer = 100;
		Vector2 offset = Vector2.Zero;
		float sineAlpha = 0;
        public override bool PreAI()
		{
			if (stuck)
            {
				sineAlpha += 0.2f;
				NPC target = Main.npc[enemyID];
				projectile.position = target.position + offset;
				explosionTimer--;
				if (explosionTimer <= 0 || !target.active)
                {
					Explode(target);
                }
				return false;
            }
			else
            {
				projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
            }
			return base.PreAI();
		}
		private void Explode(NPC target)
        {
			double shrinkage = Main.rand.NextFloat(0f, 0.3f);
			for (double i = 2.7 + shrinkage; i < 3.5 - shrinkage; i += 0.1)
			{
				Projectile.NewProjectile(projectile.position + (Vector2.UnitY.RotatedBy(projectile.rotation + 3.14) * 15), Vector2.UnitY.RotatedBy(projectile.rotation + i) * 15, ModContent.ProjectileType<FlareExplosion>(), projectile.damage, 0, projectile.owner);
				for (int j = 0; j < 25; j++)
                {
					int dust = Dust.NewDust(projectile.position + (Vector2.UnitY.RotatedBy(projectile.rotation + 3.14) * 15), projectile.width, projectile.height, ModContent.DustType<BreacherDust>());
					Vector2 angle = Vector2.UnitY.RotatedBy(projectile.rotation + i + Main.rand.NextFloat(-0.2f, 0.2f));
					angle *= Main.rand.NextFloat(10f, 50f);
					Main.dust[dust].velocity.Y = angle.Y;
					Main.dust[dust].velocity.X = angle.X;
					Main.dust[dust].noGravity = true;
					Main.dust[dust].scale = 1.6f;
					Main.dust[dust].alpha = Main.rand.Next(120);
				}
			}
			Gore.NewGore(projectile.position, Vector2.UnitY.RotatedBy(projectile.rotation + 3.14f), mod.GetGoreSlot("Gores/FlareCasing"), 1f);
			projectile.active = false;
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (!stuck && target.life > 0)
			{
				stuck = true;
				projectile.friendly = false;
				projectile.tileCollide = false;
				enemyID = target.whoAmI;
				offset = projectile.position - target.position;
				offset -= projectile.velocity;
			}
		}
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.position - Main.screenPosition + new Vector2(projectile.width / 2, projectile.height / 2), new Rectangle(0, 0, projectile.width, projectile.height),
				lightColor, projectile.rotation, new Vector2(projectile.width / 2, projectile.height / 2), projectile.scale, SpriteEffects.None, 1);
			if (stuck)
            {
				float alpha = (float)(Math.Sin(sineAlpha) / 2) + 0.5f;
				Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.position - Main.screenPosition + new Vector2(projectile.width / 2, projectile.height / 2), new Rectangle(0, projectile.height, projectile.width, projectile.height),
				lightColor * alpha, projectile.rotation, new Vector2(projectile.width / 2, projectile.height / 2), projectile.scale, SpriteEffects.None, 1);
			}
			return false;
        }
    }
}