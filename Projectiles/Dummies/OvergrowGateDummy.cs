using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.Dummies
{
    internal class OvergrowGateDummy : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
        }

        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 124;
            projectile.aiStyle = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            foreach (Player player in Main.player.Where(player => Vector2.Distance(player.Center, projectile.Center) <= 100))
            {
                AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

                if (AbilityHelper.CheckDash(player, projectile.Hitbox))
                {
                    WorldGen.KillTile((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16);
                    mp.dash.Active = false;

                    if (player.velocity.Length() != 0)
                    {
                        player.velocity = Vector2.Normalize(player.velocity) * -1f;
                    }
                    Main.PlaySound(SoundID.Shatter, projectile.Center);
                }
            }
            projectile.timeLeft = 2;

            if (Main.tile[(int)projectile.Center.X / 16, (int)projectile.Center.Y / 16].type != TileType<Tiles.Overgrow.OvergrowGate>())
            {
                projectile.timeLeft = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Overgrow/OvergrowGateGlow"), projectile.position + new Vector2(6, 53) - Main.screenPosition, Color.White * (float)Math.Sin(StarlightWorld.rottime));
            return true;
        }
    }
}