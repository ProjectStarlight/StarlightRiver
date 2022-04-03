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

        public override void Load()
        {
            StarlightRiver.Instance.AddGore(Texture + "_Gore1");
            StarlightRiver.Instance.AddGore(Texture + "_Gore2");
            StarlightRiver.Instance.AddGore(Texture + "_Gore3");
            StarlightRiver.Instance.AddGore(Texture + "_Gore4");
            StarlightRiver.Instance.AddGore(Texture + "_Gore5");
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


        private Player Player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bomber");
            Main.projFrames[Projectile.type] = 2;
            ProjectileID.Sets.Homing[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = 60;
            Projectile.height = 56;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1200;
            idleHoverOffset = Main.rand.NextFloat(6.28f);
            idleYOffset = Main.rand.Next(-25, 35);

        }

        public override void AI()
        {
            backOffsetY *= 0.92f;
            frontOffsetY *= 0.92f;
            NPC testtarget = Main.npc.Where(n => n.active  && n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800 && findPosToBe(n).Length() >= 60).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();

            if (testtarget != default)
            {
                target = testtarget;

                NPC testtarget2 = Main.npc.Where(n => n.active  && n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800 && findPosToBe(n).Length() >= 60 && n.Distance(target.Center) > 60).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();
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
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Vector2 origin = new Vector2(tex.Width / 2, frameHeight / 2);

            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

            Texture2D backTex = ModContent.Request<Texture2D>(Texture + "_Back").Value;
            Texture2D frontTex = ModContent.Request<Texture2D>(Texture + "_Front").Value;

            Vector2 backOffsetReal = new Vector2(backOffset.X * Projectile.spriteDirection, backOffset.Y + backOffsetY).RotatedBy(Projectile.rotation);

            Vector2 frontOffsetReal = new Vector2(frontOffset.X * Projectile.spriteDirection, frontOffset.Y + frontOffsetY).RotatedBy(Projectile.rotation);

            spriteBatch.Draw(backTex, (Projectile.Center + backOffsetReal) - Main.screenPosition, null, lightColor, backRotation, backTex.Size() / 2, Projectile.scale, spriteEffects, 0f);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
            spriteBatch.Draw(frontTex, (Projectile.Center + frontOffsetReal) - Main.screenPosition, null, lightColor, frontRotation, frontTex.Size() / 2, Projectile.scale, spriteEffects, 0f);
            return false;
        }

        private void IdleMovement()
        {
            firing = false;
            posToBe = Vector2.Zero;

            Vector2 offset = new Vector2((float)Math.Cos((Main.GlobalTime * 3f) + idleHoverOffset) * 50, -100 + idleYOffset);
            Vector2 direction = (Player.Center + offset) - Projectile.Center;
            if (direction.Length() > 15)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Normalize(direction) * IDLESPEED, 0.05f);

            Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
            Projectile.rotation = 0 + (float)(Math.Sqrt(Projectile.velocity.Length()) * 0.2f * Projectile.spriteDirection);
            backRotation = Projectile.rotation;
            frontRotation = Projectile.rotation;
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

            Vector2 direction = (posToBe + target.Center) - Projectile.Center;

            Vector2 towardsTarget = target.Center - Projectile.Center;

            Projectile.spriteDirection = Math.Sign(towardsTarget.X);

            Projectile.rotation = 0 + (float)(Math.Sqrt(Projectile.velocity.Length()) * 0.2f * Projectile.spriteDirection);

            backRotation = MathHelper.Clamp((float)Math.Sqrt(Math.Abs(towardsTarget.X)) * Math.Sign(towardsTarget.X) * 0.04f, -0.5f,0.5f);
            if (target2 != default)
            {
                Vector2 towardsTarget2 = target2.Center - Projectile.Center;
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
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Normalize(direction) * speed, 0.2f);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 1; i < 6; i++)
            {
                Gore.NewGore(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), Main.rand.NextVector2Circular(5, 5), Mod.Find<ModGore>(Texture + "_Gore" + i.ToString()), 1f);
            }

            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<JetwelderDust>());
                dust.velocity = Main.rand.NextVector2Circular(4, 4);
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
                dust.alpha = Main.rand.Next(80) + 40;
                dust.rotation = Main.rand.NextFloat(6.28f);

                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustFour>()).scale = 0.9f;
            }

            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectileDirect(Projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(1, 2), ModContent.ProjectileType<CoachGunEmber>(), 0, 0, Player.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + (vel * Main.rand.Next(70)), 0, 0, ModContent.DustType<JetwelderDustTwo>());
                dust.velocity = vel * Main.rand.Next(7);
                dust.scale = Main.rand.NextFloat(0.3f, 0.7f);
                dust.alpha = 70 + Main.rand.Next(60);
                dust.rotation = Main.rand.NextFloat(6.28f);
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
                    Projectile.NewProjectile(Projectile.Center + bulletOffset, Vector2.UnitY.RotatedByRandom(0.5f).RotatedBy(frontRotation) * -15, ModContent.ProjectileType<JetwelderFinalMissle>(), Projectile.damage, Projectile.knockBack, Player.whoAmI, trueTarget.whoAmI);
                }
                else //back
                {
                    Vector2 bulletOffset = new Vector2(backOffset.X, backOffset.Y - 5);
                    bulletOffset = bulletOffset.RotatedBy(backRotation);
                    backOffsetY = 16;
                    Projectile.NewProjectile(Projectile.Center + bulletOffset, Vector2.UnitY.RotatedByRandom(0.5f).RotatedBy(backRotation) * -15, ModContent.ProjectileType<JetwelderFinalMissle>(), Projectile.damage, Projectile.knockBack, Player.whoAmI, trueTarget.whoAmI);
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
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 5 == 0)
                Projectile.frame++;

            Projectile.frame %= Main.projFrames[Projectile.type];
        }
    }
    public class JetwelderFinalMissle : ModProjectile
    {

        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private Player Player => Main.player[Projectile.owner];

        private NPC victim = default;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rocket");
            Main.projFrames[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.ranged = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Vector2 origin = new Vector2(tex.Width / 2, frameHeight / 2);

            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void AI()
        {
            NPC target = Main.npc[(int)Projectile.ai[0]];
            if (target.life <= 0 || !target.active)
            {
                target = Main.npc.Where(n => n.active  && n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 400).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();
                if (target != default)
                    Projectile.ai[0] = target.whoAmI;
            }
            if (target != default)
            {
                Vector2 dir = Vector2.Normalize(Projectile.DirectionTo(target.Center));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 13, 0.05f);
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

            Projectile.frameCounter++;
            if (Projectile.frameCounter % 5 == 0)
                Projectile.frame++;

            Projectile.frame %= Main.projFrames[Projectile.type];

            if (Projectile.timeLeft < 298)
            {
                for (int i = 0; i < 4; i++)
                {
                    Dust dust = Dust.NewDustPerfect((Projectile.Center - new Vector2(4, 4)) - (Projectile.velocity * Main.rand.NextFloat(2)), ModContent.DustType<JetwelderFinalDust>(), Vector2.Normalize(-Projectile.velocity).RotatedByRandom(0.3f) * Main.rand.NextFloat(1.5f));
                    dust.scale = 0.3f * Projectile.scale;
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
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<JetwelderDust>());
                dust.velocity = Main.rand.NextVector2Circular(2, 2);
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
                dust.alpha = Main.rand.Next(80) + 40;
                dust.rotation = Main.rand.NextFloat(6.28f);

                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustFour>()).scale = 0.9f;
            }

            for (int i = 0; i < 1; i++)
            {
                Projectile.NewProjectileDirect(Projectile.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(1, 2), ModContent.ProjectileType<CoachGunEmber>(), 0, 0, Player.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
            }

            Projectile.NewProjectileDirect(Projectile.Center, Vector2.Zero, ModContent.ProjectileType<JetwelderJumperExplosion>(), Projectile.damage, 0, Player.whoAmI, victim == default ? -1 : victim.whoAmI);
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + (vel * Main.rand.Next(20)), 0, 0, ModContent.DustType<JetwelderDustTwo>());
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