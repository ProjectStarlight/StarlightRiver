using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class JetwelderFinal : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + "JetwelderFinal";

        public override bool Autoload(ref string name)
        {
            return base.Autoload(ref name);
        }

        private readonly int ATTACKRANGE = 400;
        private readonly int MINATTACKRANGE = 150;
        private readonly float SPEED = 5f;
        private readonly float IDLESPEED = 3f;

        private float idleHoverOffset;
        private int idleYOffset;

        private NPC target;
        private NPC target2;

        private Vector2 posToBe = Vector2.Zero;

        private int shootCounter = 0;

        private bool firing = false;

        private float backRotation = 0f;
        private float frontRotation = 0f;
        private Vector2 backOffset = new Vector2(12, -15);

        private float backOffsetY = 0f;
        private float frontOffsetY = 0f;
        private Vector2 frontOffset = new Vector2(-6, -15);


        private Player player => Main.player[projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bomber");
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.Homing[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.aiStyle = -1;
            projectile.width = 60;
            projectile.height = 56;
            projectile.friendly = false;
            projectile.tileCollide = false;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 1200;
            idleHoverOffset = Main.rand.NextFloat(6.28f);
            idleYOffset = Main.rand.Next(-25, 35);

        }

        public override void AI()
        {
            backOffsetY *= 0.92f;
            frontOffsetY *= 0.92f;
            NPC testtarget = Main.npc.Where(n => n.active /* && n.CanBeChasedBy(projectile, false)*/ && Vector2.Distance(n.Center, projectile.Center) < 800 && findPosToBe(n).Length() >= 60).OrderBy(n => Vector2.Distance(n.Center, projectile.Center)).FirstOrDefault();

            if (testtarget != default)
            {
                target = testtarget;

                NPC testtarget2 = Main.npc.Where(n => n.active /* && n.CanBeChasedBy(projectile, false)*/&& Vector2.Distance(n.Center, projectile.Center) < 800 && findPosToBe(n).Length() >= 60 && n.Distance(target.Center) > 60).OrderBy(n => Vector2.Distance(n.Center, projectile.Center)).FirstOrDefault();
                if (testtarget2 != default)
                    target2 = testtarget2;

                AttackMovement();
            }
            else
                IdleMovement();

            FindFrame();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            Texture2D glowTex = ModContent.GetTexture(Texture + "_Glow");
            SpriteEffects spriteEffects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int frameHeight = tex.Height / Main.projFrames[projectile.type];
            Vector2 origin = new Vector2(tex.Width / 2, frameHeight / 2);

            Rectangle frame = new Rectangle(0, frameHeight * projectile.frame, tex.Width, frameHeight);

            Texture2D backTex = ModContent.GetTexture(Texture + "_Back");
            Texture2D frontTex = ModContent.GetTexture(Texture + "_Front");

            Vector2 backOffsetReal = new Vector2(backOffset.X * projectile.spriteDirection, backOffset.Y + backOffsetY).RotatedBy(projectile.rotation);

            Vector2 frontOffsetReal = new Vector2(frontOffset.X * projectile.spriteDirection, frontOffset.Y + frontOffsetY).RotatedBy(projectile.rotation);

            spriteBatch.Draw(backTex, (projectile.Center + backOffsetReal) - Main.screenPosition, null, lightColor, backRotation, backTex.Size() / 2, projectile.scale, spriteEffects, 0f);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, lightColor, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);
            spriteBatch.Draw(glowTex, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);
            spriteBatch.Draw(frontTex, (projectile.Center + frontOffsetReal) - Main.screenPosition, null, lightColor, frontRotation, frontTex.Size() / 2, projectile.scale, spriteEffects, 0f);
            return false;
        }

        private void IdleMovement()
        {
            firing = false;
            posToBe = Vector2.Zero;

            Vector2 offset = new Vector2((float)Math.Cos((Main.GlobalTime * 3f) + idleHoverOffset) * 50, -100 + idleYOffset);
            Vector2 direction = (player.Center + offset) - projectile.Center;
            if (direction.Length() > 15)
                projectile.velocity = Vector2.Lerp(projectile.velocity, Vector2.Normalize(direction) * IDLESPEED, 0.05f);

            projectile.spriteDirection = Math.Sign(projectile.velocity.X);
            projectile.rotation = 0 + (float)(Math.Sqrt(projectile.velocity.Length()) * 0.2f * projectile.spriteDirection);
            backRotation = projectile.rotation;
            frontRotation = projectile.rotation;
        }

        private void AttackMovement()
        {
            if (posToBe == Vector2.Zero || !ClearPath(target.Center + posToBe, target.Center))
            {
                posToBe = findPosToBe(target);
            }
            if (posToBe.Length() < 60)
            {
                IdleMovement();
                return;
            }

            Vector2 direction = (posToBe + target.Center) - projectile.Center;

            Vector2 towardsTarget = target.Center - projectile.Center;

            projectile.spriteDirection = Math.Sign(towardsTarget.X);

            projectile.rotation = 0 + (float)(Math.Sqrt(projectile.velocity.Length()) * 0.2f * projectile.spriteDirection);

            backRotation = MathHelper.Clamp((float)Math.Sqrt(Math.Abs(towardsTarget.X)) * Math.Sign(towardsTarget.X) * 0.04f, -0.5f,0.5f);
            if (target2 != default)
            {
                Vector2 towardsTarget2 = target2.Center - projectile.Center;
                frontRotation = MathHelper.Clamp((float)Math.Sqrt(Math.Abs(towardsTarget2.X)) * Math.Sign(towardsTarget2.X) * 0.04f, -0.5f, 0.5f);
            }
            else
                frontRotation = backRotation;

            if (direction.Length() < 250)
            {
                FireRockets();
                firing = true;
            }
            else
                firing = false;
            if (direction.Length() > 20)
            {
                float speed = (float)Math.Min(SPEED, Math.Sqrt(direction.Length() * 0.1f));
                projectile.velocity = Vector2.Lerp(projectile.velocity, Vector2.Normalize(direction) * speed, 0.2f);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 1; i < 9; i++)
            {
                Gore.NewGore(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2, projectile.height / 2), Main.rand.NextVector2Circular(5, 5), ModGore.GetGoreSlot(Texture + "_Gore" + i.ToString()), 1f);
            }
        }

        private void FireRockets()
        {
            firing = true;
            shootCounter++;

            if (shootCounter % 8 == 0)
            {
                NPC trueTarget = target;

                if (shootCounter % 16 == 0) //front
                {
                    if (target2 != default)
                        trueTarget = target2;
                    Vector2 bulletOffset = new Vector2(frontOffset.X,  frontOffset.Y - 5);
                    bulletOffset = bulletOffset.RotatedBy(frontRotation);
                    frontOffsetY = 16;
                    Projectile.NewProjectile(projectile.Center + bulletOffset, Vector2.UnitY.RotatedByRandom(0.5f).RotatedBy(frontRotation) * -15, ModContent.ProjectileType<JetwelderFinalMissle>(), projectile.damage, projectile.knockBack, player.whoAmI, trueTarget.whoAmI);
                }
                else //back
                {
                    Vector2 bulletOffset = new Vector2(backOffset.X, backOffset.Y - 5);
                    bulletOffset = bulletOffset.RotatedBy(backRotation);
                    backOffsetY = 16;
                    Projectile.NewProjectile(projectile.Center + bulletOffset, Vector2.UnitY.RotatedByRandom(0.5f).RotatedBy(backRotation) * -15, ModContent.ProjectileType<JetwelderFinalMissle>(), projectile.damage, projectile.knockBack, player.whoAmI, trueTarget.whoAmI);
                }
            }
        }

        private bool ClearPath(Vector2 point1, Vector2 point2)
        {
            Vector2 direction = point2 - point1;
            for (int i = 0; i < direction.Length(); i += 8)
            {
                Vector2 toLookAt = point1 + (Vector2.Normalize(direction) * i);
                if ((Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).active() && Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).type]))
                {
                    return false;
                }
            }
            return true;
        }


        private Vector2 findPosToBe(NPC tempTarget)
        {
            Vector2 ret = Vector2.Zero;
            int tries = 0;
            while (tries < 99)
            {
                tries++;
                float angle = Main.rand.NextFloat(-3.14f, 0f);
                if (angle < -2.641f || angle > -0.5)
                    continue;
                for (int i = 0; i < ATTACKRANGE; i += 8)
                {
                    Vector2 toLookAt = tempTarget.Center + (angle.ToRotationVector2() * i);
                    if (i > ATTACKRANGE - 16 || (Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).active() && Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).type]))
                    {
                        ret = (angle.ToRotationVector2() * i * 0.75f);

                        if (i > MINATTACKRANGE)
                            tries = 100;
                        break;
                    }
                }
            }
            return ret;
        }

        private void FindFrame()
        {
            projectile.frameCounter++;
            if (projectile.frameCounter % 3 == 0)
                projectile.frame++;

            projectile.frame %= Main.projFrames[projectile.type];
        }
    }
    public class JetwelderFinalMissle : ModProjectile
    {

        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private Player player => Main.player[projectile.owner];

        private NPC victim = default;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rocket");
            Main.projFrames[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.tileCollide = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 300;
            projectile.ignoreWater = true;
            projectile.aiStyle = -1;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            Texture2D glowTex = ModContent.GetTexture(Texture + "_Glow");

            int frameHeight = tex.Height / Main.projFrames[projectile.type];
            Vector2 origin = new Vector2(tex.Width / 2, frameHeight / 2);

            Rectangle frame = new Rectangle(0, frameHeight * projectile.frame, tex.Width, frameHeight);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, lightColor, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, origin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void AI()
        {
            NPC target = Main.npc[(int)projectile.ai[0]];
            if (target.life <= 0 || !target.active)
            {
                target = Main.npc.Where(n => n.active  && n.CanBeChasedBy(projectile, false) && Vector2.Distance(n.Center, projectile.Center) < 400).OrderBy(n => Vector2.Distance(n.Center, projectile.Center)).FirstOrDefault();
                if (target != default)
                    projectile.ai[0] = target.whoAmI;
            }
            if (target != default)
            {
                Vector2 dir = Vector2.Normalize(projectile.DirectionTo(target.Center));
                projectile.velocity = Vector2.Lerp(projectile.velocity, dir * 13, 0.05f);
            }
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;

            projectile.frameCounter++;
            if (projectile.frameCounter % 5 == 0)
                projectile.frame++;

            projectile.frame %= Main.projFrames[projectile.type];

            if (projectile.timeLeft < 298)
            {
                for (int i = 0; i < 4; i++)
                {
                    Dust dust = Dust.NewDustPerfect((projectile.Center - new Vector2(4, 4)) - (projectile.velocity * Main.rand.NextFloat(2)), ModContent.DustType<JetwelderFinalDust>(), Vector2.Normalize(-projectile.velocity).RotatedByRandom(0.3f) * Main.rand.NextFloat(1.5f));
                    dust.scale = 0.3f * projectile.scale;
                    dust.rotation = Main.rand.NextFloatDirection();
                }
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            return base.CanHitNPC(target);
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<JetwelderDust>());
                dust.velocity = Main.rand.NextVector2Circular(2, 2);
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
                dust.alpha = Main.rand.Next(80) + 40;
                dust.rotation = Main.rand.NextFloat(6.28f);

                Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustFour>()).scale = 0.9f;
            }

            for (int i = 0; i < 1; i++)
            {
                Projectile.NewProjectileDirect(projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(1, 2), ModContent.ProjectileType<CoachGunEmber>(), 0, 0, player.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
            }

            Projectile.NewProjectileDirect(projectile.Center, Vector2.Zero, ModContent.ProjectileType<JetwelderJumperExplosion>(), projectile.damage, 0, player.whoAmI, victim == default ? -1 : victim.whoAmI);
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(16, 16) + (vel * Main.rand.Next(20)), 0, 0, ModContent.DustType<JetwelderDustTwo>());
                dust.velocity = vel * Main.rand.Next(2);
                dust.scale = Main.rand.NextFloat(0.3f, 0.7f);
                dust.alpha = 70 + Main.rand.Next(60);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            victim = target;
        }
    }
}