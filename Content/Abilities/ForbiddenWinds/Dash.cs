using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	public class Dash : CooldownAbility, ILoadable
    {
        public const int DEFAULTTIME = 15;

        public int Time;
        public int EffectTimer;

        private List<Vector2> cache;
        private Trail trail;

        public override float ActivationCostDefault => 1;
        public override string Texture => "StarlightRiver/Assets/Abilities/ForbiddenWinds";
        public override Color Color => new Color(188, 255, 246);

        public override int CooldownMax => 80;

        public Vector2 Dir { get; private set; }
        public Vector2 Vel { get; private set; }
        public float Speed { get; set; }
        public float Boost { get; set; }

        public float Priority => 1;

		public void Load()
		{
            StarlightPlayer.PostUpdateEvent += UpdatePlayerFrame;
		}

        public void Unload() { }

        public static float SignedLesserBound(float limit, float other)
        {
            if (limit < 0) return Math.Min(limit, other);
            if (limit > 0) return Math.Max(limit, other);

            return other;
        }

        public static Vector2 SignedLesserBound(Vector2 limit, Vector2 other)
        {
            return new Vector2(SignedLesserBound(limit.X, other.X), SignedLesserBound(limit.Y, other.Y));
        }

        public void SetVelocity()
        {
            Vel = SignedLesserBound(Dir * Speed * Boost, Player.velocity); // "conservation of momentum" (lol)
        }

        public override void Reset()
        {
            Boost = 0.25f;
            Speed = 28;
            Time = DEFAULTTIME;
            CooldownBonus = 0;
        }

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            if (abilityKeys.Get<Dash>().JustPressed && triggers.DirectionsRaw != default)
            {
                Dir = Vector2.Normalize(triggers.DirectionsRaw);

                if (Player.HasBuff(BuffID.Confused))
                    Dir = new Vector2(Dir.X * -1, Dir.Y);

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
            EffectTimer = 45;
        }

		public override void OnDeactivate()
		{
            Player.UpdateRotation(0);
        }

		public override void UpdateActive()
        {
            base.UpdateActive();

            ManageCaches();

            var progress = Time > 7 ? 1 - (Time - 7) / 8f : 1;

            Player.velocity = SignedLesserBound((Dir * Speed) * progress, Player.velocity * progress); // "conservation of momentum"

            Player.frozen = true;
            Player.gravity = 0;
            Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, Speed);

            if (Time-- <= 0) 
                Deactivate();        
        }

        public override void UpdateActiveEffects()
        {
            if (Time >= 8)
                return;

            Vector2 prevPos = Player.Center + Vector2.Normalize(Player.velocity) * 10;
            int direction = Time % 2 == 0 ? -1 : 1;

            for (int k = 0; k < 60; k++)
            {
                float rot = 0.1f * k * direction + 3.14f;
                Dust dus = Dust.NewDustPerfect(
                    prevPos + Vector2.Normalize(Player.velocity).RotatedBy(rot) * (k / 2) * (0.8f - Time / 11f),
                    DustType<AirDash>(),
                    Vector2.UnitX
                    );
                dus.fadeIn = (k) + Time * 3;
            }
        }

		public override void DrawActiveEffects(SpriteBatch spriteBatch)
		{
            if (!Main.gameMenu && EffectTimer < 44 && EffectTimer > 0)
            {
				DrawPrimitives();
            }
		}

		public void UpdatePlayerFrame(Player player)
		{
            if(player.GetHandler().ActiveAbility is Dash)
			{
                var dash = player.GetHandler().ActiveAbility as Dash;

                player.bodyFrame = new Rectangle(0, 56 * 3, 40, 56);
                player.UpdateRotation(dash.Time / 15f * 6.28f);

                if(dash.Time == 15 || player.dead)
                    player.UpdateRotation(0);
            }
		}

		public override void UpdateFixed()
		{
            if (EffectTimer > 0 && cache != null)
            {
                ManageTrail();
                EffectTimer--;
            }

            base.UpdateFixed();
		}

		public override void CooldownFinish()
        {
            for (int k = 0; k <= 60; k++)
            {
                Dust dus = Dust.NewDustPerfect(Player.Center + Vector2.One.RotatedBy(k / 60f * 6.28f) * Main.rand.NextFloat(50), DustType<AirLegacyWindsAnimation>(), Vector2.Zero);
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

        private void ManageCaches()
        {
            if (Time == 14)
                cache?.Clear();

            if (cache == null || cache.Count < 14)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 14; i++)
                {
                    cache.Add(Player.Center + Player.velocity * 3);
                }
            }

            cache.Add(Player.Center + Player.velocity * 3);

            while (cache.Count > 14)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 14, new TriangularTip(40 * 4), factor => Math.Min(factor * 50, 40), factor =>
            {
                if (factor.X >= 0.80f)
                    return Color.White * 0;

                return new Color(140, 150 + (int)(105 * factor.X), 255) * factor.X * (float)Math.Sin(EffectTimer / 45f * 3.14f);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Player.Center + Player.velocity * 6;
        }

        public void DrawPrimitives()
        {
            Main.spriteBatch.End();

            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1); 

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.01f);
            effect.Parameters["repeats"].SetValue(1f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/FireTrail"));

            trail?.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}