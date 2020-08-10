using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.Dummies
{
    internal class SealDummy : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
        }

        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 200;
            projectile.height = 32;
            projectile.aiStyle = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            foreach (Player player in Main.player)
            {
                if (AbilityHelper.CheckSmash(player, projectile.Hitbox))
                {
                    if (!StarlightWorld.SealOpen)
                    {
                        StarlightWorld.SealOpen = true;
                        player.GetModPlayer<AbilityHandler>().smash.OnExit();
                        player.GetModPlayer<AbilityHandler>().smash.Active = false;
                        player.GetModPlayer<StarlightPlayer>().Shake = 80;

                        Main.PlaySound(SoundID.NPCDeath59);
                        Main.PlaySound(SoundID.Item123);

                        for (float k = 0; k <= 3.14f; k += 0.02f)
                        {
                            Dust.NewDustPerfect(projectile.Center, DustID.Stone, new Vector2(-1, 0).RotatedBy(k) * Main.rand.Next(0, 150) * 0.1f, 0, default, 2.5f);
                            Dust.NewDustPerfect(projectile.Center, DustID.WitherLightning, new Vector2(-1, 0).RotatedBy(k) * Main.rand.Next(0, 150) * 0.2f, 0, Color.Blue, 2.5f);
                        }

                        player.velocity.Y = -20;
                    }
                }
            }
            projectile.timeLeft = 2;

            if (Main.tile[(int)projectile.Center.X / 16, (int)(projectile.Center.Y) / 16].type != TileType<Tiles.Void.Seal>())
            {
                projectile.timeLeft = 0;
            }
        }
    }
}