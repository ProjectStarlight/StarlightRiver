//using Microsoft.Xna.Framework;
//using System.Runtime.Serialization;
//using Terraria;
//using Terraria.ID;
//using static Terraria.ModLoader.ModContent;

//namespace StarlightRiver.Abilities.Content
//{
//    public class DashFlame : Dash
//    {
//        public override void OnActivate()
//        {
//            base.OnActivate();

//            Projectile proj = Main.projectile[Projectile.NewProjectile(new Vector2(Player.position.X - 21, Player.position.Y - 12), Vector2.Zero, ProjectileType<Projectiles.Ability.DashFire>(), 10, 1f)];
//            proj.owner = Player.whoAmI;
//        }

//        protected override void UpdateEffects()
//        {
//            for (int k = 0; k <= 15; k++)
//            {
//                //Dust.NewDust(player.Center - new Vector2(player.height / 2, player.height / 2), player.height, player.height, DustID.Fire);
//                Dust.NewDustPerfect(Player.Center + Player.velocity * Main.rand.NextFloat(0, 2), DustID.Fire, Player.velocity.RotatedBy((Main.rand.Next(2) == 0) ? 2.8f : 3.48f) * Main.rand.NextFloat(0, 0.15f), 0, default, 0.95f);
//                Dust.NewDustPerfect(Player.Center + Vector2.Normalize(Player.velocity) * Main.rand.Next(0, 50), DustType<Dusts.FireDust>(), -Player.velocity * Main.rand.NextFloat(-2, 5) + new Vector2(Main.rand.Next(-1, 2), Main.rand.Next(-1, 2)), 0, default, 2f);
//            }
//        }

//        public override void OnExit()
//        {
//            Player.velocity.X *= 0.15f;
//            Player.velocity.Y *= 0.15f;
//        }
//    }
//}