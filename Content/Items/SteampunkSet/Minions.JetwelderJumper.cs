using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Dusts;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class JetwelderJumper : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + "JetwelderJumper";

		private readonly int STARTTIMELEFT = 1200;

        private bool jumping = false;

        private float xVel = 0f;

        private bool fired = false;

        private int fireCounter = 0;

		private Player player => Main.player[projectile.owner];

		public override bool Autoload(ref string name)
		{
			StarlightRiver.Instance.AddGore(Texture + "_Gore1");
			StarlightRiver.Instance.AddGore(Texture + "_Gore2");
			StarlightRiver.Instance.AddGore(Texture + "_Gore3");
			StarlightRiver.Instance.AddGore(Texture + "_Gore4");
			StarlightRiver.Instance.AddGore(Texture + "_Gore5");
			StarlightRiver.Instance.AddGore(Texture + "_Gore6");
			StarlightRiver.Instance.AddGore(Texture + "_Gore7");
			StarlightRiver.Instance.AddGore(Texture + "_Gore8");
			return base.Autoload(ref name);
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jumper");
            Main.projFrames[projectile.type] = 14;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.aiStyle = -1;
            projectile.width = 38;
            projectile.height = 50;
            projectile.friendly = false;
            projectile.tileCollide = true;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.penetrate = -1;
            projectile.timeLeft = STARTTIMELEFT;
            projectile.ignoreWater = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            int frameHeight = tex.Height / Main.projFrames[projectile.type];
            Rectangle frame = new Rectangle(0, frameHeight * projectile.frame, tex.Width, frameHeight);

            SpriteEffects effects = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, lightColor, projectile.rotation, tex.Size() / new Vector2(2, 2 * Main.projFrames[projectile.type]), projectile.scale, effects, 0f);
            return false;
        }

        public override void AI()
        {
            NPC testtarget = Main.npc.Where(n => n.active && n.CanBeChasedBy(projectile, false) && Vector2.Distance(n.Center, projectile.Center) < 800).OrderBy(n => Vector2.Distance(n.Center, projectile.Center)).FirstOrDefault();
                projectile.frameCounter++;
            if (projectile.frameCounter % 4 == 0 && !jumping && (fireCounter == 20 || fireCounter == 0 || testtarget == default))
            {
                projectile.frame++;
                if (projectile.frame == 11)
                {
                    Jump(testtarget);
                }
            }

            if (testtarget != default && !jumping && !fired)
            {
                if (projectile.frameCounter % 4 == 0 && projectile.frame < 5)
                    projectile.frame++;
                fireCounter++;
                if (fireCounter == 10)
                    FireMissle(testtarget);
                if (fireCounter == 20)
                    fired = true;
            }
            if (projectile.velocity.Y < 15)
                projectile.velocity.Y += 0.2f;

            if (jumping)
            {
                projectile.velocity.X = xVel;
                if (Math.Abs(projectile.velocity.Y) < 0.5f)
                    projectile.frame = 12;
                else if (projectile.velocity.Y >= 0.5f)
                    projectile.frame = 13;
                else
                    projectile.frame = 11;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (oldVelocity.Y != projectile.velocity.Y && oldVelocity.Y > 0 && projectile.frame > 10)
            {
                if (jumping)
                {
                    fired = false;
                    fireCounter = 0;
                    jumping = false;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 dustVel = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(-2, -0.5f);
                        dustVel.X *= 10;
                        if (Math.Abs(dustVel.X) < 6)
                            dustVel.X += Math.Sign(dustVel.X) * 4;
                        Dust.NewDustPerfect(projectile.Bottom, ModContent.DustType<JetwelderJumperDust>(), dustVel, 0, new Color(236, 214, 146) * 0.3f, Main.rand.NextFloat(0.25f, 0.5f));
                    }
                }
                projectile.frame = 0;
                projectile.frameCounter = 0;
                projectile.velocity.X = 0;
            }
            return false;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 1; i < 9; i++)
            {
                Gore.NewGore(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2, projectile.height / 2), Main.rand.NextVector2Circular(5, 5), ModGore.GetGoreSlot(Texture + "_Gore" + i.ToString()), 1f);
            }
        }

        private void Jump(NPC target)
        {
            jumping = true;
            Vector2 dir = new Vector2(0, -1);

            if (target == default)
                dir = dir.RotatedByRandom(0.4f);
            else
            {
                int offsetDirection = Math.Sign(target.Center.X - projectile.Center.X);
                dir = dir.RotatedBy(Main.rand.NextFloat(Math.Sign((target.Center.X - (offsetDirection * 300)) - projectile.Center.X) * 0.6f));
            }
            projectile.velocity = dir * Main.rand.Next(6, 11);
            xVel = projectile.velocity.X;

            projectile.spriteDirection = Math.Sign(dir.X);
        }

        private void FireMissle(NPC target)
        {
            if (target != default)
            {
                Vector2 vel = ArcVelocityHelper.GetArcVel(projectile.Center, target.Center, 0.25f, 300, 600);
                Projectile.NewProjectile(projectile.Center, vel, ModContent.ProjectileType<JetwelderJumperMissle>(), projectile.damage * 2, projectile.knockBack, player.whoAmI, target.whoAmI);
            }
        }
    }
    public class JetwelderJumperMissle : ModProjectile
    {

        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private Player player => Main.player[projectile.owner];

        private List<Vector2> cache;
        private Trail trail;

        private NPC victim = default;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Grenade");
            Main.projFrames[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 12;       
            projectile.height = 12;  
            projectile.friendly = true;     
            projectile.ranged = true;      
            projectile.tileCollide = true;   
            projectile.penetrate = 1;      
            projectile.timeLeft = 300;   
            projectile.ignoreWater = true;
            projectile.aiStyle = -1;
        }

        public override void AI()
        {
            projectile.velocity.Y += 0.25f;
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;

            projectile.frameCounter++;
            if (projectile.frameCounter % 5 == 0)
                projectile.frame++;

            projectile.frame %= Main.projFrames[projectile.type];

            if (projectile.frame == 0)
                Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 0.7f);

            ManageCaches();
            ManageTrail();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            DrawTrail(spriteBatch);
            Texture2D tex = ModContent.GetTexture(Texture);
            Texture2D glowTex = ModContent.GetTexture(Texture + "_Glow");

            int frameHeight = tex.Height / Main.projFrames[projectile.type];
            Vector2 origin = new Vector2(tex.Width / 2, frameHeight / 2);

            Rectangle frame = new Rectangle(0, frameHeight * projectile.frame, tex.Width, frameHeight);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, lightColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (projectile.velocity.Y < 0)
                return false;
            return base.CanHitNPC(target);
        }
        public override void Kill(int timeLeft)
        {

            /*for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDust>());
                dust.velocity = Main.rand.NextVector2Circular(7, 7);
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
                dust.alpha = 70 + Main.rand.Next(60);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }*/

            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<JetwelderDust>());
                dust.velocity = Main.rand.NextVector2Circular(4, 4);
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
                dust.alpha = Main.rand.Next(80) + 40;
                dust.rotation = Main.rand.NextFloat(6.28f);

                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustFour>()).scale = 0.9f;
            }

            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectileDirect(projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(1, 2), ModContent.ProjectileType<CoachGunEmber>(), 0, 0, player.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
            }

            Projectile.NewProjectileDirect(projectile.Center, Vector2.Zero, ModContent.ProjectileType<JetwelderJumperExplosion>(), projectile.damage, 0, player.whoAmI, victim == default ? -1 : victim.whoAmI);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16) + (vel * Main.rand.Next(70)), 0, 0, ModContent.DustType<JetwelderDustTwo>());
                dust.velocity = vel * Main.rand.Next(7);
                dust.scale = Main.rand.NextFloat(0.3f, 0.7f);
                dust.alpha = 70 + Main.rand.Next(60);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            victim = target;
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 10; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > 10)
            {
                cache.RemoveAt(0);
            }

        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(4), factor => 4, factor =>
            {
                Color trailColor = Color.White;
                return trailColor * 0.3f;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity;
        }

        private void DrawTrail(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            Effect effect = Filters.Scene["CoachBombTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/MotionTrail"));

            trail?.Render(effect);

            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    internal class JetwelderJumperExplosion : ModProjectile
    {
        public override string Texture => AssetDirectory.Assets + "Invisible";

        //private List<Vector2> cache;

        //private Trail trail;
        //private Trail trail2;

        private float Progress => 1 - (projectile.timeLeft / 5f);

        private float Radius => 75 * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override void SetDefaults()
        {
            projectile.width = 80;
            projectile.height = 80;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 5;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rocket");
        }

        public override void AI()
        {
            //ManageCaches();
            //ManageTrail();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - projectile.Center;
            line.Normalize();
            line *= Radius;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + line))
            {
                return true;
            }
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.whoAmI == projectile.ai[0])
                return false;
            return base.CanHitNPC(target);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
    }
}