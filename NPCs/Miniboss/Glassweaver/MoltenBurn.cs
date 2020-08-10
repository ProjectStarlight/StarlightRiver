using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    class MoltenBurn : ModProjectile, IDrawAdditive
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 240;
            projectile.tileCollide = false;
            projectile.damage = 5;
        }

        public override void AI()
        {
            Tile tile = Main.tile[(int)projectile.Center.X / 16, (int)projectile.Center.Y / 16];
            if (!tile.active() || tile.collisionType != 1) projectile.timeLeft = 0;

            Lighting.AddLight(projectile.Center, new Vector3(1.1f, 0.2f, 0.2f) * (projectile.timeLeft / 180f));
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Tile tile = Main.tile[(int)projectile.Center.X / 16, (int)projectile.Center.Y / 16];
            Texture2D tex = Main.tileTexture[tile.type];
            Rectangle frame = new Rectangle(tile.frameX, tile.frameY, 16, 16);
            Vector2 pos = projectile.position + Vector2.One - Main.screenPosition;
            Color color = new Color(255, 50, 50) * 0.2f * (projectile.timeLeft / 180f);

            spriteBatch.Draw(tex, pos, frame, color, 0, Vector2.Zero, 1, 0, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Keys/Glow");
            Color color = new Color(255, 50, 50) * 0.3f * (projectile.timeLeft / 180f);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), color, 0, tex.Size() / 2, 1.2f * (projectile.timeLeft / 180f), 0, 0);
        }
    }
}
