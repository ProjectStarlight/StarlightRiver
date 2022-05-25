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
        public override string Texture => AssetDirectory.Glassweaver + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glass Sword");

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 110;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public ref float Timer => ref Projectile.ai[0];

        public ref float Variant => ref Projectile.localAI[0];

        public NPC Parent => Main.npc[(int)Projectile.ai[1]];

        public override void AI()
        {
            if (!Parent.active || Parent.type != NPCType<Glassweaver>())
                Projectile.Kill();

            Projectile.velocity = Parent.velocity;
            Projectile.Center = Parent.Center + new Vector2(30 * Parent.direction, -20);

            Timer++;

            Lighting.AddLight(Projectile.Center, Glassweaver.GlassColor.ToVector3());

            if (Timer > 30)
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
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0)
            {
                Parent.velocity.X = -oldVelocity.X * 0.7f;
                Parent.velocity.Y -= 1;
            }
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {

            return false;
        }

        private void DrawSlashOld()
        {
            Asset<Texture2D> slashTexture = Request<Texture2D>(Texture);
            Rectangle slashFrame = slashTexture.Frame(1, 2, 0, 0);
            Rectangle glowFrame = slashTexture.Frame(1, 2, 0, 1);
            Vector2 origin = glowFrame.Size() * new Vector2(0.55f, 0.5f);

            float scaleX = 1f + Utils.GetLerpValue(12, 28, Timer, true) * 0.12f;
            float scaleY = 1f - Utils.GetLerpValue(12, 28, Timer, true) * 0.05f;

            Color slashColor = Glassweaver.GlassColor * (float)Math.Pow(Utils.GetLerpValue(30, 15, Timer, true), 2) * Utils.GetLerpValue(-2, 1, Timer, true);
            slashColor.A = 0;
            Color glowColor = new Color(255, 255, 255, 0) * (float)Math.Pow(Utils.GetLerpValue(30, 15, Timer, true), 2) * Utils.GetLerpValue(-2, 1, Timer, true);

            float rotOff = (Parent.direction < 0 ? MathHelper.Pi : 0) + (Parent.direction * (Variant % 2 == 0 ? -0.14f : 0.14f));
            Main.EntitySpriteDraw(slashTexture.Value, Projectile.Center - Main.screenPosition, slashFrame, slashColor, Projectile.rotation + rotOff, origin, Projectile.scale * new Vector2(scaleX, scaleY), Direction(), 0);
            Main.EntitySpriteDraw(slashTexture.Value, Projectile.Center - Main.screenPosition, glowFrame, glowColor, Projectile.rotation + rotOff, origin, Projectile.scale * new Vector2(scaleX, scaleY), Direction(), 0);
        }

        private void DrawSword()
        {

        }

        private SpriteEffects Direction()
        {
            SpriteEffects effects = SpriteEffects.None;

            if (Parent.direction < 0)
                effects = SpriteEffects.FlipVertically;

            if (Variant % 2 == 1)
                effects = effects == 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            return effects; 
        }
    }
}
