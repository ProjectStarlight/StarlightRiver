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

        public override bool CanUse => player.velocity.Y != 0;
        public override Texture2D Texture => GetTexture("StarlightRiver/Pickups/GaiaFist");
        public const int ChargeTime = 30;

        public override void OnCast()
        {
            Timer = 0;
        }

        public override void InUse()
        {
            if (Timer < ChargeTime)
            {
                if (StarlightRiver.Smash.Current && player.velocity.Y != 0)
                {
                    player.frozen = true;
                    player.maxFallSpeed = 0.8f;
                    Timer++;
                    if (Timer == ChargeTime) Main.PlaySound(SoundID.MaxMana, player.Center);
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
                    player.velocity.Y -= 20;
                    Timer++;
                }
                else
                {
                    player.frozen = true;
                    player.maxFallSpeed = 0.5f;
                }
            }

            if (Timer > ChargeTime)
            {
                Timer++;

                player.maxFallSpeed = 999;
                //player.velocity.X = 0;
                if (player.velocity.Y < 35) player.velocity.Y += 2;
                else player.velocity.Y = 35;

                if (Timer % 10 == 0)
                {
                    Main.PlaySound(SoundID.DD2_BookStaffCast.WithVolume(0.25f + (Timer - 60) / 30f), player.Center);
                }

                if (player.position.Y - player.oldPosition.Y == 0)
                {
                    Active = false;
                    OnExit();
                }
            }
            else
            {
                //player.velocity.X = 0;
                player.velocity.Y = Timer * 2 - 15;
            }
        }

        public override void UseEffects()
        {
            if (Timer < ChargeTime)
            {
                for (int k = 0; k < 3; k++)
                {
                    float rot = (Timer * 3 - k) / ((float)(ChargeTime - 1) * 3) * 6.28f;
                    Dust d = Dust.NewDustPerfect(player.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * 20, 0, default, 0.7f);
                    Dust d2 = Dust.NewDustPerfect(player.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * 30, 0, default, 0.7f);
                    Dust d3 = Dust.NewDustPerfect(player.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * (25 + (float)Math.Sin(rot * 8f) * 5), 0, default, 0.7f);
                    d.customData = player;
                    d2.customData = player;
                    d3.customData = player;
                }
                Lighting.AddLight(player.Center, new Vector3(0.3f, 0.6f, 0.2f) * (Timer / (float)ChargeTime));
            }

            if (Timer == ChargeTime)
            {
                Dust.NewDustPerfect(player.Center + Vector2.One.RotatedByRandom(6.28f), DustType<JungleEnergy>(), Vector2.One.RotatedByRandom(6.28f), 200, default, 0.8f);
                Lighting.AddLight(player.Center, new Vector3(0.3f, 0.6f, 0.2f));
            }

            if (Timer > ChargeTime)
            {
                for (int k = 0; k <= 5; k++)
                {
                    Dust.NewDustPerfect(player.Center + new Vector2(0, 64), DustType<Grass>(), new Vector2(0, -k).RotatedByRandom(0.5f), 0, default, 2);

                    Dust.NewDustPerfect(player.Center + Vector2.Normalize(player.velocity) * 8 * k, DustType<JungleEnergy>(), new Vector2(0, -1).RotatedBy(-1), 200);
                    Dust.NewDustPerfect(player.Center + Vector2.Normalize(player.velocity) * 8 * k, DustType<JungleEnergy>(), new Vector2(0, -1).RotatedBy(1), 200);
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
                Dust.NewDust(player.Center, 1, 1, DustType<Stone>(), (float)Math.Cos(k) * power, (float)Math.Sin(k) * power, 0, default, 0.5f + power / 7f);
                Dust.NewDust(player.Center - new Vector2(player.height / 2, -32), player.height, player.height, DustType<Grass>(), (float)Math.Cos(k) * power * 0.75f, (float)Math.Sin(k) * power * 0.75f, 0, default, 0.5f + power / 7f);
            }

            Main.PlaySound(SoundID.Item70, player.Center);
            Main.PlaySound(SoundID.DD2_BetsysWrathImpact, player.Center);
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, player.Center);

            player.GetModPlayer<StarlightPlayer>().Shake = power;

            player.velocity.X = 0;
            player.velocity.Y = 0;
        }
    }
}