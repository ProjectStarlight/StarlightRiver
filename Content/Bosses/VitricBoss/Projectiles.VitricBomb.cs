using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	class VitricBomb : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.VitricBoss + Name;

		public override bool Autoload(ref string name)
		{
            for(int k = 0; k < 4; k++)
                mod.AddGore(AssetDirectory.VitricBoss + "Gore/Mine" + k);

            return base.Autoload(ref name);
		}

		public override void SetDefaults()
        {
            projectile.width = 46;
            projectile.height = 48;
            projectile.hostile = true;
            projectile.timeLeft = 300;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle rect = new Rectangle(0, 48 * projectile.frame, 46, 48);
            spriteBatch.Draw(GetTexture(Texture), projectile.Center - Main.screenPosition, rect, lightColor * 4, 0, Vector2.One * 23, 1, 0, 0);
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture(Texture + "Glow");
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), Helper.IndicatorColor, 0, tex.Size() / 2, 1, 0, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
            Texture2D tex2 = GetTexture(AssetDirectory.VitricBoss + "BombTell");

            float bright = (300 - projectile.timeLeft) / 300f * 0.7f;
            if (projectile.timeLeft < 60) bright += (float)Math.Sin(StarlightWorld.rottime * 6) * 0.12f;
            spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, tex2.Frame(), (projectile.timeLeft < 60 ? new Color(255, 100, 50) : new Color(210, 200, 240)) * bright, 0, tex2.Size() / 2, 2, 0, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, tex.Frame(), (projectile.timeLeft < 60 ? new Color(255, 200, 50) : new Color(220, 255, 255)) * bright, 0, tex.Size() / 2, 5, 0, 0);
        }

        public override bool CanHitPlayer(Player target)
        {
            if (Abilities.AbilityHelper.CheckDash(target, projectile.Hitbox))
            {
                projectile.active = false;

                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.GlassGravity>());
                }

                Item.NewItem(projectile.Center, ItemID.Heart);
                Main.PlaySound(SoundID.Shatter, projectile.Center);
                return false;
            }
            return true;
        }

        public override void AI()
        {
            projectile.velocity *= 0.97f;
            if (projectile.timeLeft % 4 == 0)
            {
                projectile.frame++;
                if (projectile.frame >= 8) projectile.frame = 0;
            }
        }
        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, projectile.Center);
            Main.PlaySound(SoundID.DD2_KoboldExplosion, projectile.Center);
            Helper.PlayPitched("Magic/FireHit", 0.5f, 0, projectile.Center);

            for (int k = 0; k < 80; k++)
            {
                Dust.NewDustPerfect(projectile.Center, DustType<Dusts.LavaSpark>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, new Color(255, 155, 0), Main.rand.NextFloat(0.1f, 0.8f));
            }

            for (int k = 0; k < 60; k++)
            {
                var velocity = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(30);
                var scale = Main.rand.NextFloat(1.2f, 2.7f);

                var d = Dust.NewDustPerfect(projectile.Center, DustType<Dusts.GlassAttracted>(), velocity, Scale: scale);
                d.customData = projectile.Center;

                var d2 = Dust.NewDustPerfect(projectile.Center, DustType<Dusts.GlassAttractedGlow>(), velocity, Scale: scale);
                d2.customData = projectile.Center;
                d2.frame = d.frame;
            }

            for (int k = 0; k < 4; k++)
            {
                Gore.NewGore(projectile.Center, Vector2.One.RotatedByRandom(6.28f) * 5, ModGore.GetGoreSlot(AssetDirectory.VitricBoss + "Gore/Mine" + k));
                Projectile.NewProjectile(projectile.Center, Vector2.UnitY.RotatedByRandom(1) * -Main.rand.NextFloat(3, 5), ProjectileType<Items.Vitric.NeedlerEmber>(), 0, 0, 0);
            }

            foreach (Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, projectile.Center) < 1500))
            {
                player.GetModPlayer<StarlightPlayer>().Shake += 15;
            }

            foreach (Player player in Main.player.Where(n => Vector2.Distance(n.Center, projectile.Center) < 400))
            {
                player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(player.whoAmI, projectile.whoAmI), projectile.damage, 0);
            }
        }
    }
}
