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

        public override void Load()
        {
            for (int k = 1; k <= 8; k++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + k);

            GoreLoader.AddGoreFromTexture<JetwelderCasingGore>(Mod, AssetDirectory.SteampunkItem + "JetwelderCasing");           
        }

        private const int ATTACKRANGE = 500;
        private const int MINATTACKRANGE = 150;
        private const float SPEED = 15f;
        private const float IDLESPEED = 8f;

        private float idleHoverOffset;
        private int idleYOffset;

        private NPC target;

        private Vector2 posToBe = Vector2.Zero;

        private float rotationGoal;
        private float currentRotation;

        private int gunFrame = 0;

        private bool firing = false;

        private Player Player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gatler");
            Main.projFrames[Projectile.type] = 8;
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
            NPC testtarget = Main.npc.Where(n => n.active  && n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 800 && findPosToBe(n).Length() >= 60).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();

            if (testtarget != default)
            {
                target = testtarget;
                AttackMovement();
            }
            else
                IdleMovement();

            FindFrame();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int frameHeight = tex.Height / Main.projFrames[Projectile.type];
            Vector2 origin = new Vector2(Projectile.spriteDirection == 1 ? 30 : tex.Width - 30, frameHeight / 2);

            Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);

            Texture2D gunTex = ModContent.Request<Texture2D>(Texture + "_Gun").Value;
            Texture2D gunTexIdle = ModContent.Request<Texture2D>(Texture + "_GunIdle").Value;
            Rectangle gunFrame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
            spriteBatch.Draw(firing ? gunTex : gunTexIdle, Projectile.Center - Main.screenPosition, firing ? gunFrame : new Rectangle(0,0, gunTexIdle.Width, gunTexIdle.Height), lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);

            if (firing)
            {
                Texture2D flashTex = ModContent.Request<Texture2D>(Texture + "_Flash").Value;
                spriteBatch.Draw(flashTex, Projectile.Center - Main.screenPosition, gunFrame, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
            }

            return false;
        }

        private void IdleMovement()
        {
            firing = false;
            posToBe = Vector2.Zero;

            Vector2 offset = new Vector2((float)Math.Cos((Main.timeForVisualEffects * 3f) + idleHoverOffset) * 50, -100 + idleYOffset);
            Vector2 direction = (Player.Center + offset) - Projectile.Center;
            if (direction.Length() > 15)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Normalize(direction) * IDLESPEED, 0.02f);

            Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
            Projectile.rotation = 0 + (float)(Math.Sqrt(Projectile.velocity.Length()) * 0.2f * Projectile.spriteDirection);
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

            rotationGoal = towardsTarget.ToRotation();
            float rotDifference = ((((rotationGoal - currentRotation) % 6.28f) + 9.42f) % 6.28f) - 3.14f;
            currentRotation += Math.Sign(rotDifference) * 0.1f;

            Projectile.rotation = currentRotation;

            if (Math.Abs(rotDifference) < 0.15f)
            {
                Projectile.rotation = rotationGoal;
            }

            if (Projectile.rotation.ToRotationVector2().X < 0)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation += 3.14f;
            }
            else
                Projectile.spriteDirection = 1;


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
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Normalize(direction) * speed, lerper);
            }
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 1; i < 9; i++)
            {
                Gore.NewGore(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), Main.rand.NextVector2Circular(5, 5), Mod.Find<ModGore>(Texture + "_Gore" + i.ToString()).Type, 1f);
            }
        }

        private void FireBullets()
        {
            if ((gunFrame == 0 || gunFrame == 4) && Projectile.frameCounter % 2 == 0)
            {
                Vector2 bulletOffset = new Vector2(10, 9 * Projectile.spriteDirection);

                Vector2 dir = currentRotation.ToRotationVector2();
                dir.Normalize();
                bulletOffset = bulletOffset.RotatedBy(currentRotation);

                Gore.NewGore(Projectile.Center, new Vector2(Math.Sign(dir.X) * -1, -0.5f) * 2, Mod.Find<ModGore>(AssetDirectory.SteampunkItem + "JetwelderCasing").Type, 1f);
                Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center + bulletOffset, dir.RotatedByRandom(0.13f) * 15, ProjectileID.Bullet, Projectile.damage, Projectile.knockBack, Player.whoAmI);
            }
        }

        private bool ClearPath(Vector2 point1, Vector2 point2)
        {
            Vector2 direction = point2 - point1;
            for (int i = 0; i < direction.Length(); i += 8)
            {
                Vector2 toLookAt = point1 + (Vector2.Normalize(direction) * i);
                if ((Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).HasTile && Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).TileType]))
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
                    if (i > ATTACKRANGE - 16 || (Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).HasTile && Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).TileType]))
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
            if (Projectile.frameCounter % 3 == 0)
                Projectile.frame++;

            if (Projectile.frameCounter % 2 == 0)
                gunFrame++;
            Projectile.frame %= Main.projFrames[Projectile.type];
            gunFrame %= Main.projFrames[Projectile.type];
            if (!firing)
                gunFrame = 0;
        }
    }
}