using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class VitricStaffProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 28;
            projectile.height = 28;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 600;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 drawOrigin = new Vector2(projectile.width / 2, projectile.height / 2);
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, projectile.gfxOffY);
                Color color = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - k) / projectile.oldPos.Length);
                spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, color, projectile.rotation, drawOrigin, projectile.scale - (k * 0.15f), SpriteEffects.None, 0f);
            }
            return true;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            for (int k = 0; k < Main.projectile.Length; k++)
            {
                if (Main.projectile[k].active)
                {
                    if (Main.projectile[k].type == projectile.type)
                    {
                        if (Main.projectile[k].ai[0] == 1 && Main.projectile[k].ai[1] == target.whoAmI)
                        {
                            damage++;
                        }
                    }
                }
            }
            projectile.penetrate = -1;
            projectile.timeLeft = 180;
            projectile.ai[0] = 1;
            projectile.ai[1] = target.whoAmI;
            offset = target.position - projectile.position;
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override bool? CanHitNPC(NPC target)
        {
            return projectile.ai[0] == 1 ? false : base.CanHitNPC(target);
        }

        private Vector2 offset;

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + 1.57079637f;
            if (projectile.ai[0] == 1)
            {
                projectile.ignoreWater = true;
                projectile.tileCollide = false;
                NPC stuckTarget = Main.npc[(int)projectile.ai[1]];
                if (stuckTarget.active && !stuckTarget.dontTakeDamage)
                {
                    projectile.position = stuckTarget.position - offset;
                    projectile.gfxOffY = stuckTarget.gfxOffY;
                }
            }
        }
    }
}