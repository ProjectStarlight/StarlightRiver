using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
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

		public override void Load()
		{
            for(int k = 0; k < 4; k++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricBoss + "Gore/Mine" + k);
		}

		public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 48;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Rectangle rect = new Rectangle(0, 48 * Projectile.frame, 46, 48);
            Main.spriteBatch.Draw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, rect, lightColor * 4, 0, Vector2.One * 23, 1, 0, 0);

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), Helper.IndicatorColor, 0, tex.Size() / 2, 1, 0, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.VitricBoss + "BombTell").Value;

            float bright = (300 - Projectile.timeLeft) / 300f * 0.7f;

            if (Projectile.timeLeft < 60) 
                bright += (float)Math.Sin(StarlightWorld.rottime * 6) * 0.12f;

            spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, tex2.Frame(), (Projectile.timeLeft < 60 ? new Color(255, 100, 50) : new Color(210, 200, 240)) * bright, 0, tex2.Size() / 2, 2, 0, 0);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), (Projectile.timeLeft < 60 ? new Color(255, 200, 50) : new Color(220, 255, 255)) * bright, 0, tex.Size() / 2, 5, 0, 0);
        }

        public override bool CanHitPlayer(Player target)
        {
            var dashBox = new Rectangle(Projectile.Hitbox.X - 10, Projectile.Hitbox.Y - 10, Projectile.Hitbox.Width + 20, Projectile.Hitbox.Height + 20);

            if (Abilities.AbilityHelper.CheckDash(target, dashBox))
            {
                Projectile.active = false;

                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.GlassGravity>());
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ItemID.Heart);

                Projectile.netUpdate = true;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
            }

            return false;
        }

        public override void AI()
        {
            if (Main.rand.Next(27) == 0)
            {
                if (Main.rand.NextBool())
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CrystalSparkle>(), 0, 0);
                else
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CrystalSparkle2>(), 0, 0);
            }

            Projectile.velocity *= 0.97f;

            if (Projectile.timeLeft % 4 == 0)
            {
                Projectile.frame++;

                if (Projectile.frame >= 8) 
                    Projectile.frame = 0;
            }

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player Player = Main.player[i];

                if (Player.active)
                    CanHitPlayer(Player);
            }
        }

        public override void Kill(int timeLeft)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, Projectile.Center);
            Helper.PlayPitched("Magic/FireHit", 0.5f, 0, Projectile.Center);

            for (int k = 0; k < 80; k++)
            {
                Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.LavaSpark>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(6), 0, new Color(255, 155, 0), Main.rand.NextFloat(0.1f, 0.8f));
            }

            for (int k = 0; k < 60; k++)
            {
                var velocity = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(30);
                var scale = Main.rand.NextFloat(1.2f, 2.7f);

                var d = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.GlassAttracted>(), velocity, Scale: scale);
                d.customData = Projectile.Center;

                var d2 = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.GlassAttractedGlow>(), velocity, Scale: scale);
                d2.customData = Projectile.Center;
                d2.frame = d.frame;
            }

            for (int k = 0; k < 4; k++)
            {
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Vector2.One.RotatedByRandom(6.28f) * 5, Mod.Find<ModGore>("Mine" + k).Type);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitY.RotatedByRandom(1) * -Main.rand.NextFloat(3, 5), ProjectileType<Items.Vitric.NeedlerEmber>(), 0, 0, 0);
            }

            foreach (Player Player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, Projectile.Center) < 1500))
            {
                Core.Systems.CameraSystem.Shake += 15;
            }

            foreach (Player Player in Main.player.Where(n => Vector2.Distance(n.Center, Projectile.Center) < 400))
            {
                Player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(Player.whoAmI, Projectile.whoAmI), Main.expertMode ? Projectile.damage * 2 : Projectile.damage, 0);
            }

            if(Main.masterMode)
			{
                for (int k = 0; k < 8; k++)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(k / 8f * 6.28f) * 18, ProjectileType<TelegraphedGlassSpike>(), 20, 1, Main.myPlayer);
			}
        }
    }
}
