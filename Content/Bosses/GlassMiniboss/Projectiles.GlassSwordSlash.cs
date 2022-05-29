using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassSword : ModProjectile
    {
        public override string Texture => AssetDirectory.Glassweaver + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glass Sword");
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 110;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.manualDirectionChange = true;
        }

        public ref float Timer => ref Projectile.ai[0];

        public ref float Variant => ref Projectile.localAI[0];

        public NPC Parent => Main.npc[(int)Projectile.ai[1]];

        private Vector2 gripPos;

        private int[] slashTime = new int[] { 70, 105, 120 };

        public override void OnSpawn(IEntitySource source)
        {
            Helpers.Helper.PlayPitched("GlassMiniboss/WeavingShort", 1f, 0f, Projectile.Center);
            slashTime = new int[] { 70, 105, 120 };
        }

        public override void AI()
        {
            if (!Parent.active || Parent.type != NPCType<Glassweaver>())
                Projectile.Kill();

            Timer++;

            Projectile.Center = Parent.Center + new Vector2(30 * Parent.direction, -20);
            Projectile.velocity = Parent.velocity;

            Lighting.AddLight(Projectile.Center, Glassweaver.GlassColor.ToVector3());
            Projectile.direction = -1;

            Vector2 swordOff = Vector2.Zero;
            float swordTargetRot = 4.2f;
            switch (Variant)
            {
                case 0:
                    swordOff = new Vector2(38, 15);
                    swordTargetRot = 4f;
                    if (Timer > slashTime[0])
                    {
                        swordOff = new Vector2(32, -55);
                        swordTargetRot = -0.4f;
                        Projectile.direction = -1;
                    }
                    else
                        Projectile.direction = -1;
                    if (Timer > slashTime[1])
                    {
                        swordOff = new Vector2(34, 25);
                        swordTargetRot = 3.5f;
                    }
                    if (Timer > slashTime[2])
                    {
                        swordOff = new Vector2(48, -25);
                        swordTargetRot = -0.1f;
                    }

                    break;

                case 1:
                    swordOff = new Vector2(-24, 15);
                    swordTargetRot = 4.2f;
                    Projectile.direction = 1;
                    if (Timer > slashTime[0])
                    {
                        swordOff = new Vector2(24, 5);
                        swordTargetRot = 0.44f;
                    }
                    if (Timer > slashTime[1])
                    {
                        swordOff = new Vector2(-38, 0);
                        swordTargetRot = 4.2f;
                    }
                    if (Timer > slashTime[2])
                    {
                        swordOff = new Vector2(48, -25);
                        swordTargetRot = -0.44f;
                    }

                    break;

                case 2:
                    swordOff = new Vector2(-36, 5);
                    swordTargetRot = 4.4f;
                    if (Timer > slashTime[0])
                    {
                        swordOff = new Vector2(-36, 20);
                        swordTargetRot = 4.1f;
                    }
                    if (Timer > slashTime[1])
                    {
                        swordOff = new Vector2(-24, 12);
                        swordTargetRot = 3.9f;
                    }
                    if (Timer > slashTime[2])
                    {
                        swordOff = new Vector2(48, -25);
                        swordTargetRot = -0.8f;
                    }

                    break;
            }
            swordOff.X *= Parent.direction;
            swordTargetRot *= Parent.direction;

            gripPos = Parent.Center + swordOff.RotatedBy(Parent.rotation);
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, swordTargetRot, 0.35f) + Parent.rotation;

            if (Timer < 60)
            {
                Vector2 vel = new Vector2(0, -Main.rand.NextFloat(5f)).RotatedBy(Projectile.rotation).RotatedByRandom(0.2f);
                Dust.NewDustPerfect(gripPos, DustType<Dusts.Cinder>(), vel, 0, Glassweaver.GlowDustOrange, 1f);
            }

            int extraTime = 27;
            if (Variant == 1)
                extraTime = 12;
            if (Timer > extraTime + slashTime[(int)Variant])
                Projectile.Kill();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Timer > slashTime[(int)Variant] && projHitbox.Intersects(targetHitbox);

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Math.Abs(Parent.velocity.X) > 0)
                Parent.velocity.X = -oldVelocity.X;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> sword = Request<Texture2D>(Texture);
            Rectangle frame = sword.Frame(2, 1, 0);
            Rectangle hotFrame = sword.Frame(2, 1, 1);
            Vector2 origin = frame.Size() * new Vector2(0.5f, 0.84f);

            SpriteEffects dir = Projectile.direction * Parent.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float scaleIn = Projectile.scale * Helpers.Helper.BezierEase(Utils.GetLerpValue(50, 70, Timer, true));

            Color fadeIn = Color.Lerp(lightColor, Color.White, Utils.GetLerpValue(100, 60, Timer, true));
            Main.EntitySpriteDraw(sword.Value, gripPos - Main.screenPosition, frame, fadeIn, Projectile.rotation, origin, scaleIn, dir, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(80, 70, Timer, true);
            Main.EntitySpriteDraw(sword.Value, gripPos - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, origin, scaleIn, dir, 0);

            Asset<Texture2D> slash = Request<Texture2D>(Texture + "Slash");
            Rectangle slashFill = slash.Frame(1, 2, 0, 0);
            Rectangle slashLine = slash.Frame(1, 2, 0, 1);
            float slashLerp = Utils.GetLerpValue(slashTime[(int)Variant] - 2, slashTime[(int)Variant], Timer, true) * Utils.GetLerpValue(slashTime[(int)Variant] + 10, slashTime[(int)Variant] + 3, Timer, true);
            Color slashFillColor = Glassweaver.GlassColor * 0.8f * slashLerp;
            slashFillColor.A = 0;
            Color slashLineColor = Color.MediumAquamarine * slashLerp;
            slashLineColor.A = 0;
            Vector2 slashScale = new Vector2(1.2f, 1.5f) + new Vector2(Utils.GetLerpValue(slashTime[(int)Variant] - 2, slashTime[(int)Variant] + 10, Timer, true));
            Main.EntitySpriteDraw(slash.Value, gripPos - Main.screenPosition, slashFill, slashFillColor, (MathHelper.PiOver4 * Parent.direction) + Projectile.rotation * 0.5f, slashFill.Size() * new Vector2(0.5f, 0.4f), slashScale, 0, 0);
            Main.EntitySpriteDraw(slash.Value, gripPos - Main.screenPosition, slashLine, slashLineColor, (MathHelper.PiOver4 * Parent.direction) + Projectile.rotation * 0.5f, slashFill.Size() * new Vector2(0.5f, 0.4f), slashScale, 0, 0);

            return false;
        }
    }
}
