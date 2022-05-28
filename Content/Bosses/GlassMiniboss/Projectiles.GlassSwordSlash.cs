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
    class GlassSwordSlash : ModProjectile
    {
        public override string Texture => AssetDirectory.Glassweaver + "GlassSword";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glass Sword");

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 110;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.ignoreWater = true;
            Projectile.manualDirectionChange = true;
        }

        public ref float Timer => ref Projectile.ai[0];

        public ref float Variant => ref Projectile.localAI[0];

        public NPC Parent => Main.npc[(int)Projectile.ai[1]];

        private Vector2 gripPos;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = 2f * Parent.direction;
        }

        public override void AI()
        {
            if (!Parent.active || Parent.type != NPCType<Glassweaver>())
                Projectile.Kill();

            Timer++;

            Projectile.velocity = Parent.velocity;
            Projectile.Center = Parent.Center + new Vector2(30 * Parent.direction, -20);

            Lighting.AddLight(Projectile.Center, Glassweaver.GlassColor.ToVector3());
            Projectile.direction = -1;

            int[] slashTime = new int[] { 70, 105, 120 };
            Vector2 swordOff = Vector2.Zero;
            float swordTargetRot = 0f;
            switch (Variant)
            {
                case 0:

                    swordOff = new Vector2(-10, 10);
                    swordTargetRot = 4.2f;
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
                    swordOff = new Vector2(-20, 10);
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
                    swordOff = new Vector2(-10, 0);
                    swordTargetRot = 4.2f;
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
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, swordTargetRot, 0.38f) + Parent.rotation;

            int extraTime = 27;
            if (Variant == 1)
                extraTime = 12;
            if (Timer > extraTime + slashTime[(int)Variant])
                Projectile.Kill();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Rectangle slashBox = projHitbox;
            slashBox.Inflate(20, 20);
            return Timer > 0 && slashBox.Intersects(targetHitbox);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawSword(lightColor);
            return false;
        }

        private void DrawSword(Color lightColor)
        {
            Asset<Texture2D> sword = Request<Texture2D>(Texture);
            Vector2 origin = sword.Size() * new Vector2(0.5f, 0.8f);

            Color fadeIn = Color.Lerp(lightColor, Color.White, Utils.GetLerpValue(115 - (20 * Variant), 70, Timer, true));

            Main.EntitySpriteDraw(sword.Value, gripPos - Main.screenPosition, null, fadeIn, Projectile.rotation, origin, Projectile.scale, Direction(), 0);

        }

        private SpriteEffects Direction()
        {
            SpriteEffects effect = Projectile.direction * Parent.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            return effect;
        }
    }
}
