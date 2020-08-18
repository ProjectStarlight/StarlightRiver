using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Dusts;
using System;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    [DataContract]
    public class Smash : Ability
    {
        public Smash(Player player) : base(2, player)
        {
        }

        public override bool Available => User.velocity.Y != 0;
        public override Texture2D Texture => GetTexture("StarlightRiver/Pickups/GaiaFist");
        public const int ChargeTime = 30;

        public override void OnCast()
        {
            Timer = 0;
        }

        public override void UpdateActive()
        {
            if (Timer < ChargeTime)
            {
                if (StarlightRiver.Smash.Current && User.velocity.Y != 0)
                {
                    User.frozen = true;
                    User.maxFallSpeed = 0.8f;
                    Timer++;
                    if (Timer == ChargeTime) Main.PlaySound(SoundID.MaxMana, User.Center);
                }
                else
                {
                    Active = false;
                }
            }

            if (Timer == ChargeTime)
            {
                if (!StarlightRiver.Smash.Current)
                {
                    User.velocity.Y -= 20;
                    Timer++;
                }
                else
                {
                    User.frozen = true;
                    User.maxFallSpeed = 0.5f;
                }
            }

            if (Timer > ChargeTime)
            {
                Timer++;

                User.maxFallSpeed = 999;
                //player.velocity.X = 0;
                if (User.velocity.Y < 35) User.velocity.Y += 2;
                else User.velocity.Y = 35;

                if (Timer % 10 == 0)
                {
                    Main.PlaySound(SoundID.DD2_BookStaffCast.WithVolume(0.25f + (Timer - 60) / 30f), User.Center);
                }

                if (User.position.Y - User.oldPosition.Y == 0)
                {
                    Active = false;
                    OnExit();
                }
            }
            else
            {
                //player.velocity.X = 0;
                User.velocity.Y = Timer * 2 - 15;
            }
        }

        public override void UpdateEffects()
        {
            if (Timer < ChargeTime)
            {
                for (int k = 0; k < 3; k++)
                {
                    float rot = (Timer * 3 - k) / ((float)(ChargeTime - 1) * 3) * 6.28f;
                    Dust d = Dust.NewDustPerfect(User.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * 20, 0, default, 0.7f);
                    Dust d2 = Dust.NewDustPerfect(User.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * 30, 0, default, 0.7f);
                    Dust d3 = Dust.NewDustPerfect(User.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * (25 + (float)Math.Sin(rot * 8f) * 5), 0, default, 0.7f);
                    d.customData = User;
                    d2.customData = User;
                    d3.customData = User;
                }
                Lighting.AddLight(User.Center, new Vector3(0.3f, 0.6f, 0.2f) * (Timer / (float)ChargeTime));
            }

            if (Timer == ChargeTime)
            {
                Dust.NewDustPerfect(User.Center + Vector2.One.RotatedByRandom(6.28f), DustType<JungleEnergy>(), Vector2.One.RotatedByRandom(6.28f), 200, default, 0.8f);
                Lighting.AddLight(User.Center, new Vector3(0.3f, 0.6f, 0.2f));
            }

            if (Timer > ChargeTime)
            {
                for (int k = 0; k <= 5; k++)
                {
                    Dust.NewDustPerfect(User.Center + new Vector2(0, 64), DustType<Grass>(), new Vector2(0, -k).RotatedByRandom(0.5f), 0, default, 2);

                    Dust.NewDustPerfect(User.Center + Vector2.Normalize(User.velocity) * 8 * k, DustType<JungleEnergy>(), new Vector2(0, -1).RotatedBy(-1), 200);
                    Dust.NewDustPerfect(User.Center + Vector2.Normalize(User.velocity) * 8 * k, DustType<JungleEnergy>(), new Vector2(0, -1).RotatedBy(1), 200);
                }
            }
            else
            {
                //float rot = Main.rand.NextFloat(6.28f);
                //Dust.NewDustPerfect(player.Center + Vector2.One.RotatedBy(rot) * 40, ModContent.DustType<JungleEnergy>(), Vector2.One.RotatedBy(rot) * -2f, 0, default, 0.3f);
            }
        }

        public override void OnExit()
        {
            int power = (Timer > 120) ? 12 : (int)(Timer / 120f * 12);
            for (float k = 0; k <= 6.28; k += 0.1f - (power * 0.005f))
            {
                Dust.NewDust(User.Center, 1, 1, DustType<Stone>(), (float)Math.Cos(k) * power, (float)Math.Sin(k) * power, 0, default, 0.5f + power / 7f);
                Dust.NewDust(User.Center - new Vector2(User.height / 2, -32), User.height, User.height, DustType<Grass>(), (float)Math.Cos(k) * power * 0.75f, (float)Math.Sin(k) * power * 0.75f, 0, default, 0.5f + power / 7f);
            }

            Main.PlaySound(SoundID.Item70, User.Center);
            Main.PlaySound(SoundID.DD2_BetsysWrathImpact, User.Center);
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, User.Center);

            User.GetModPlayer<StarlightPlayer>().Shake = power;

            User.velocity.X = 0;
            User.velocity.Y = 0;
        }
    }
}