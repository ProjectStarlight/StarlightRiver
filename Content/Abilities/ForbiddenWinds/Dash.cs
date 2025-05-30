﻿using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	public class Dash : CooldownAbility, IOrderedLoadable
	{
		public int Time;
		public int maxTime = 15;
		public int EffectTimer;

		protected List<Vector2> cache;
		protected Trail trail;

		public override string Name => "Forbidden Winds";
		public override string Tooltip
		{
			get
			{
				List<string> ab = StarlightRiver.Instance.AbilityKeys.Get<Dash>().GetAssignedKeys();
				string def = "Unbound";

				return $"Press [c/6699FF:{(ab.Count > 0 ? ab[0] : def)}] while holding any combination of [c/6699FF:{UIHelper.GetVanillaInputString("Up")}], [c/6699FF:{UIHelper.GetVanillaInputString("Left")}], [c/6699FF:{UIHelper.GetVanillaInputString("Down")}], and [c/6699FF:{UIHelper.GetVanillaInputString("Right")}] to channel starlight into a powerful dash.\n\nYou'll strike with enough force to shatter any object with a [c/88FFFF:glowing blue outline!]";
			}
		}

		public override float ActivationCostDefault => 1;

		public override Asset<Texture2D> Texture => Assets.Abilities.ForbiddenWinds;
		public override Asset<Texture2D> PreviewTexture => Assets.Abilities.ForbiddenWindsPreview;
		public override Asset<Texture2D> PreviewTextureOff => Assets.Abilities.ForbiddenWindsPreviewOff;

		public override Color Color => new(188, 255, 246);

		public override int CooldownMax => 80;

		public Vector2 Dir { get; set; }
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
			if (limit < 0)
				return Math.Min(limit, other);
			if (limit > 0)
				return Math.Max(limit, other);

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
			Time = maxTime = 15;
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

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, Player.Center);
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item104, Player.Center);
			EffectTimer = 45;
		}

		public override void OnDeactivate()
		{
			Player.UpdateRotation(0);
		}

		public override void UpdateActive()
		{
			base.UpdateActive();

			if (Main.netMode != NetmodeID.Server)
				ManageCaches();

			float progress = Time > 7 ? 1 - (Time - 7) / 8f : 1;

			Player.velocity = SignedLesserBound(Dir * Speed * progress, Player.velocity * progress); // "conservation of momentum"

			//Player.frozen = true;
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
				var dus = Dust.NewDustPerfect(
					prevPos + Vector2.Normalize(Player.velocity).RotatedBy(rot) * (k / 2) * (0.8f - Time / 11f),
					DustType<AirDash>(),
					Vector2.UnitX
					);
				dus.fadeIn = k + Time * 3;
			}
		}

		public override void DrawActiveEffects(SpriteBatch spriteBatch)
		{
			if (!Main.gameMenu && EffectTimer < 44 && EffectTimer > 0)
			{
				DrawPrimitives();
			}
		}

		public void UpdatePlayerFrame(Player Player)
		{
			if (Player.GetHandler().ActiveAbility is Dash && !Player.GetHandler().ActiveAbility.GetType().IsSubclassOf(typeof(Dash)))
			{
				var dash = Player.GetHandler().ActiveAbility as Dash;

				Player.bodyFrame = new Rectangle(0, 56 * 3, 40, 56);
				Player.UpdateRotation(dash.Time / (float)dash.maxTime * 6.28f);

				if (dash.Time == dash.maxTime || Player.dead)
					Player.UpdateRotation(0);
			}
		}

		public override void SafeUpdateFixed()
		{
			if (EffectTimer > 0 && cache != null)
			{
				if (Main.netMode != NetmodeID.Server)
					ManageTrail();
				EffectTimer--;
			}
		}

		public override void CooldownFinish()
		{
			for (int k = 0; k <= 60; k++)
			{
				var dus = Dust.NewDustPerfect(Player.Center + Vector2.One.RotatedBy(k / 60f * 6.28f) * Main.rand.NextFloat(50), DustType<AirLegacyWindsAnimation>(), Vector2.Zero);
				dus.customData = Player;
			}

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, Player.Center);
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item25, Player.Center);
		}

		public override void OnExit()
		{
			Player.velocity = Vel;
			Player.fallStart = (int)(Player.position.Y / 16);
			Player.fallStart2 = (int)(Player.position.Y / 16);
		}

		private void ManageCaches()
		{
			if (Time == 15)
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
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 14, new NoTip(), factor => Math.Min(factor * 50, 40), factor =>
							{
								if (factor.X == 1)
									return Color.Transparent;

								return new Color(140, 150 + (int)(105 * factor.X), 255) * factor.X * (float)Math.Sin(EffectTimer / 45f * 3.14f);
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Player.Center + Player.velocity * 6;
		}

		public virtual void DrawPrimitives()
		{
			Main.spriteBatch.End();

			Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

			if (effect != null)
			{
				var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
				Matrix view = Main.GameViewMatrix.TransformationMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.01f);
				effect.Parameters["repeats"].SetValue(1f);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(Assets.FireTrail.Value);

				trail?.Render(effect);
			}

			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}