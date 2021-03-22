using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.Vitric;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class PlayerShield : ModProjectile
    {
        public override string Texture => AssetDirectory.GlassBoss + Name;

        public ref float Timer => ref projectile.ai[0];

        public override void SetDefaults()
        {
            projectile.timeLeft = 2;
            projectile.width = 20;
            projectile.height = 20;
            projectile.scale = 0;
            projectile.friendly = true;
        }

        public override void AI()
        {
            projectile.timeLeft = 2;
            Timer++;

            if (Timer <= 120)
                projectile.scale += 1 / 120f;

            if (Timer > 500)
                projectile.timeLeft = 0;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = ModContent.GetTexture(Texture);
            var color = VitricSummonOrb.MoltenGlow(Timer / 3f);

            if (Timer >= 60)
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.Teal, 0, Vector2.One * 16, projectile.scale, 0, 0);

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color, 0, Vector2.One * 16, projectile.scale, 0, 0);


        }
    }
}
