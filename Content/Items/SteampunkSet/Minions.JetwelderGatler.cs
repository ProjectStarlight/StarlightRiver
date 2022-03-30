using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class JetwelderGatler : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + "JetwelderGatler";

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
            StarlightRiver.Instance.AddGore(AssetDirectory.SteampunkItem + "JetwelderCasing", new JetwelderCasingGore());
            return base.Autoload(ref name);
        }

        private readonly int ATTACKRANGE = 500;
        private readonly int MINATTACKRANGE = 150;
        private readonly float SPEED = 15f;
        private readonly float IDLESPEED = 8f;

        private float idleHoverOffset;
        private int idleYOffset;

        private NPC target;

        private Vector2 posToBe = Vector2.Zero;

        private float rotationGoal;
        private float currentRotation;

        private int gunFrame = 0;

        private bool firing = false;

        private Player player => Main.player[projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gatler");
            Main.projFrames[projectile.type] = 8;
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
            NPC testtarget = Main.npc.Where(n => n.active  && n.CanBeChasedBy(projectile, false) && Vector2.Distance(n.Center, projectile.Center) < 800 && findPosToBe(n).Length() >= 60).OrderBy(n => Vector2.Distance(n.Center, projectile.Center)).FirstOrDefault();

            if (testtarget != default)
            {
                target = testtarget;
                AttackMovement();
            }
            else
                IdleMovement();

            FindFrame();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            SpriteEffects spriteEffects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int frameHeight = tex.Height / Main.projFrames[projectile.type];
            Vector2 origin = new Vector2(projectile.spriteDirection == 1 ? 30 : tex.Width - 30, frameHeight / 2);

            Rectangle frame = new Rectangle(0, frameHeight * projectile.frame, tex.Width, frameHeight);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, lightColor, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);

            Texture2D gunTex = ModContent.GetTexture(Texture + "_Gun");
            Texture2D gunTexIdle = ModContent.GetTexture(Texture + "_GunIdle");
            Rectangle gunFrame = new Rectangle(0, frameHeight * projectile.frame, tex.Width, frameHeight);
            spriteBatch.Draw(firing ? gunTex : gunTexIdle, projectile.Center - Main.screenPosition, firing ? gunFrame : new Rectangle(0,0, gunTexIdle.Width, gunTexIdle.Height), lightColor, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);

            if (firing)
            {
                Texture2D flashTex = ModContent.GetTexture(Texture + "_Flash");
                spriteBatch.Draw(flashTex, projectile.Center - Main.screenPosition, gunFrame, Color.White, projectile.rotation, origin, projectile.scale, spriteEffects, 0f);
            }

            return false;
        }

        private void IdleMovement()
        {
            firing = false;
            posToBe = Vector2.Zero;

            Vector2 offset = new Vector2((float)Math.Cos((Main.GlobalTime * 3f) + idleHoverOffset) * 50, -100 + idleYOffset);
            Vector2 direction = (player.Center + offset) - projectile.Center;
            if (direction.Length() > 15)
                projectile.velocity = Vector2.Lerp(projectile.velocity, Vector2.Normalize(direction) * IDLESPEED, 0.02f);

            projectile.spriteDirection = Math.Sign(projectile.velocity.X);
            projectile.rotation = 0 + (float)(Math.Sqrt(projectile.velocity.Length()) * 0.2f * projectile.spriteDirection);
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

            rotationGoal = towardsTarget.ToRotation();
            float rotDifference = ((((rotationGoal - currentRotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
            currentRotation += Math.Sign(rotDifference) * 0.1f;

            projectile.rotation = currentRotation;

            if (Math.Abs(rotDifference) < 0.15f)
            {
                projectile.rotation = rotationGoal;
            }

            if (projectile.rotation.ToRotationVector2().X < 0)
            {
                projectile.spriteDirection = -1;
                projectile.rotation += 3.14f;
            }
            else
                projectile.spriteDirection = 1;


            if (direction.Length() < 250 || Math.Abs(currentRotation - direction.ToRotation()) < 0.3f)
            {
                FireBullets();
                firing = true;
            }
            else
                firing = false;
            if (direction.Length() > 20)
            {
                float speed = (float)Math.Min(SPEED, Math.Sqrt(direction.Length() * 0.1f));

                float lerper = direction.Length() > 100 ? 0.1f : 0.04f;
                projectile.velocity = Vector2.Lerp(projectile.velocity, Vector2.Normalize(direction) * speed, lerper);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 1; i < 9; i++)
            {
                Gore.NewGore(projectile.Center + Main.rand.NextVector2Circular(projectile.width / 2, projectile.height / 2), Main.rand.NextVector2Circular(5, 5), ModGore.GetGoreSlot(Texture + "_Gore" + i.ToString()), 1f);
            }
        }

        private void FireBullets()
        {
            if ((gunFrame == 0 || gunFrame == 4) && projectile.frameCounter % 2 == 0)
            {
                Vector2 bulletOffset = new Vector2(10, 9 * projectile.spriteDirection);

                Vector2 dir = currentRotation.ToRotationVector2();
                dir.Normalize();
                bulletOffset = bulletOffset.RotatedBy(currentRotation);

                Gore.NewGore(projectile.Center, new Vector2(Math.Sign(dir.X) * -1, -0.5f) * 2, ModGore.GetGoreSlot(AssetDirectory.SteampunkItem + "JetwelderCasing"), 1f);
                Projectile.NewProjectile(projectile.Center + bulletOffset, dir.RotatedByRandom(0.13f) * 15, ProjectileID.Bullet, projectile.damage, projectile.knockBack, player.whoAmI);
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
                if (angle > -2.355f && angle < -0.785)
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

            if (projectile.frameCounter % 2 == 0)
                gunFrame++;
            projectile.frame %= Main.projFrames[projectile.type];
            gunFrame %= Main.projFrames[projectile.type];
            if (!firing)
                gunFrame = 0;
        }
    }
}