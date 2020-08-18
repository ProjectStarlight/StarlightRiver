using Microsoft.Xna.Framework;
using StarlightRiver.Dusts;
using System.Runtime.Serialization;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    [DataContract]
    public class DashAstral : Dash
    {
        public DashAstral(Player player) : base(player)
        {
        }

        public override void UpdateActive()
        {
            User.maxFallSpeed = 999;

            Timer--;

            if (X != 0 || Y != 0)
            {
                User.velocity = Vector2.Normalize(new Vector2(X, Y)) * 44;
            }

            if (Vector2.Distance(User.position, User.oldPosition) < 5 && Timer < 4)
            {
                Timer = 0;
                User.velocity *= -0.2f;
            }

            if (Timer <= 0)
            {
                Active = false;
                OnExit();
            }
        }

        public override void UpdateEffects()
        {
            if (User.velocity.Length() > 6)
            {
                for (int k = 0; k <= 10; k++)
                {
                    Dust.NewDust(User.Center - new Vector2(User.height / 2, User.height / 2), User.height, User.height, DustType<Starlight>(), -10 * Vector2.Normalize(User.velocity).X, -10 * Vector2.Normalize(User.velocity).Y, 0, default, 0.75f);
                    Dust.NewDustPerfect(User.Center + Vector2.Normalize(User.velocity) * Main.rand.Next(-100, 0), DustType<Starlight>(), Vector2.Normalize(User.velocity).RotatedBy(1) * (Main.rand.Next(-20, -5) + Timer * -3), 0, default, 1 - Timer * 0.1f);
                    Dust.NewDustPerfect(User.Center + Vector2.Normalize(User.velocity) * Main.rand.Next(-100, 0), DustType<Starlight>(), Vector2.Normalize(User.velocity).RotatedBy(-1) * (Main.rand.Next(-20, -5) + Timer * -3), 0, default, 1 - Timer * 0.1f);
                }
            }
        }
    }
}