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
    class Whirlwind : ModProjectile
    {
        public override string Texture => AssetDirectory.Glassweaver + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Spinning Blades");

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 120;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
        }

        public ref float Timer => ref Projectile.ai[0];

        public NPC Parent => Main.npc[(int)Projectile.ai[1]];

        public override void AI()
        {
            if (!Parent.active || Parent.type != NPCType<Glassweaver>())
                Projectile.Kill();

            Projectile.velocity = Parent.velocity;
            Projectile.Center = Parent.Center;

            Timer++;
            if (Timer > 15 && Timer < 20)
                Timer = 16;

            Lighting.AddLight(Projectile.Center, Glassweaver.GlassColor.ToVector3());

            if (Timer > 30)
                Projectile.Kill();
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Rectangle slashBox = projHitbox;
            slashBox.Inflate(30, 10);
            return Timer > 8 && slashBox.Intersects(targetHitbox);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> spinTexture = Request<Texture2D>(Texture);

            Color glowColor = Glassweaver.GlassColor * Utils.GetLerpValue(0, 15, Timer, true) * Utils.GetLerpValue(40, 25, Timer, true);
            glowColor.A = 0;
            Main.EntitySpriteDraw(spinTexture.Value, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, spinTexture.Size() * 0.5f, Projectile.scale * new Vector2(1.5f, 1f), 0, 0);

            return false;
        }
    }
}
