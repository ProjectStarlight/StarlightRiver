using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dusts;
using System;
using System.Runtime.Serialization;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities.Content
{
    public class Dash : CooldownAbility
    {
        public int time;

        public override float ActivationCost => 1;
        public override Texture2D Texture => GetTexture("StarlightRiver/Pickups/ForbiddenWinds");

        public override int CooldownMax => 180;

        protected Vector2 dir;

        public float Speed { get; set; }

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            return abilityKeys.Get<Dash>().JustPressed && (dir = triggers.DirectionsRaw) != default;
        }

        public override void OnActivate()
        {
            base.OnActivate();

            Speed = 28;

            Main.PlaySound(SoundID.Item45, Player.Center);
            Main.PlaySound(SoundID.Item104, Player.Center);
        }

        public override void UpdateActive()
        {
            base.UpdateActive();

            Player.maxFallSpeed = 999;
            Player.velocity = Vector2.Normalize(dir) * Speed;

            if (Vector2.DistanceSquared(Player.position, Player.oldPosition) < 5*5 && time < 4)
            {
                Deactivate();
                Player.velocity *= -0.2f;
            }

            UpdateEffects();
        }

        protected virtual void UpdateEffects()
        {
            Vector2 prevPos = Player.Center + Vector2.Normalize(Player.velocity) * 10;
            int direction = time % 2 == 0 ? -1 : 1;
            for (int k = 0; k < 60; k++)
            {
                float rot = (0.1f * k) * direction;
                Dust dus = Dust.NewDustPerfect(prevPos + Vector2.Normalize(Player.velocity).RotatedBy(rot) * (k / 2) * (0.5f + time / 8f), DustType<AirDash>());
                dus.fadeIn = k - time * 3;
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
            Player.velocity.X *= 0.15f;
            Player.velocity.Y *= 0.15f;
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