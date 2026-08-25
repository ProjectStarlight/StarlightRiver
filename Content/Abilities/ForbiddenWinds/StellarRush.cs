using StarlightRiver.Content.Abilities.Infusions;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Vitric.Temple;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.PixelationSystem;
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

		public new void Load()
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
			goalVel = Dir * Speed; // "conservation of momentum"
			Player.velocity = goalVel;

			//Player.frozen = true;
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
			for (float k = 0; k <= 1; k += 0.2f)
			{
				Vector2 swirlOff = Vector2.UnitX.RotatedBy(Player.velocity.ToRotation() + 1.57f) * (float)Math.Sin((Time + k) / 25f * 6.28f * 1.5f) * 14;
				Vector2 pos = Player.Center + (Player.Center - nextPos) * k + swirlOff;
				Vector2 vel = Player.velocity * Main.rand.NextFloat(0.1f, 0.2f) + swirlOff * Main.rand.NextFloat(0.1f, 0.14f);

				int type = k == 0 ? DustType<Dusts.AuroraDecelerating>() : DustType<Dusts.PixelatedEmber>();

				if (type == DustType<Dusts.AuroraDecelerating>())
					vel *= 4;

				var d = Dust.NewDustPerfect(pos, type, vel, 0, new Color(40 + (int)(Time / 25f * 60), 160 - (int)(Time / 25f * 100), 255, 0), Main.rand.NextFloat(0.1f, 0.2f));

				if (type == DustType<Dusts.AuroraDecelerating>())
					d.customData = Main.rand.NextFloat(0.8f, 1.5f);
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
				trail = new Trail(Main.instance.GraphicsDevice, 24, new NoTip(), factor => (float)Math.Sin(factor * 3.14f) * 30, factor =>
							{
								if (factor.X == 1)
									return Color.Transparent;

								return new Color(100 - (int)(factor.X * 50), 100 + (int)(factor.X * 50), 255) * (float)Math.Sin(factor.X * 3.14f) * (float)Math.Sin(EffectTimer / 45f * 3.14f) * 0.25f;
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Player.Center + Player.velocity * 6;
		}

		public override void DrawPrimitives()
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

					effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.02f);
					effect.Parameters["repeats"].SetValue(1f);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.FireTrail.Value);

					trail?.Render(effect);
				}
			});

			//Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
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
			Tooltip.SetDefault("[i:StarlightRiver/WindsHover][c/99FFCC:Forbidden Winds] Infusion\nIncreases cost to[i:StarlightRiver/StarlightHover][c/AAF0FF:1.5] {{Starlight}}\nYour dash will travel farther and launch you more afterwards");
		}

		public override void SetDefaults()
		{
			SetStaticDefaults();
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