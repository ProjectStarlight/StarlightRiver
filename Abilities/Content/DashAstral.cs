//using Microsoft.Xna.Framework;
//using StarlightRiver.Dusts;
//using System.Runtime.Serialization;
//using Terraria;
//using static Terraria.ModLoader.ModContent;

//namespace StarlightRiver.Abilities.Content
//{
//    public class DashAstral : Dash
//    {
//        public override void OnActivate()
//        {
//            base.OnActivate();
//            Speed = 44;
//        }

//        protected override void UpdateEffects()
//        {
//            if (Player.velocity.LengthSquared() > 6*6)
//            {
//                for (int k = 0; k <= 10; k++)
//                {
//                    Dust.NewDust(Player.Center - new Vector2(Player.height / 2, Player.height / 2), Player.height, Player.height, DustType<Starlight>(), -10 * Vector2.Normalize(Player.velocity).X, -10 * Vector2.Normalize(Player.velocity).Y, 0, default, 0.75f);
//                    Dust.NewDustPerfect(Player.Center + Vector2.Normalize(Player.velocity) * Main.rand.Next(-100, 0), DustType<Starlight>(), Vector2.Normalize(Player.velocity).RotatedBy(1) * (Main.rand.Next(-20, -5) + time * -3), 0, default, 1 - time * 0.1f);
//                    Dust.NewDustPerfect(Player.Center + Vector2.Normalize(Player.velocity) * Main.rand.Next(-100, 0), DustType<Starlight>(), Vector2.Normalize(Player.velocity).RotatedBy(-1) * (Main.rand.Next(-20, -5) + time * -3), 0, default, 1 - time * 0.1f);
//                } // TODO fix vfx probably
//            }
//        }
//    }
//}