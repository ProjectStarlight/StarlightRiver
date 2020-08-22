using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.NPCs;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{

    internal class VitricBookSpikeTrap : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 52;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 500;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.magic = true;
            projectile.penetrate = -1;
            projectile.localNPCHitCooldown = 1;
            projectile.usesLocalNPCImmunity = true;

        }
        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/BossSpike";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Spike Trap");
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            Rectangle rect = new Rectangle(0, 0, tex.Width, (int)projectile.localAI[0]);
            spriteBatch.Draw(tex, (projectile.Center + new Vector2(0, 12 + (tex.Height / 2))) - Main.screenPosition - new Vector2(0, projectile.localAI[0]), rect, lightColor, projectile.rotation, new Vector2(tex.Width / 2f, tex.Height / 2), projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool CanDamage()
        {
            return projectile.ai[0] == 0;
        }
        public override void AI()
        {
            projectile.ai[0] -= 1;
            projectile.localAI[1] -= 1;

            if (projectile.localAI[1] > 0)
                projectile.localAI[0] += 8;

            if (projectile.localAI[0] > 10)
                projectile.timeLeft = Math.Max(projectile.timeLeft, 1);

            projectile.localAI[0] = MathHelper.Clamp(projectile.localAI[0] - 4, 10, 36);

            if (projectile.ai[0] > 10)
                SpikeUp();
            if (projectile.ai[0] < -40)
            {
                for (int zz = 0; zz < Main.maxNPCs; zz += 1)
                {
                    NPC npc = Main.npc[zz];
                    if (!npc.dontTakeDamage && !npc.townNPC && npc.active && npc.life > 0)
                    {
                        Rectangle rech = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
                        Rectangle rech2 = new Rectangle((int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height);
                        if (rech.Intersects(rech2))
                        {
                            SpikeUp();
                            break;
                        }
                    }
                }
            }
        }
        public override void Kill(int timeLeft)
        {
            for(int i=-3;i<7;i+=1)
            Dust.NewDust(new Vector2(projectile.Center.X, projectile.Center.Y+i), projectile.width, 0, mod.DustType("VitricDust"), 0f, 0f, 75, default(Color), 0.85f);
        }
        private void SpikeUp()
        {
            Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 24, 0.75f, 0.5f);
            projectile.ai[0] = 10;
            projectile.localAI[1] = 20;
            projectile.netUpdate = true;
        }
    }

    internal class VitricBookProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 10;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;

        }
        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/BossSpike";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Book 1");
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            return false;
        }

        public override bool CanDamage()
        {
            return false;
        }

        public override void AI()
        {
            for (float num315 = 0.2f; num315 < 6; num315 += 0.25f)
            {
                float angle = projectile.velocity.ToRotation()+MathHelper.ToRadians(Main.rand.Next(40, 140));
                Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315;
                int num316 = Dust.NewDust(new Vector2(projectile.Center.X, projectile.Center.Y+Main.rand.Next(-30,90)), projectile.width, projectile.height, mod.DustType("VitricDust"), 0f, 0f, 50, default(Color), ((10f - num315) / 5f)*(float)projectile.timeLeft/20f);
                Main.dust[num316].noGravity = true;
                Main.dust[num316].velocity = vecangle;
                Main.dust[num316].fadeIn = 0.5f;
            }

            projectile.ai[0] += 1;
            if (projectile.ai[0]%2==0)
            Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, 0, 2, ModContent.ProjectileType<VitricBookProjectiletilecheck>(), projectile.damage, projectile.knockBack, projectile.owner);
        }
    }

    internal class VitricBookProjectiletilecheck : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 40;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.extraUpdates = 40;

        }
        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/BossSpike";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Book 2");
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
            fallThrough = false;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
           return false;
        }

        public override bool CanDamage()
        {
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!Collision.CanHit(projectile.Center, 4, 4, projectile.Center - new Vector2(0,24), 0, 0))
                return true;

            for (int zz = 0; zz < Main.maxProjectiles; zz += 1)
            {
                Projectile proj = Main.projectile[zz];
                if (proj.active && proj.type==ModContent.ProjectileType<VitricBookSpikeTrap>())
                {
                    if (proj.Distance(projectile.Center) < 24)
                        return true;
                }
            }

            for (float num315 = 0.2f; num315 < 8; num315 += 0.25f)
            {
                float angle = MathHelper.ToRadians(-Main.rand.Next(70, 120));
                Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315;
                int num316 = Dust.NewDust(new Vector2(projectile.Center.X, projectile.Center.Y), projectile.width, projectile.height, mod.DustType("VitricDust"), 0f, 0f, 50, default(Color), (10f - num315) / 5f);
                Main.dust[num316].noGravity = true;
                Main.dust[num316].velocity = vecangle;
                Main.dust[num316].fadeIn = 0.5f;
            }

            Projectile.NewProjectile(projectile.Center.X, projectile.Center.Y, 0, 0, ModContent.ProjectileType<VitricBookSpikeTrap>(), projectile.damage, 0, projectile.owner,12);

            return false;
        }
    }

}