using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class VitricBomb : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.GlassBoss + Name;

        public override void SetDefaults()
        {
            projectile.width = 46;
            projectile.height = 46;
            projectile.hostile = true;
            projectile.timeLeft = 300;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle rect = new Rectangle(0, 46 * projectile.frame, 46, 46);
            spriteBatch.Draw(GetTexture(Texture), projectile.Center - Main.screenPosition, rect, lightColor, 0, Vector2.One * 23, 1, 0, 0);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture(Texture + "Glow");
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Helper.IndicatorColor, 0, tex.Size() / 2, 1, 0, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex2 = GetTexture(AssetDirectory.GlassBoss + "BombTell");

            float bright = (300 - projectile.timeLeft) / 300f * 0.9f;
            if (projectile.timeLeft < 60) bright += (float)Math.Sin(StarlightWorld.rottime * 6) * 0.1f;
            spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, tex2.Frame(), (projectile.timeLeft < 60 ? Color.Red : Color.White) * bright, 0, tex2.Size() / 2, 2, 0, 0);
        }

        public override bool CanHitPlayer(Player target)
        {
            if (Abilities.AbilityHelper.CheckDash(target, projectile.Hitbox))
            {
                projectile.active = false;
                for (int k = 0; k < 20; k++) Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.GlassGravity>());
                Item.NewItem(projectile.Center, ItemID.Heart);
                Main.PlaySound(SoundID.Shatter, projectile.Center);
                return false;
            }
            return true;
        }

        public override void AI()
        {
            projectile.velocity *= 0.97f;
            if (projectile.timeLeft % 2 == 0)
            {
                projectile.frame++;
                if (projectile.frame >= 8) projectile.frame = 0;
            }
        }
        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, projectile.Center);
            for (int k = 0; k < 80; k++)
                Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4));
            foreach (Player player in Main.player.Where(n => Vector2.Distance(n.Center, projectile.Center) < 400))
                player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(player.whoAmI, projectile.whoAmI), 60, 0);
        }
    }
}
