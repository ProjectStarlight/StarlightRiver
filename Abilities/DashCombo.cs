using Microsoft.Xna.Framework;
using StarlightRiver.Dusts;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    [DataContract]
    public class DashCombo : DashAstral
    {
        public DashCombo(Player player) : base(player)
        {
        }

        public override void OnCast()
        {
            Active = true;
            Main.PlaySound(SoundID.Item45);
            Main.PlaySound(SoundID.Item104);

            Projectile proj = Main.projectile[Projectile.NewProjectile(new Vector2(player.position.X - 21, player.position.Y - 12), Vector2.Zero, ProjectileType<Projectiles.Ability.DashFire>(), 10, 1f)];
            proj.owner = player.whoAmI;

            X = ((player.controlLeft) ? -1 : 0) + ((player.controlRight) ? 1 : 0);
            Y = ((player.controlUp) ? -1 : 0) + ((player.controlDown) ? 1 : 0);
            Timer = 7;
            Cooldown = 90;
        }

        public override void UseEffects()
        {
            for (int k = 0; k <= 15; k++)
            {
                Dust.NewDustPerfect(player.Center + Vector2.Normalize(player.velocity) * Main.rand.Next(0, 70), DustType<FireDust>(), -player.velocity * Main.rand.NextFloat(-2, 5) + new Vector2(Main.rand.Next(-1, 2), Main.rand.Next(-1, 2)), 0, default, 3f);

                Dust.NewDustPerfect(player.Center + Vector2.Normalize(player.velocity) * Main.rand.Next(-100, 0), DustType<FireDust2>(), Vector2.Normalize(player.velocity).RotatedBy(1) * (Main.rand.Next(-20, -5) + Timer * -3), 0, default, 2 - Timer * 0.2f);
                Dust.NewDustPerfect(player.Center + Vector2.Normalize(player.velocity) * Main.rand.Next(-100, 0), DustType<FireDust2>(), Vector2.Normalize(player.velocity).RotatedBy(-1) * (Main.rand.Next(-20, -5) + Timer * -3), 0, default, 2 - Timer * 0.2f);
                Dust.NewDustPerfect(player.Center + player.velocity * Main.rand.NextFloat(0, 2), DustType<Stamina>(), player.velocity.RotatedBy((Main.rand.Next(2) == 0) ? 2.8f : 3.48f) * Main.rand.NextFloat(0, 0.25f), 0, default, 1.95f);
            }
        }

        public override void OnExit()
        {
            player.velocity.X *= 0.15f;
            player.velocity.Y *= 0.15f;
        }
    }
}