using static Terraria.ModLoader.ModContent;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class GlassSlash : ModProjectile
    {
        public GlassMiniboss parent;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Blade");

        public override void SetDefaults()
        {
            projectile.width = 46;
            projectile.height = 30;
            projectile.hostile = true;
            projectile.timeLeft = 20;
            projectile.tileCollide = false;
            projectile.aiStyle = -1;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 20) Main.PlaySound(SoundID.Item65, projectile.Center);
            if (parent != null) projectile.Center = parent.npc.Center + Vector2.UnitX * (projectile.ai[0] == 0 ? -23 : 23);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = projectile.ai[1] == 0 ? GetTexture("StarlightRiver/NPCs/Miniboss/Glassweaver/SlashShort") : GetTexture("StarlightRiver/NPCs/Miniboss/Glassweaver/SlashLong");
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, new Rectangle(0, 62 * (int)(projectile.timeLeft / 20f * (tex.Height / 62 - 1)), 92, 60), Color.White, 0, new Vector2(46, 30), 1, projectile.ai[0] == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }
}
