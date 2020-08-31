using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.NPCs;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class VitricBowProjectile : ModProjectile,IDrawAdditive
    {
        internal int MaxCharge=100;
        internal int ChargeNeededToFire = 30;
        private float maxangle = 45f;
        private int MaxFireTime = 30;
        private int AddedFireBuffer = 15;
        
        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 600;
            projectile.ranged = true;
            projectile.arrow = true;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        //Kek
        public override string Texture => "StarlightRiver/NPCs/Boss/VitricBoss/VolleyTell";
        public override bool? CanHitNPC(NPC target) => false;
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }



        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 27, 0.75f);
            for (float num315 = 0.2f; num315 < 5; num315 += 0.25f)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(0, 360));
                Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle))* num315;
                int num316 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, mod.DustType("Glass3"), 0f, 0f, 50, default, (10f- num315)/5f);
                Main.dust[num316].noGravity = true;
                Main.dust[num316].velocity = vecangle/3f;
                Main.dust[num316].fadeIn = 0.5f;
            }
        }


		public override void AI()
		{

            //int DustID2 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, mod.DustType("AcidDust"), projectile.velocity.X * 1f, projectile.velocity.Y * 1f, 20, default(Color), 1f);

            Player player = projectile.Owner();
            if (player.dead)
			{
				projectile.Kill();
			}
			else
			{
				if (projectile.localAI[1] > 0 || !player.channel)
				{
                    LetGo(player);
				}
				else
				{
                    Charging(player);
				}
                Holding(player);
                projectile.Center = player.Center;
            }
		}

        public void Holding(Player player)
        {
            int dir = projectile.direction;
            player.ChangeDir(projectile.direction);
            player.itemTime = 3;
            player.itemAnimation = 3;
            player.heldProj = projectile.whoAmI;

            Vector2 distz = projectile.Center - player.Center;
            player.itemRotation = (float)Math.Atan2(distz.Y * dir, distz.X * dir);
        }

            public void Charging(Player player)
            {
                projectile.ai[0] = Math.Min(projectile.ai[0]+1, MaxCharge);
                Vector2 mousePos = Main.MouseWorld;
                if (projectile.owner == Main.myPlayer && mousePos != projectile.Center)
                {
                    Vector2 diff2 = mousePos - player.Center;
                    diff2.Normalize();
                    projectile.velocity = diff2 * 20f;
                    projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
                    projectile.netUpdate = true;
                }

                projectile.ai[1] = MaxFireTime-10;
                projectile.timeLeft = MaxFireTime+ AddedFireBuffer;
        }

        public void LetGo(Player player)
        {
            if (projectile.ai[0] > ChargeNeededToFire)
            {
                if (projectile.timeLeft <= MaxFireTime)
                {
                    projectile.localAI[1] += 1;
                    float percent = Math.Max((projectile.ai[0] - ChargeNeededToFire) / (MaxCharge-ChargeNeededToFire),0f);
                    int timeleft = projectile.timeLeft - 10;
                    float maxdelta = (maxangle * (projectile.ai[0] / MaxCharge));

                    for (int i = -1; i < 2; i += 2)
                    {
                        bool first = (projectile.timeLeft == MaxFireTime);
                        if ((projectile.localAI[1] % 8 == 0 && timeleft > 0) || (first && i > 0))
                        {
                            float rot = MathHelper.ToRadians(((1f-((timeleft) / projectile.ai[1])) * maxdelta / 2f)) * i;
                            float chargefloat = (1f - ((timeleft) / projectile.ai[1]));
                            Projectile.NewProjectile(projectile.Center, new Vector2(4f+ percent*(3f+chargefloat), 0).RotatedBy(projectile.velocity.ToRotation() + rot), ModContent.ProjectileType<VitricBowShardProjectile>(), projectile.damage, projectile.knockBack, projectile.owner = player.whoAmI
                                ,percent, 0f); //fire the flurry of projectiles
                            if (i>0)
                            Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                        }
                    }
                }
                return;
            }
            projectile.timeLeft -= 1;

        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            float maxalpha = MathHelper.Clamp((projectile.ai[0] - ChargeNeededToFire) / 20f, 0.25f, 0.5f);
            float alpha = Math.Min(projectile.ai[0] / 60f, maxalpha)*Math.Min((float)projectile.timeLeft/8,1f);
            Vector2 maxspread = new Vector2(Math.Min(projectile.ai[0] / MaxCharge, 1) * 0.5f,0.4f);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), new Color(200, 255, 255) * alpha, projectile.velocity.ToRotation() + 1.57f, new Vector2(tex.Width / 2, tex.Height), maxspread, 0, 0);
        }

    }
    //Hey, the boss's attack is 2 projectiles in one file, let me do it here too please?
    public class VitricBowShardProjectile : ModProjectile,IDrawAdditive
    {
        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.friendly = true;
            projectile.tileCollide = true;
            projectile.width = 24;
            projectile.height = 24;
            projectile.arrow = true;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = -1;
            projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            for (int k = 0; k <= 1; k++)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center + projectile.velocity, 264, (projectile.velocity * (Main.rand.NextFloat(-0.25f, -0.1f))).RotatedBy((k == 0) ? 0.4f : -0.4f), 0, default, 1f);
                d.noGravity = true;
            }
            projectile.velocity.Y += 0.025f;

            projectile.localAI[0] += 1;
            if (projectile.localAI[0] == 1)
            {
                projectile.scale = 0.5f+projectile.ai[0]/3f;
                projectile.width = (int)((float)projectile.width * projectile.scale);
                projectile.height = (int)((float)projectile.height * projectile.scale);
                projectile.penetrate = 1+((int)(projectile.ai[0]*4f));
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor, projectile.velocity.ToRotation()-MathHelper.ToRadians(90), tex.Size()/2f, projectile.scale, 0, 0);
            return false;
        }
        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.Item, (int)projectile.Center.X, (int)projectile.Center.Y, 27, 0.75f);
            for (float num315 = 0.2f; num315 < 2+ projectile.scale*1.5f; num315 += 0.25f)
            {
                float angle = MathHelper.ToRadians(Main.rand.Next(0, 360));
                Vector2 vecangle = (new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315)+(projectile.velocity* num315);
                Dust num316 = Dust.NewDustPerfect(new Vector2(projectile.position.X, projectile.position.Y)+new Vector2(Main.rand.Next(projectile.width), Main.rand.Next(projectile.height)), mod.DustType("Glass2"), vecangle / 3f, 50, default, (8f - num315) / 5f);
                num316.fadeIn = 0.5f;
            }
        }
        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.GetTexture("StarlightRiver/Projectiles/GlassSpikeGlow");
            spriteBatch.Draw(tex, projectile.Center + Vector2.Normalize(projectile.velocity) * -40 - Main.screenPosition, tex.Frame(),
                new Color(150, 255, 255) * Math.Min(projectile.timeLeft / 100f,0.5f), projectile.velocity.ToRotation()+MathHelper.ToRadians(-135), tex.Size() / 2, 1.8f+projectile.scale, 0, 0);
        }
    }

}