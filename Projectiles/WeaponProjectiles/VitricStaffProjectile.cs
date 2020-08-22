using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.NPCs;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class VitricStaffProjectile : ModProjectile
    {
        internal int maxImpale=6;
        internal int impaletime = 300;
        private float appear = 0f;
        private float scaleup = 0;
        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 600;
            projectile.magic = true;
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
            Color alphacol = projectile.GetAlpha(lightColor);
            if (projectile.ai[0] == 0)
            {
                for (int k = projectile.oldPos.Length - 1; k > 0; k -= 1)
                {
                    float sizer = (projectile.scale - (k * 0.15f)) * (1.00f);
                    if (sizer > 0)
                    {
                        Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, projectile.gfxOffY);
                        Color color = alphacol * ((float)(projectile.oldPos.Length - k) / projectile.oldPos.Length);
                        spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, color * appear, projectile.rotation, drawOrigin, sizer* scaleup, SpriteEffects.None, 0f);
                    }
                }
            }
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center-Main.screenPosition + new Vector2(0f, projectile.gfxOffY), null, alphacol* appear, projectile.rotation, drawOrigin, projectile.scale* scaleup, SpriteEffects.None, 0f);
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            int countin = 0;
            int first = -1;
            int firsttimeleft = 99990;
            for (int k = 0; k < Main.projectile.Length; k++)
            {
                if (Main.projectile[k].active)
                {
                    if (Main.projectile[k].type == projectile.type)
                    {
                        if (Main.projectile[k].ai[0] == 1 && Main.projectile[k].ai[1] == target.whoAmI)
                        {
                            if (Main.projectile[k].timeLeft < firsttimeleft)
                                {
                                firsttimeleft = Main.projectile[k].timeLeft;
                                first = k;
                                }
                            countin+=1;
                        }
                    }
                }
            }
            if (countin >= maxImpale && first>-1)
            {
                Main.projectile[first].Kill();
            }
            projectile.penetrate = -1;
            projectile.timeLeft = impaletime;
            projectile.ai[0] = 1;
            projectile.ai[1] = target.whoAmI;
            offset = target.position - projectile.position;
            projectile.netUpdate = true;
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 27, 0.75f);
            for (float num315 = 0.2f; num315 < 8; num315 += 0.25f)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(0, 360));
                Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle))* num315;
                int num316 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 3, 0f, 0f, 50, Color.Green, (10f- num315)/5f);
                Main.dust[num316].noGravity = true;
                Main.dust[num316].velocity = vecangle;
                Main.dust[num316].fadeIn = 0.5f;
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            return projectile.ai[0] == 1 ? false : base.CanHitNPC(target);
        }

        private Vector2 offset;

        public override void AI()
        {
            scaleup = MathHelper.Clamp(scaleup+0.10f, 0f, 1f);
            appear = Math.Min(appear + 0.05f, 1f);
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
                    stuckTarget.GetGlobalNPC<DebuffHandler>().impaled += 4;//Change it whatever you need
                }
                else
                {
                    projectile.ai[0] = 0;
                    projectile.ignoreWater = false;
                    projectile.tileCollide = true;
                    projectile.penetrate = 1;
                }
            }
        }
    }
}