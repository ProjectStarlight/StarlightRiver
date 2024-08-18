using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Infernal
{
	internal class InfernalHarvest : ModItem
	{
		public int combo = 1;

		public override string Texture => "StarlightRiver/Assets/Items/Infernal/InfernalHarvest";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Infernal Harvest");
			Tooltip.SetDefault("Swing a huge harvesting scythe to gather mana from enemies\n<right> to spend 90 mana to throw the scythe, leaving a burning trail");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Magic;
			Item.damage = 34;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.useTime = 55;
			Item.useAnimation = 55;
			Item.shoot = ModContent.ProjectileType<InfernalHarvestFree>();
			Item.shootSpeed = 1f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 1);
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse != 2)
			{
				Projectile.NewProjectile(Item.GetSource_FromThis(), position, velocity, type, damage, knockback, player.whoAmI, 0, combo);
				combo *= -1;

				Helper.PlayPitched("Effects/HeavyWhoosh", 0.8f, -0.1f + Main.rand.NextFloat(-0.1f, 0.1f), player.Center);
				Helper.PlayPitched("Effects/FancySwoosh", 0.8f, 0f, player.Center);
			}
			else if (player.CheckMana(90, true))
			{
				player.manaRegenDelay = 35;
				Projectile.NewProjectile(Item.GetSource_FromThis(), position, velocity * 24, ModContent.ProjectileType<InfernalHarvestPaid>(), damage * 3, knockback, player.whoAmI, 0, velocity.X > 0 ? -1 : 1);

				Helper.PlayPitched("Effects/FancySwoosh", 0.8f, 0.2f, player.Center);
				Helper.PlayPitched("Magic/FireHit", 0.6f, -0.1f, player.Center);
			}

			return false;
		}
	}

	internal class InfernalHarvestFree : ModProjectile, IDrawPrimitive
	{
		public const int SCYTHE_LENGTH = 362;

		public float startRot;
		public int direction;

		private List<Vector2> cache;
		private Trail trail;

		private List<Vector2> cacheBig;
		private Trail trailBig;

		public Player Owner => Main.player[Projectile.owner];

		public ref float Timer => ref Projectile.ai[0];
		public ref float InvertDirection => ref Projectile.ai[1];

		public override string Texture => "StarlightRiver/Assets/Items/Infernal/InfernalHarvestProj";

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.timeLeft = 110;
			Projectile.width = SCYTHE_LENGTH * 2;
			Projectile.height = SCYTHE_LENGTH * 2;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.DamageType = DamageClass.Magic;
		}

		public override void AI()
		{
			Timer++;

			Projectile.Center = Owner.MountedCenter;

			Owner.heldProj = Projectile.whoAmI;
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, Projectile.rotation - 1.57f);
			Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.97f);

			if (Timer == 1)
			{
				direction = Projectile.velocity.X > 0 ? -1 : 1;
				direction *= (int)InvertDirection;

				startRot = Projectile.velocity.ToRotation() + 2f * direction;
				Projectile.rotation = startRot;
			}

			if (Timer > 16)
				Projectile.rotation = startRot - Ease((Timer - 16) / 94f) * 4 * direction;

			if (Timer > 16 && Timer < 50)
			{
				Vector2 pos = Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH * 0.7f + Main.rand.NextVector2Circular(16, 16);
				var d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFollowPlayer>(), Main.rand.NextVector2Circular(2, 2), 0, new Color(255, 150, 0) * 0.5f, Main.rand.NextFloat(0.3f, 0.6f));
				d.customData = new object[] { Owner, pos };
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public float Ease(float x)
		{
			return x == 1 ? 1 : 1f - (float)Math.Pow(2, -10 * x);
		}

		public override bool? CanHitNPC(NPC target)
		{
			Vector2 perp = Vector2.UnitX.RotatedBy(Projectile.rotation + 1.57f);

			bool hit = target.immune[0] <= 0 && Timer < 60 && (
				Helper.CheckLinearCollision(Projectile.Center, Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH, target.Hitbox, out _) ||
				Helper.CheckLinearCollision(Projectile.Center + perp * 10, Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH + perp * 10, target.Hitbox, out _) ||
				Helper.CheckLinearCollision(Projectile.Center + perp * -10, Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH + perp * -10, target.Hitbox, out _));

			return hit ? null : false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			int amount = Math.Min(20, Owner.statManaMax2 - Owner.statMana);

			if (amount > 0)
			{
				Owner.statMana += amount;
				Owner.ManaEffect(amount);
			}

			Helper.PlayPitched("Impacts/StabFleshy", 0.9f, 0f, target.Center);
			Helper.PlayPitched("Impacts/GoreLight", 0.25f, 0.5f, target.Center);

			Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowLineFast>(), Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f) * 15 * direction, 0, new Color(255, 80, 0), 2.5f);

			Owner.GetModPlayer<StarlightPlayer>().SetHitPacketStatus(shouldRunProjMethods: true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "Glow").Value;
			var origin = new Vector2(tex.Width - 16, tex.Height - 16);

			float opacity;

			if (Timer < 16)
				opacity = Timer / 16f;
			else if (Timer > 90)
				opacity = 1 - (Timer - 90) / 20f;
			else
				opacity = 1;

			float glow = 1;

			if (Timer > 50)
				glow *= Math.Max(0f, 1f - (Timer - 50) / 10f);

			if (direction == 1)
			{
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * opacity, Projectile.rotation + 1.57f + 1.57f * 0.5f, origin, 1f * opacity, 0, 0);
				Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, Color.White * glow, Projectile.rotation + 1.57f + 1.57f * 0.5f, origin, 1f * opacity, 0, 0);
			}
			else
			{
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * opacity, Projectile.rotation + 1.57f * 0.5f, new Vector2(16, tex.Height - 16), 1f * opacity, SpriteEffects.FlipHorizontally, 0);
				Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, Color.White * glow, Projectile.rotation + 1.57f * 0.5f, new Vector2(16, tex.Height - 16), 1f * opacity, SpriteEffects.FlipHorizontally, 0);
			}

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null || cacheBig == null)
			{
				cache = new List<Vector2>();
				cacheBig = new List<Vector2>();

				for (int i = 0; i < 30; i++)
				{
					cache.Add(Vector2.Lerp(Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH, Vector2.Zero, 0.3f));
					cacheBig.Add(Vector2.Lerp(Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH, Vector2.Zero, 0.5f));
				}
			}

			cache.Add(Vector2.Lerp(Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH, Vector2.Zero, 0.3f));
			cacheBig.Add(Vector2.Lerp(Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH, Vector2.Zero, 0.5f));

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}

			while (cacheBig.Count > 30)
			{
				cacheBig.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 160f, factor =>
			{
				if (factor.X >= 0.8f)
					return Color.White * 0;

				float opacity = 0.5f;

				if (Timer > 50)
					opacity *= Math.Max(0f, 1f - (Timer - 50) / 10f);

				return new Color(255, (int)(255 * factor.X), 0) * opacity * factor.X;
			});

			Vector2[] ar = cache.ToArray();
			for (int k = 0; k < ar.Length; k++)
			{
				ar[k] = ar[k] + Projectile.Center;
			}

			trail.Positions = ar;
			trail.NextPosition = Vector2.Lerp(Projectile.Center, Owner.Center, 0.15f) + Projectile.velocity;

			trailBig ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 160f, factor =>
			{
				if (factor.X >= 0.8f)
					return Color.White * 0;

				float opacity = 0.1f;

				if (Timer > 50)
					opacity *= Math.Max(0f, 1f - (Timer - 50) / 10f);

				return new Color(255, 80, (int)(175 * factor.X)) * opacity * factor.X;
			});

			Vector2[] ar2 = cacheBig.ToArray();
			for (int k = 0; k < ar2.Length; k++)
			{
				ar2[k] = ar2[k] + Projectile.Center;
			}

			trailBig.Positions = ar2;
			trailBig.NextPosition = Vector2.Lerp(Projectile.Center, Owner.Center, 0.15f) + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);
			trail?.Render(effect);

			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value);
			trailBig?.Render(effect);
		}
	}

	internal class InfernalHarvestPaid : ModProjectile, IDrawPrimitive
	{
		public const int SCYTHE_LENGTH = 240;

		private List<Vector2> cache;
		private Trail trail;

		public float traveled;
		public bool slowedThisFrame;

		public Player Owner => Main.player[Projectile.owner];

		public ref float Timer => ref Projectile.ai[0];
		public ref float Direction => ref Projectile.ai[1];

		public override string Texture => "StarlightRiver/Assets/Items/Infernal/InfernalHarvestPaid";

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.timeLeft = 400;
			Projectile.width = SCYTHE_LENGTH;
			Projectile.height = SCYTHE_LENGTH;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 3;
			Projectile.DamageType = DamageClass.Magic;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return Projectile.velocity.Length() > 4 ? null : false;
		}

		public override void AI()
		{
			Timer++;
			traveled += Projectile.velocity.Length();

			slowedThisFrame = false;

			Projectile.rotation -= Projectile.velocity.Length() * 0.01f * Direction;
			Projectile.velocity *= 0.982f;

			for (int x = 0; x < 15; x++)
			{
				for (int y = 0; y < 15; y++)
				{
					Tile tile = Framing.GetTileSafely((Projectile.position / 16).ToPoint16() + new Point16(x, y));

					if (tile.HasTile && Main.tileSolid[tile.TileType])
						slowedThisFrame = true;
				}
			}

			if (slowedThisFrame)
			{
				Projectile.velocity *= 0.985f;
				Projectile.timeLeft--;
				Timer++;
				slowedThisFrame = false;
			}

			if (traveled >= 130)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BurnTrail>(), 5, 0, Projectile.owner, Main.rand.Next(3000));
				traveled = 0;
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(90, 90), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * -1, 0, new Color(255, Main.rand.Next(100, 200), 20) * (Projectile.velocity.Length() / 24f), Main.rand.NextFloat(0.3f, 0.7f));
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (!slowedThisFrame)
			{
				Projectile.velocity = oldVelocity * 0.995f;
				slowedThisFrame = true;
			}
			else
			{
				Projectile.velocity = oldVelocity;
			}

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.damage -= 7;

			Helper.PlayPitched("Impacts/StabFleshy", 0.9f, -0.2f, target.Center);
			Helper.PlayPitched("Impacts/GoreLight", 0.25f, 0.2f, target.Center);

			Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowLineFast>(), Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f) * 15, 0, new Color(255, 80, 0), 2.5f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 origin = new Vector2(tex.Width, tex.Height) / 2f;

			float opacity;

			if (Timer < 6)
				opacity = Timer / 6f;
			else if (Timer > 300)
				opacity = 1 - (Timer - 300) / 60f;
			else
				opacity = 1;

			SpriteEffects effects = Direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			float rotation = Direction == 1 ? Projectile.rotation + 1.57f + 1.57f * 0.5f : Projectile.rotation + 1.57f * 0.5f;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * opacity, rotation, origin, 1f, effects, 0);

			opacity *= Projectile.velocity.Length() / 24f;

			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
			Color color = new Color(160, 50, 80) * opacity;
			color.A = 0;
			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, 7f * opacity, 0, 0);

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 70; i++)
				{
					cache.Add(Vector2.Lerp(Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH, Vector2.Zero, 0.5f));
				}
			}

			cache.Add(Vector2.Lerp(Vector2.UnitX.RotatedBy(Projectile.rotation) * SCYTHE_LENGTH, Vector2.Zero, 0.5f));

			while (cache.Count > 70)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 70, new TriangularTip(40 * 4), factor => factor * 100f, factor =>
			{
				if (factor.X >= 0.8f)
					return Color.White * 0;

				float opacity = Projectile.velocity.Length() / 24f;

				return new Color(255, (int)(255 * factor.X), 0) * opacity * factor.X;
			});

			Vector2[] ar = cache.ToArray();
			for (int k = 0; k < ar.Length; k++)
			{
				ar[k] = ar[k] + Projectile.Center;
			}

			trail.Positions = ar;
			trail.NextPosition = Vector2.Lerp(Projectile.Center, Owner.Center, 0.15f) + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);
			trail?.Render(effect);
		}
	}

	internal class BurnTrail : ModProjectile
	{
		public ref float Timer => ref Projectile.ai[0];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.timeLeft = 200;
			Projectile.width = 140;
			Projectile.height = 140;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}

		public override void AI()
		{
			Timer++;

			float opacity = Projectile.timeLeft / 200f;

			Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.5f, 0f) * Projectile.timeLeft / 200f);

			if (Main.rand.NextBool(3))
				Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * 70 + Main.rand.NextVector2Circular(90, 90), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * -1, 0, new Color(255, Main.rand.Next(100, 200), 20) * opacity, Main.rand.NextFloat(0.2f, 0.8f));

			if (Main.rand.NextBool(5))
				Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * 70 + Main.rand.NextVector2Circular(110, 110), ModContent.DustType<Dusts.BigFire>(), Vector2.UnitY * Main.rand.NextFloat(-1, -3), 0, Color.White, 1f);
		}

		public override bool? CanHitNPC(NPC target)
		{
			return null;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire3, 120);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;

			Effect effect = Filters.Scene["ColoredFire"].GetShader().Shader;

			if (effect is null)
				return false;

			float opacity = 4f * Projectile.timeLeft / 200f;

			effect.Parameters["u_time"].SetValue(Timer * 0.015f % 2f);
			effect.Parameters["primary"].SetValue(new Vector3(1, 0.7f, 0.1f) * opacity);
			effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1, 1));
			effect.Parameters["secondary"].SetValue(new Vector3(1f, 0.2f, 0.05f) * opacity);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);
			effect.Parameters["mapTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise3").Value);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * opacity, 0, tex.Size() / 2f, 2.5f, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}