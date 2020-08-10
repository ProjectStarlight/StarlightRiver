using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.Dummies
{
    internal class StaminaOrbDummy : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("");
        }

        public override string Texture => "StarlightRiver/Invisible";

        public override void SetDefaults()
        {
            projectile.width = 22;
            projectile.height = 22;
            projectile.aiStyle = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (projectile.localAI[0] > 0) { projectile.localAI[0]--; }
            else
            {
                float rot = Main.rand.NextFloat(0, 6.28f);
                Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Stamina>(), new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * 0.4f, 0, default, 2f);
            }

            foreach (Player player in Main.player.Where(player => Vector2.Distance(player.Center, projectile.Center) <= 100))
            {
                AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

                if (projectile.Hitbox.Intersects(player.Hitbox) && mp.StatStamina < mp.StatStaminaMax && !mp.Abilities.Any(a => a.Active) && projectile.localAI[0] == 0)
                {
                    mp.StatStamina++;
                    projectile.localAI[0] = 300;
                    Main.PlaySound(SoundID.Item112, projectile.Center);
                    CombatText.NewText(player.Hitbox, new Color(255, 170, 60), "+1");

                    for (float k = 0; k <= 6.28; k += 0.1f)
                    {
                        Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Stamina>(), new Vector2((float)Math.Cos(k), (float)Math.Sin(k)) * (Main.rand.Next(25) * 0.1f), 0, default, 3f);
                    }
                }
            }

            projectile.timeLeft = 2;

            if (Main.tile[(int)projectile.Center.X / 16, (int)projectile.Center.Y / 16].type != mod.TileType("StaminaOrb"))
            {
                projectile.timeLeft = 0;
            }
        }
    }
}