using StarlightRiver.Content.Abilities.Infusions;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Vitric.Temple;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	class StellarRush : Dash, IOrderedLoadable
	{
		public override float ActivationCostDefault => 1.5f;

		public void Load()
		{
			StarlightPlayer.PostUpdateEvent += UpdatePlayerFrame;
		}

		public override void OnActivate()
		{
			base.OnActivate();
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item96, Player.Center);

			Time = 25;
		}

		public override void UpdateActive()
		{
			Player.velocity = Dash.SignedLesserBound(Dir * Speed, Player.velocity); // "conservation of momentum"

			Player.frozen = true;
			Player.gravity = 0;
			Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, Speed);

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			if (Time-- <= 0)
				Deactivate();
		}

		public override void Reset()
		{
			Boost = 0.5f;
			Speed = 22;
			Time = maxTime = 25;
			CooldownBonus = 0;
		}

		public override void UpdateActiveEffects()
		{
			if (Time == 25)
				return;

			Vector2 nextPos = Player.Center + Vector2.Normalize(Player.velocity) * Speed;
			for (float k = 0; k <= 1; k += 0.08f)
			{
				Vector2 swirlOff = Vector2.UnitX.RotatedBy(Player.velocity.ToRotation() + 1.57f) * (float)Math.Sin((Time + k) / 25f * 6.28f * 1.5f) * 14;
				Vector2 pos = Player.Center + (Player.Center - nextPos) * k + swirlOff;
				Vector2 vel = Player.velocity * Main.rand.NextFloat(0.1f, 0.2f) + swirlOff * Main.rand.NextFloat(0.1f, 0.14f);

				int type = k == 0 ? DustType<Dusts.AuroraDecelerating>() : DustType<Dusts.Cinder>();

				if (type == DustType<Dusts.AuroraDecelerating>())
					vel *= 4;

				var d = Dust.NewDustPerfect(pos, type, vel, 0, new Color(40, 230 - (int)(Time / 25f * 160), 255), Main.rand.NextFloat(0.2f, 0.4f));
				d.customData = Main.rand.NextFloat(0.4f, 0.9f);
			}
		}

		public override void SafeUpdateFixed()
		{
			if (cache is null)
				return;

			if (EffectTimer > 0)
			{
				EffectTimer--;
			}

			if (EffectTimer < 44 - 24)
			{
				for (int i = 0; i < 24; i++)
				{
					Vector2 swirlOff2 = Vector2.UnitX.RotatedBy((cache[0] - cache[23]).ToRotation() + 1.57f) * (float)Math.Sin((i - 3) / 25f * 6.28f * 1.5f) * 30;
					cache[i] += swirlOff2 * 0.05f;
					cache[i] += Vector2.Normalize(cache[23] - cache[0]) * 1f;
				}
			}
		}

		new public void UpdatePlayerFrame(Player Player)
		{
			if (Player.GetHandler().ActiveAbility is StellarRush)
			{
				var dash = Player.GetHandler().ActiveAbility as Dash;

				Player.bodyFrame = new Rectangle(0, 56 * 3, 40, 56);
				Player.UpdateRotation(dash.Time / (float)dash.maxTime * 6.28f);

				if (dash.Time == dash.maxTime || Player.dead)
					Player.UpdateRotation(0);
			}
		}

		private void ManageCaches()
		{
			if (Time == 25)
				cache?.Clear();

			if (cache == null || cache.Count < 24)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 24; i++)
				{
					cache.Add(Player.Center + Player.velocity * 3);
				}
			}

			Vector2 swirlOff = Vector2.UnitX.RotatedBy(Player.velocity.ToRotation() + 1.57f) * (float)Math.Sin((Time - 3) / 25f * 6.28f * 1.5f) * 30;
			cache.Add(Player.Center + Player.velocity * 3 + swirlOff);

			while (cache.Count > 24)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 24, new NoTip(), factor => (float)Math.Sin(factor * 3.14f) * 60, factor =>
							{
								if (factor.X == 1)
									return Color.Transparent;

								return new Color(50, 100 + (int)(factor.X * 150), 255) * (float)Math.Sin(factor.X * 3.14f) * (float)Math.Sin(EffectTimer / 45f * 3.14f) * 0.15f;
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Player.Center + Player.velocity * 6;
		}

		public override void DrawPrimitives()
		{
			Main.spriteBatch.End();

			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.0025f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

			trail?.Render(effect);

			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	class StellarRushItem : InfusionItem<Dash, StellarRush>
	{
		public override InfusionTier Tier => InfusionTier.Bronze;
		public override string Texture => "StarlightRiver/Assets/Abilities/Astral";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Stellar Rush");
			Tooltip.SetDefault("Forbidden Winds Infusion\nDash farther and carry more speed\nIncreases starlight cost to 1.5");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 14;
			Item.rare = ItemRarityID.Green;

			color = new Color(100, 200, 250);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<BasicInfusion>(1);
			recipe.AddIngredient<StaminaGel>(25);
			recipe.AddTile(ModContent.TileType<MainForge>());
			recipe.Register();
		}
	}
}