using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Threading;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	public class Dash : CooldownAbility, IOrderedLoadable
	{
		public int Time;
		public int maxTrail;
		public int maxTime = 15;
		public int EffectTimer;
		public float flowTimer;

		public Vector2 lastRealVel;
		public Vector2 goalVel;

		protected List<Vector2> cache;
		protected Trail trail;

		public override string Tooltip
		{
			get
			{
				List<string> ab = StarlightRiver.Instance.AbilityKeys.Get<Dash>().GetAssignedKeys();
				string def = ab.Count > 0 ? ab[0] : "Unbound";

				return UnformattedTooltip.Format(
					def,
					UIHelper.GetVanillaInputString("Up"),
					UIHelper.GetVanillaInputString("Left"),
					UIHelper.GetVanillaInputString("Down"),
					UIHelper.GetVanillaInputString("Right")
				);
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
			EffectTimer = 65;
			flowTimer = 0;
			maxTrail = 0;
		}

		public override void OnDeactivate()
		{
			Player.UpdateRotation(0);
		}

		public override void UpdateActive()
		{
			base.UpdateActive();

			maxTrail++;

			if (Main.netMode != NetmodeID.Server)
				ManageCaches();

			float progress = Time > 7 ? 1 - (Time - 7) / 8f : 1;

			goalVel = SignedLesserBound(Dir * Speed * progress, Player.velocity * progress);
			Player.velocity = goalVel; // "conservation of momentum"

			//Player.frozen = true;
			Player.gravity = 0;
			Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, Speed);

			if (Time-- <= 0)
				Deactivate();
		}

		public override void PostPlayerActiveUpdate()
		{
			lastRealVel = Player.velocity;

			if (((Player.velocity - goalVel).Length() >= 0.2f || Math.Abs(Player.velocity.ToRotation() - Dir.ToRotation()) > 0.05f) && Time < (maxTime - 1))
			{
				SoundEngine.PlaySound(SoundID.Dig, Player.Center);

				Deactivate();

				Player.velocity += Dir * -14;

				if (Dir.X != 0 && Dir.Y != 0)
					Player.velocity.X *= -1;

				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(Player.Center + Dir * 32, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Dir.RotatedByRandom(0.4f) * -Main.rand.NextFloat(26), 0, new Color(50, 200, 255, 0), 0.1f);
				}
			}
		}

		public override void DrawActiveEffects(SpriteBatch spriteBatch)
		{
			if (!Main.gameMenu && EffectTimer < 64 && EffectTimer > 0)
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

				flowTimer += EffectTimer < 20f ? EffectTimer / 20f * 0.025f : 0.025f;
			}

			if (Time < 8 && EffectTimer > 0)
			{
				Vector2 prevPos = Player.Center + Vector2.Normalize(Player.velocity) * 10;
				int direction = EffectTimer % 2 == 0 ? -1 : 1;

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
		}

		public override void CooldownFinish()
		{
			for (int k = 0; k <= 20; k++)
			{
				var dus = Dust.NewDustPerfect(Player.Center + Vector2.One.RotatedBy(k / 20f * 6.28f) * Main.rand.NextFloat(50), DustType<AirLegacyWindsAnimation>(), Vector2.Zero);
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
				cache = [];

				for (int i = 0; i < 14; i++)
				{
					cache.Add(Player.Center + lastRealVel * 3);
				}
			}

			cache.Add(Player.Center + lastRealVel * 3);

			while (cache.Count > 14)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 14, new NoTip(), 
					factor => {
						float trueFactor = StarlightMathHelper.GetEarlyTrailFactor(factor, 14, maxTrail);

						return trueFactor <= 0.8f ? Math.Min(trueFactor * 50, 30) : 30 - (trueFactor - 0.8f) / 0.2f * 10;
					}, 
					
					factor => {
						float trueFactor = StarlightMathHelper.GetEarlyTrailFactor(factor.X, 14, maxTrail);

						if (factor.X == 1)
							return Color.Transparent;

						float alpha = trueFactor < 0.8 ? trueFactor / 0.8f : 1f - (trueFactor - 0.8f) / 0.2f;

						return new Color(100, 120 + (int)(105 * trueFactor), 255 - (int)(50 * trueFactor)) * 0.75f * alpha * (float)Math.Sin(EffectTimer / 65f * 3.14f);
					});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Player.Center + Player.velocity * 6;
		}

		public virtual void DrawPrimitives()
		{
			//Main.spriteBatch.End();
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("ScrollingTrail").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(flowTimer);
					effect.Parameters["repeats"].SetValue(1f);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.FireTrail.Value);

					trail?.Render(effect);
				}
			});

			//Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}
}