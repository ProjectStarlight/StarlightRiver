using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Content.Abilities.Infusions;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Tiles.Vitric.Temple;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.ForbiddenWinds
{
	internal class GatheringWinds : Dash, IOrderedLoadable
	{
		public int charge;
		public float rot;

		public bool charging;

		public override float ActivationCostDefault => 0f;

		new public void Load()
		{
			StarlightPlayer.PostUpdateEvent += UpdatePlayerFrame;
		}

		public override void OnActivate()
		{
			SetVelocity();

			charging = true;
			charge = 0;
			rot = 0;

			Time = 35;
		}

		public override void UpdateActive()
		{
			bool control = StarlightRiver.Instance.AbilityKeys.Get<Dash>().Current;

			if (charging && control && Player.GetHandler().Stamina > 0.1f)
			{
				charging = true;

				Time = 20 + (int)(charge / 9f);
				maxTime = 20 + (int)(charge / 9f);
				EffectTimer = Time + 10;
				charge++;
				rot += 0.2f + charge / 500f;
				Player.GetHandler().Stamina -= 0.032f;

				Player.frozen = true;
				Player.gravity = 0;
				Player.velocity.Y *= 0.9f;
			}
			else if (charge > 30)
			{
				charging = false;

				Player.velocity = Dash.SignedLesserBound(Dir * (12 + charge / 6f), Player.velocity); // "conservation of momentum"

				Player.frozen = true;
				Player.gravity = 0;
				Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, 12 + charge / 6f);

				// NPC Collision to break defense
				Rectangle box = Player.Hitbox;
				box.Inflate(32, 32);
				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (box.Intersects(npc.Hitbox) && !npc.dontTakeDamage && !npc.immortal)
					{
						Player.immune = true;
						Player.immuneTime = 10;
						Player.velocity *= -0.5f;

						if (npc.defense > 0)
						{
							npc.defense = 0;
							Helpers.Helper.PlayPitched("World/HeavyClose", 1f, 0.5f, Player.Center);
							SoundEngine.PlaySound(npc.HitSound, Player.Center);

							CombatText.NewText(npc.Hitbox, Color.White, "Defense shattered!");
						}
						else
						{
							SoundEngine.PlaySound(npc.HitSound, Player.Center);
						}

						Speed = -0.25f;
						Boost = 1f;
						SetVelocity();
						Deactivate();
					}
				}

				if (Main.netMode != NetmodeID.Server)
				{
					ManageCaches();
					ManageTrail();
				}
			}
			else if (charging)
			{
				charging = false;
				Time = 1;
			}

			if (Time-- <= 0)
				Deactivate();
		}

		new public void UpdatePlayerFrame(Player Player)
		{
			if (Player.GetHandler().ActiveAbility is GatheringWinds)
			{
				bool control = StarlightRiver.Instance.AbilityKeys.Get<Dash>().Current;
				var dash = Player.GetHandler().ActiveAbility as GatheringWinds;

				if (dash.charging)
				{
					Player.bodyFrame = new Rectangle(0, 56 * 3, 40, 56);
					Player.UpdateRotation(-dash.rot);
				}
				else if (dash.Active)
				{
					Player.UpdateRotation(0);
				}
			}
		}

		public override void Reset()
		{
			Boost = 0.5f;
			Time = maxTime = 35;
			CooldownBonus = 0;
		}

		public override void UpdateActiveEffects()
		{
			if (charging)
			{
				if (Main.GameUpdateCount % 10 == 0)
				{
					Projectile.NewProjectile(null, Player.Center, Vector2.Zero, ModContent.ProjectileType<GatheringWindsWhirl>(), 0, 0, Player.whoAmI, 20 - charge / 2f, 1f + charge / 150f, 3 + charge / 20f);

					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45.WithPitchOffset(-1f + charge / 300f), Player.Center);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item104.WithPitchOffset(-1f + charge / 300f), Player.Center);
				}
			}
			else if (charge > 30)
			{
				if (Time % 2 == 0)
				{
					Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(48, 48) + Player.velocity * 2, ModContent.DustType<Dusts.GlowLine>(), Player.velocity * -Main.rand.NextFloat(0.2f, 0.5f), 0, new Color(160, 150, 170), 0.6f);
				}
			}
		}

		public override void SafeUpdateFixed()
		{
			if (EffectTimer > 0)
				EffectTimer--;
		}

		public override void DrawActiveEffects(SpriteBatch spriteBatch)
		{
			if (!Main.gameMenu && !charging && EffectTimer > 0)
				DrawPrimitives();
		}

		private void ManageCaches()
		{
			if (charging)
				cache?.Clear();

			if (cache == null || cache.Count < maxTime)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < maxTime; i++)
				{
					cache.Add(Player.Center + Player.velocity * 3);
				}
			}

			cache.Add(Player.Center + Player.velocity * 3);

			while (cache.Count > maxTime)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed || maxTime != trail.Positions.Count())
			{
				trail = new Trail(Main.instance.GraphicsDevice, maxTime, new NoTip(), factor => Math.Min(factor * 50, 40), factor =>
				{
					if (factor.X <= EffectTimer / (float)(maxTime + 10) || factor.X == 1)
						return Color.Transparent;

					return new Color(110 + (int)(50 * factor.X), 105 + (int)(50 * factor.X), 200) * factor.X * (float)Math.Sin(EffectTimer / (float)(maxTime + 10) * 3.14f) * 0.35f;
				});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Player.Center + Player.velocity * 6;
		}

		public override void DrawPrimitives()
		{
			if (charge < 30)
				return;

			Main.spriteBatch.End();

			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.01f);
			effect.Parameters["repeats"].SetValue(maxTime / 15f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.FireTrail.Value);

			trail?.Render(effect);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.005f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.ShadowTrail.Value);

			trail?.Render(effect);

			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	internal class GatheringWindsWhirl : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		private float offset;
		public ref float Vertical => ref Projectile.ai[0];
		private ref float Radius => ref Projectile.ai[1];
		private ref float Speed => ref Projectile.ai[2];

		public float Progress;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.extraUpdates = 1;

			offset = Main.rand.NextFloat(6.28f);
		}

		public override void AI()
		{
			Projectile.Center = Main.player[Projectile.owner].Center;

			Progress += 0.02f;

			bool control = StarlightRiver.Instance.AbilityKeys.Get<Dash>().Current;

			if (control && Main.player[Projectile.owner].GetHandler().Stamina > 0.1f && Projectile.timeLeft < 50)
				Projectile.timeLeft = 20;
			else if (Projectile.timeLeft > 10)
				Projectile.timeLeft = 10;

			if (!Main.dedServ)
			{
				ManageCache(ref cache);
				ManageTrail(ref trail, cache);
			}
		}

		private void ManageCache(ref List<Vector2> cache)
		{
			if (cache == null || cache.Count < 120)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 120; i++)
				{
					cache.Add(Projectile.Center + new Vector2(0, 100));
				}
			}

			float rad = 1;

			if (Projectile.timeLeft < 10)
				rad = Projectile.timeLeft / 10f;

			if (Projectile.timeLeft > 50)
				rad = 1 - (Projectile.timeLeft - 50) / 10f;

			rad *= Radius;

			for (int i = 0; i < 120; i++)
			{
				cache[i] = Projectile.Center + new Vector2((float)Math.Cos(Progress * Speed + i / 60f + offset) * 48 * rad, (float)Math.Sin(Progress * Speed + i / 60f + offset) * 16 * rad + Vertical);
			}

			while (cache.Count > 120)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail(ref Trail trail, List<Vector2> cache)
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 120, new NoTip(), factor => 5, factor =>
			{
				if (factor.X == 1)
					return Color.Transparent;

				return new Color(110 - (int)Vertical / 2, 105 - (int)Vertical, 170) * (float)Math.Sin(factor.X * 3.14f) * 0.15f * Radius;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			if (effect is null)
				return;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.01f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrailNoEnd.Value);

			trail?.Render(effect);
		}
	}

	class GatheringWindsItem : InfusionItem<Dash, GatheringWinds>
	{
		public override InfusionTier Tier => InfusionTier.Bronze;
		public override string Texture => "StarlightRiver/Assets/Abilities/Blink";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gathering Winds");
			Tooltip.SetDefault("Forbidden Winds Infusion\nHold the dash key to hover and charge up a dash\nCharged dashes shatter enemy defense on contact\nDrain 2 starlight/s while charging\nMust spend atleast 1 starlight to activate");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 14;
			Item.rare = ItemRarityID.Green;

			color = new Color(200, 220, 250);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<BasicInfusion>(1);
			recipe.AddIngredient(ItemID.Cloud, 15);
			recipe.AddIngredient(ItemID.AnkletoftheWind, 1);
			recipe.AddTile(ModContent.TileType<MainForge>());
			recipe.Register();
		}
	}
}