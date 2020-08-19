//using Microsoft.Xna.Framework;
//using StarlightRiver.Dusts;
//using System.Runtime.Serialization;
//using Terraria;
//using Terraria.ID;
//using static Terraria.ModLoader.ModContent;

//namespace StarlightRiver.Abilities.Content
//{
//    public class DashCombo : DashAstral
//    {
//        public override void OnActivate()
//        {
//            Projectile proj = Main.projectile[Projectile.NewProjectile(new Vector2(Player.position.X - 21, Player.position.Y - 12), Vector2.Zero, ProjectileType<Projectiles.Ability.DashFire>(), 10, 1f)];
//            proj.owner = Player.whoAmI;
//        }

//        protected override void UpdateEffects()
//        {
//            for (int k = 0; k <= 15; k++)
//            {
//                Dust.NewDustPerfect(Player.Center + Vector2.Normalize(Player.velocity) * Main.rand.Next(0, 70), DustType<FireDust>(), -Player.velocity * Main.rand.NextFloat(-2, 5) + new Vector2(Main.rand.Next(-1, 2), Main.rand.Next(-1, 2)), 0, default, 3f);

//                Dust.NewDustPerfect(Player.Center + Vector2.Normalize(Player.velocity) * Main.rand.Next(-100, 0), DustType<FireDust2>(), Vector2.Normalize(Player.velocity).RotatedBy(1) * (Main.rand.Next(-20, -5) + time * -3), 0, default, 2 - time * 0.2f);
//                Dust.NewDustPerfect(Player.Center + Vector2.Normalize(Player.velocity) * Main.rand.Next(-100, 0), DustType<FireDust2>(), Vector2.Normalize(Player.velocity).RotatedBy(-1) * (Main.rand.Next(-20, -5) + time * -3), 0, default, 2 - time * 0.2f);
//                Dust.NewDustPerfect(Player.Center + Player.velocity * Main.rand.NextFloat(0, 2), DustType<Stamina>(), Player.velocity.RotatedBy((Main.rand.Next(2) == 0) ? 2.8f : 3.48f) * Main.rand.NextFloat(0, 0.25f), 0, default, 1.95f);
//            }
//        }

//        public override void OnExit()
//        {
//            Player.velocity.X *= 0.15f;
//            Player.velocity.Y *= 0.15f;
//        }
//    }
//}