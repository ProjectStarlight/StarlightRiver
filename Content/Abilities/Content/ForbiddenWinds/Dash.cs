using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dusts;
using Steamworks;
using System;
using System.Runtime.Serialization;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities.Content.ForbiddenWinds
{
    public class Dash : CooldownAbility
    {
        public static float SignedLesserBound(float limit, float other)
        {
            if (limit < 0) return Math.Min(limit, other);
            if (limit > 0) return Math.Max(limit, other);

            return other;
            // return 0; <-- do this to lock the player's perpendicular momentum when dashing
        }

        public static Vector2 SignedLesserBound(Vector2 limit, Vector2 other)
        {
            return new Vector2(SignedLesserBound(limit.X, other.X), SignedLesserBound(limit.Y, other.Y));
        }

        public int Time;

        public override float ActivationCostDefault => 1;
        public override string Texture => "StarlightRiver/Pickups/ForbiddenWinds";
        public override Color Color => new Color(188, 255, 246);

        public override int CooldownMax => 90;

        public const int defaultTime = 7;

        public Vector2 Dir { get; private set; }
        public Vector2 Vel { get; private set; }
        public float Speed { get; set; }
        public float Boost { get; set; }

        public void SetVelocity()
        {
            Vel = SignedLesserBound(Dir * Speed * Boost, Player.velocity); // "conservation of momentum" (lol)
        }

        public override void Reset()
        {
            Boost = 0.1f;
            Speed = 28;
            Time = defaultTime;
            CooldownBonus = 0;
        }

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            if (abilityKeys.Get<Dash>().JustPressed && triggers.DirectionsRaw != default)
            {
                Dir = Vector2.Normalize(triggers.DirectionsRaw);
                return true;
            }
            return false;
        }

        public override void OnActivate()
        {
            base.OnActivate();

            SetVelocity();

            Main.PlaySound(SoundID.Item45, Player.Center);
            Main.PlaySound(SoundID.Item104, Player.Center);
        }

        public override void UpdateActive()
        {
            base.UpdateActive();

            Player.velocity = SignedLesserBound(Dir * Speed, Player.velocity); // "conservation of momentum"

            Player.frozen = true;
            Player.gravity = 0;
            Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, Speed);

            if (Time-- <= 0) Deactivate();

            // Notable differences with new Ability:
            // - you can tech building momentum
            // - less bug prone
            // - supports gamepad i think
            // - it just.. feels better..

            // Notable issues found with Ability:
            // - new and old:
            //   - you can tech a jump if you hold jump during the ability (I like this, very celeste-y)
            // - old:
            //   - you can double-jump mid dash and f*ck up your dash
        }

        public override void UpdateActiveEffects()
        {
            Vector2 prevPos = Player.Center + Vector2.Normalize(Player.velocity) * 10;
            int direction = Time % 2 == 0 ? -1 : 1;

            for (int k = 0; k < 60; k++)
            {
                float rot = 0.1f * k * direction;
                Dust dus = Dust.NewDustPerfect(
                    prevPos + Vector2.Normalize(Player.velocity).RotatedBy(rot) * (k / 2) * (0.5f + Time / 8f), 
                    DustType<AirDash>(), 
                    Vector2.UnitX.RotatedByRandom(Math.PI) / 3f
                    );
                dus.fadeIn = k - Time * 3;
            }
        }

        public override void CooldownFinish()
        {
            for (int k = 0; k <= 60; k++)
            {
                Dust dus = Dust.NewDustPerfect(Player.Center + Vector2.One.RotatedBy(k / 60f * 6.28f) * Main.rand.NextFloat(50), DustType<Air2>(), Vector2.Zero);
                dus.customData = Player;
            }

            Main.PlaySound(SoundID.Item45, Player.Center);
            Main.PlaySound(SoundID.Item25, Player.Center);
        }

        public override void OnExit()
        {
            Player.velocity = Vel;
            Player.fallStart = (int)(Player.position.Y / 16);
            Player.fallStart2 = (int)(Player.position.Y / 16);
        }

        //public override void OnCastDragon()
        //{
        //    if (Player.velocity.Y == 0) //on the ground, set to zero so the game knows to do the pounce
        //    {
        //        X = Player.direction * 2;
        //        Y = 0;
        //    }
        //    else // jumping/in the air, do the barrel roll
        //    {
        //        X = Vector2.Normalize(Player.Center - Main.MouseWorld).X;
        //        Y = Vector2.Normalize(Player.Center - Main.MouseWorld).Y;
        //    }
        //    cooldownActive = 20;
        //    cooldown = 90;
        //}

        //public override void UpdateDragon()
        //{
        //    cooldownActive--;
        //    if (Math.Abs(X) > 1) //the normalized X should never be greater than 1, so this should be a valid check for the pounce
        //    {
        //        Player.velocity.X = X * 6;
        //        if (cooldownActive == 19) Player.velocity.Y -= 4;
        //    }
        //    else //otherwise, barrelroll
        //    {
        //        Player.velocity = new Vector2(X, Y) * 0.2f * (((10 - cooldownActive) * (10 - cooldownActive)) - 100);
        //    }
        //    if (cooldownActive <= 0)
        //    {
        //        Active = false;
        //        OnExit();
        //    }
        //}

        //public override void UpdateEffectsDragon()
        //{
        //    Dust.NewDust(Player.position, 50, 50, DustType<Air>());
        //    if (Math.Abs(X) < 1)
        //    {
        //        for (int k = 0; k <= 10; k++)
        //        {
        //            float rot = ((cooldownActive - k / 10f) / 10f * 6.28f) + new Vector2(X, Y).ToRotation();
        //            Dust.NewDustPerfect(Vector2.Lerp(Player.Center, Player.Center + Player.velocity, k / 10f) + Vector2.One.RotatedBy(rot) * 30, DustType<Air>(), Vector2.Zero);
        //        }
        //    }
        //}
    }
}