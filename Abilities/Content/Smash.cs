using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Dusts;
using System;
using System.Runtime.Serialization;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities.Content
{
    public class Smash : Ability
    {
        public override float ActivationCost => 2;

        public override bool Available => base.Available && Player.velocity.Y != 0;
        public override string Texture => "StarlightRiver/Pickups/GaiaFist";

        public float Timer { get; private set; }

        public const int ChargeTime = 30;

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            return abilityKeys.Get<Smash>().JustPressed;
        }

        public override void UpdateActive()
        {
            if (Timer < ChargeTime)
            {
                Player.frozen = true;
                Player.maxFallSpeed = 0.8f;
                Timer++;
                if (Timer == ChargeTime) 
                    Main.PlaySound(SoundID.MaxMana, Player.Center);
            }

            if (Timer == ChargeTime && !StarlightRiver.Instance.AbilityKeys.Get<Smash>().Current)
            {
                Player.velocity.Y -= 20;
                Timer++;
            }

            if (Timer > ChargeTime)
            {
                Timer++;

                Player.maxFallSpeed = 999;
                //player.velocity.X = 0;
                if (Player.velocity.Y < 35) Player.velocity.Y += 2;
                else Player.velocity.Y = 35;

                if (Timer % 10 == 0)
                {
                    Main.PlaySound(SoundID.DD2_BookStaffCast.WithVolume(0.25f + (Timer - 60) / 30f), Player.Center);
                }

                if (Player.position.Y - Player.oldPosition.Y == 0)
                {
                    Deactivate();
                }
            }
            else
            {
                //player.velocity.X = 0;
                Player.velocity.Y = Timer * 2 - 15;
            }

            UpdateEffects();
        }

        private void UpdateEffects()
        {
            if (Timer < ChargeTime)
            {
                for (int k = 0; k < 3; k++)
                {
                    float rot = (Timer * 3 - k) / ((float)(ChargeTime - 1) * 3) * 6.28f;
                    Dust d = Dust.NewDustPerfect(Player.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * 20, 0, default, 0.7f);
                    Dust d2 = Dust.NewDustPerfect(Player.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * 30, 0, default, 0.7f);
                    Dust d3 = Dust.NewDustPerfect(Player.Center, DustType<JungleEnergyFollow>(), Vector2.One.RotatedBy(rot) * (25 + (float)Math.Sin(rot * 8f) * 5), 0, default, 0.7f);
                    d.customData = Player;
                    d2.customData = Player;
                    d3.customData = Player;
                }
                Lighting.AddLight(Player.Center, new Vector3(0.3f, 0.6f, 0.2f) * (Timer / (float)ChargeTime));
            }

            if (Timer == ChargeTime)
            {
                Dust.NewDustPerfect(Player.Center + Vector2.One.RotatedByRandom(6.28f), DustType<JungleEnergy>(), Vector2.One.RotatedByRandom(6.28f), 200, default, 0.8f);
                Lighting.AddLight(Player.Center, new Vector3(0.3f, 0.6f, 0.2f));
            }

            if (Timer > ChargeTime)
            {
                for (int k = 0; k <= 5; k++)
                {
                    Dust.NewDustPerfect(Player.Center + new Vector2(0, 64), DustType<Grass>(), new Vector2(0, -k).RotatedByRandom(0.5f), 0, default, 2);

                    Dust.NewDustPerfect(Player.Center + Vector2.Normalize(Player.velocity) * 8 * k, DustType<JungleEnergy>(), new Vector2(0, -1).RotatedBy(-1), 200);
                    Dust.NewDustPerfect(Player.Center + Vector2.Normalize(Player.velocity) * 8 * k, DustType<JungleEnergy>(), new Vector2(0, -1).RotatedBy(1), 200);
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
            Timer = 0;

            int power = (Timer > 120) ? 12 : (int)(Timer / 120f * 12);
            for (float k = 0; k <= 6.28; k += 0.1f - (power * 0.005f))
            {
                Dust.NewDust(Player.Center, 1, 1, DustType<Stone>(), (float)Math.Cos(k) * power, (float)Math.Sin(k) * power, 0, default, 0.5f + power / 7f);
                Dust.NewDust(Player.Center - new Vector2(Player.height / 2, -32), Player.height, Player.height, DustType<Grass>(), (float)Math.Cos(k) * power * 0.75f, (float)Math.Sin(k) * power * 0.75f, 0, default, 0.5f + power / 7f);
            }

            Main.PlaySound(SoundID.Item70, Player.Center);
            Main.PlaySound(SoundID.DD2_BetsysWrathImpact, Player.Center);
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Player.Center);

            Player.GetModPlayer<StarlightPlayer>().Shake = power;

            Player.velocity.X = 0;
            Player.velocity.Y = 0;
        }
    }
}