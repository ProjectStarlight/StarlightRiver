using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Vitric
{
	public class IgnitionGauntlets : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public int handCounter = 0;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
			Tooltip.SetDefault("I will update this later");
		}

		public override void SetDefaults()
		{
			Item.damage = 32;
			Item.DamageType = DamageClass.Melee;
			Item.width = 5;
			Item.height = 5;
			Item.useTime = 3;
			Item.useAnimation = 3;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.shoot = ModContent.ProjectileType<IgnitionPunchPhantom>();
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
		}

		public override void HoldItem(Player player)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<IgnitionGauntletChargeUI>()] == 0)
				Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletChargeUI>(), 0, 0, player.whoAmI);
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				if (player.GetModPlayer<IgnitionPlayer>().charge > 0 && player.ownedProjectileCounts[ModContent.ProjectileType<IgnitionGauntletCharge>()] == 0)
				{
					Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletCharge>(), damage, knockback, player.whoAmI);
				}
				return false;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<IgnitionGauntletCharge>()] == 0)
			{
				IgnitionPlayer modPlayer = player.GetModPlayer<IgnitionPlayer>();
				if (modPlayer.loadedCharge > 20)
				{
					float damagelerper = (modPlayer.loadedCharge - 15) / 135f;
					damagelerper = MathHelper.Max(damagelerper, 0.5f);
					Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletCone>(), (int)(damage * 4 * damagelerper), knockback, player.whoAmI, (float)Math.Sqrt(damagelerper));

					damagelerper = (float)Math.Sqrt(damagelerper);

					modPlayer.loadedCharge = 20;
					modPlayer.flipping = true;

					for (int i = 0; i < 30; i++)
					{
						var pos = position;
						float lerper = Main.rand.NextFloat();
						Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<IgnitionGauntletSmoke3>(), Vector2.Normalize(player.DirectionTo(Main.MouseWorld)).RotatedByRandom(0.8f) * 50.5f * lerper * damagelerper);
						dust.scale = Main.rand.NextFloat(0.75f, 1.25f) * MathHelper.Lerp(0.4f, 1f, lerper) * damagelerper;
						dust.alpha = Main.rand.Next(50);
						dust.rotation = Main.rand.NextFloatDirection();
					}
		
					for (int k = 0; k < 12; k++)
					{
						Dust.NewDustPerfect(position + new Vector2(0, 35), ModContent.DustType<IgnitionGauntletSpark>(), Vector2.Normalize(player.DirectionTo(Main.MouseWorld)).RotatedByRandom(1.2f) * Main.rand.Next(3, 30) * damagelerper, 0, Color.Yellow, 2.4f * damagelerper); ;
					}
					for (int k = 0; k < 12; k++)
					{
						Dust.NewDustPerfect(position + new Vector2(0, 35), ModContent.DustType<IgnitionGauntletSpark>(), Vector2.Normalize(player.DirectionTo(Main.MouseWorld)).RotatedByRandom(0.5f) * Main.rand.Next(3, 15) * damagelerper, 0, Color.Yellow, 1.3f * damagelerper);
					}

					Vector2 handOffset = new Vector2(-10 * player.direction, -16);
					handOffset = handOffset.RotatedBy(player.fullRotation);
					Dust star = Dust.NewDustPerfect(handOffset + player.Center, ModContent.DustType<IgnitionGauntletStar>(), player.DirectionTo(Main.MouseWorld) * 5);
					star.scale = 1.5f;
					star.rotation = Main.GameUpdateCount * 0.085f;

					Projectile proj = Projectile.NewProjectileDirect(source, position, player.DirectionTo(Main.MouseWorld) * 10 * damagelerper, ModContent.ProjectileType<IgnitionGauntletsImpactRing>(), 0, 0, player.whoAmI, Main.rand.Next(150, 250) * damagelerper, player.DirectionTo(Main.MouseWorld).ToRotation());
					var mp = proj.ModProjectile as IgnitionGauntletsImpactRing;
					mp.timeLeftStart = 50;
					proj.timeLeft = 50;
					proj.extraUpdates = 3;

					player.velocity *= -0.5f;
					player.itemTime = player.itemAnimation = 20;
					player.direction *= Math.Sign(player.Center.Y - Main.MouseWorld.Y);
					player.GetModPlayer<StarlightPlayer>().Shake += (int)(12 * damagelerper);
					return false;
				}
			}
			/*if (player.velocity.Length() < 6 && !(player.controlUp || player.controlDown || player.controlLeft || player.controlRight))
            {
				Vector2 dir = player.DirectionTo(Main.MouseWorld) * 0.6f;
            }*/
			handCounter++;
			if (handCounter % 2 == 0)
			{
				Projectile proj = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, (handCounter % 4) / 2);
				var mp = proj.ModProjectile as IgnitionPunchPhantom;
				mp.directionVector = player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.2f);

				int offsetFactor = Main.rand.Next(10, 20);
				int offset = Main.rand.Next(-20, 20);
				Vector2 vel = new Vector2(7, offset * 0.4f);
				Vector2 offsetV = new Vector2(Main.rand.Next(-offsetFactor, offsetFactor), offset);
				float rot = position.DirectionTo(Main.MouseWorld).ToRotation();
				Projectile.NewProjectileDirect(source, position + offsetV.RotatedBy(rot), vel.RotatedBy(rot), ModContent.ProjectileType<IgnitionPunch>(), damage, knockback, player.whoAmI, (handCounter % 4) / 2);
			}
			return false;
		}
	}

	public class IgnitionGauntletChargeUI : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.VitricItem + Name;
		private Player owner => Main.player[Projectile.owner];

		public int Radius => 25;

		private List<Vector2> cache;
		private List<Vector2> cache2;

		private Trail trail;
		private Trail trail2;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void AI()
		{

			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = owner.Center;
			Lighting.AddLight(owner.Center, Color.OrangeRed.ToVector3() * modPlayer.charge / 150f * Main.rand.NextFloat());
			if (owner.HeldItem.type == ModContent.ItemType<IgnitionGauntlets>())
				Projectile.timeLeft = 2;
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Main.spriteBatch.End();
			Effect effect = Filters.Scene["IgnitionFlames"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);


			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Back").Value);
			effect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>(Texture + "_Noise2").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>(Texture + "_Noise").Value);
			effect.Parameters["stretch"].SetValue(0.002f * modPlayer.charge);
			effect.Parameters["stretch2"].SetValue(0.0011f * modPlayer.charge);
			effect.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.035f);
			effect.Parameters["opacity"].SetValue((float)Math.Sqrt(modPlayer.charge / 150f));
			effect.Parameters["pallette"].SetValue(ModContent.Request<Texture2D>(Texture + "_Pallette").Value);


			trail?.Render(effect);

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			cache2 = new List<Vector2>();
			float radius = Radius;
			for (int i = 0; i < 17; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = (i / 32f) * 6.28f;
				Vector2 offset = new Vector2((float)Math.Cos(rad), 0);
				offset *= radius;
				cache.Add(Projectile.Center + offset);
			}
			for (int i = 17; i > 0; i--) //TODO: Cache offsets, to improve performance
			{
				double rad = (i / 32f) * 6.28f;
				Vector2 offset = new Vector2((float)Math.Cos(rad), 0);
				offset *= radius;
				cache2.Add(Projectile.Center + offset);
			}

			while (cache.Count > 17)
				cache.RemoveAt(0);
			while (cache2.Count > 17)
				cache2.RemoveAt(0);
		}

		private void ManageTrail()
		{

			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 17, new TriangularTip(1), factor => 55, factor =>
			{
				return Color.White;
			});

			float nextplace = 17f / 32f;
			Vector2 offset = new Vector2((float)Math.Cos(nextplace), 0);
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + offset;

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 17, new TriangularTip(1), factor => 55, factor =>
			{
				return Color.White;
			});
			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}

		public void DrawPrimitives()
		{
			return;
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Effect effect = Filters.Scene["IgnitionFlames"].GetShader().Shader;


			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture).Value);
			effect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>(Texture + "_Noise2").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>(Texture + "_Noise").Value);
			effect.Parameters["stretch"].SetValue(0.002f * modPlayer.charge);
			effect.Parameters["stretch2"].SetValue(0.0011f * modPlayer.charge);
			effect.Parameters["pallette"].SetValue(ModContent.Request<Texture2D>(Texture + "_Pallette").Value);
			effect.Parameters["uTime"].SetValue(-Main.GameUpdateCount * 0.035f);
			effect.Parameters["opacity"].SetValue((float)Math.Sqrt(modPlayer.charge / 150f));

			trail2?.Render(effect);
		}
	}
	public class IgnitionPunchPhantom : ModProjectile
	{

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public Vector2 directionVector = Vector2.Zero;
		private Player owner => Main.player[Projectile.owner];

		private bool front => Projectile.ai[0] == 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 13;
			Projectile.width = Projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			Vector2 direction = Projectile.DirectionTo(Main.MouseWorld);
			Projectile.Center = owner.Center + (direction * 20);

			Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
			float extend = (float)Math.Sin(Projectile.timeLeft / 2f);
			if (extend < 0.25f)
				stretch = Player.CompositeArmStretchAmount.None;
			else if (extend < 0.5f)
				stretch = Player.CompositeArmStretchAmount.Quarter;
			else if (extend < 0.8f)
				stretch = Player.CompositeArmStretchAmount.ThreeQuarters;
			else
				stretch = Player.CompositeArmStretchAmount.Full;
			if (front)
				owner.SetCompositeArmFront(true, stretch, directionVector.ToRotation() - 1.57f);
			else
				owner.SetCompositeArmBack(true, stretch, directionVector.ToRotation() - 1.57f);

		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			//Projectile.penetrate++;
			//Projectile.friendly = false;
			//owner.velocity = -owner.DirectionTo(Projectile.Center) * 6;
		}
	}
	public class IgnitionPunch : ModProjectile
	{

		public override string Texture => AssetDirectory.VitricItem + Name;

		private Player owner => Main.player[Projectile.owner];

		private bool front => Projectile.ai[0] == 0;

		private List<float> oldRotation = new List<float>();
		private List<Vector2> oldPosition = new List<Vector2>();

		private bool initialized = false;

		private Vector2 posToBe = Vector2.Zero;

		private float fade => Math.Min(1, Projectile.timeLeft / 7f);

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft = 20;
			Projectile.extraUpdates = 2;
			Projectile.width = Projectile.height = 18;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}
		public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				Vector2 direction = owner.DirectionTo(Main.MouseWorld);
				posToBe = owner.Center + (direction * 200);
			}
			if (Projectile.extraUpdates != 0)
			{
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(posToBe) * 7, 0.25f);
				Projectile.rotation = Projectile.velocity.ToRotation();
				oldRotation.Add(Projectile.rotation);
				oldPosition.Add(Projectile.Center);
			}

			if (oldRotation.Count > ((Projectile.extraUpdates == 2) ? 16 : 0))
				oldRotation.RemoveAt(0);
			if (oldPosition.Count > ((Projectile.extraUpdates == 2) ? 16 : 0))
				oldPosition.RemoveAt(0);

			if (Projectile.timeLeft < 7)
			{
				Projectile.friendly = false;
			}
			else
				Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.4f);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			int distance = (int)(owner.Center - Projectile.Center).Length();
			float pushback = (float)Math.Sqrt(200 * EaseFunction.EaseCubicIn.Ease((200 - distance) / 200f));
			Vector2 direction = target.DirectionTo(owner.Center);
			owner.velocity += direction * pushback * 0.15f;

			Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletsImpactRing>(), 0, 0, owner.whoAmI, Main.rand.Next(15, 25), Projectile.velocity.ToRotation());
			for (int i = 0; i < 7; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, 6, -Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(), 0, default, 1.25f).noGravity = true;
			}

			if (owner.GetModPlayer<IgnitionPlayer>().charge < 150)
				owner.GetModPlayer<IgnitionPlayer>().charge += 5;

			Main.NewText(owner.GetModPlayer<IgnitionPlayer>().charge);

			Projectile.penetrate += 2;
			Projectile.timeLeft = 20;
			Projectile.extraUpdates = 0;
			Projectile.friendly = false;
			Projectile.velocity = Vector2.Zero;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture + (front ? "" : "_Back")).Value;
			Texture2D afterTex = ModContent.Request<Texture2D>(Texture + "_After").Value;

			/*Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);*/

			for (int k = 15; k > 0; k--)
			{

				float progress = (float)(((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length));
				Color color = Color.White * progress * fade * 0.8f;
				if (k > 0 && k < oldRotation.Count)
					Main.spriteBatch.Draw(tex, oldPosition[k] - Main.screenPosition, null, color, oldRotation[k], tex.Size() / 2, Projectile.scale * 0.8f * progress, SpriteEffects.None, 0f);
			}

			/*Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.AlphaBlend, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);*/

			if (Projectile.extraUpdates != 0)
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * (float)Math.Sqrt(fade), Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
	internal class IgnitionGauntletsImpactRing : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		public int timeLeftStart = 10;
		private float Progress => 1 - (Projectile.timeLeft / (float)timeLeftStart);

		private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = timeLeftStart;
			Projectile.extraUpdates = 1;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void AI()
		{
			Projectile.velocity *= 0.95f;
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor) => false;

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;
			for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = (i / 32f) * 6.28f;
				Vector2 offset = new Vector2((float)Math.Sin(rad) * 0.4f, (float)Math.Cos(rad));
				offset *= radius;
				offset = offset.RotatedBy(Projectile.ai[1]);
				cache.Add(Projectile.Center + offset);
			}

			while (cache.Count > 33)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{

			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 28 * (1 - Progress), factor =>
			{
				return Color.Orange;
			});

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => 10 * (1 - Progress), factor =>
			{
				return Color.White;
			});
			float nextplace = 33f / 32f;
			Vector2 offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + offset;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);
		}
	}

	public class IgnitionPlayer : ModPlayer
	{
		public int charge = 0;

		public int potentialCharge = 0;

		public int loadedCharge = 0;
		public bool launching = false;
		public bool flipping = false;
		public float acceleration;

		private int rotationCounter = 0;

		public override void PreUpdate() //TODO: some rotdifference shenanagins here to make the rotation transition smoother
		{
			if (launching)
			{
				loadedCharge--;
				if (loadedCharge <= 0)
				{
					launching = false;
					Player.fullRotation = 0;
					flipping = false;
					acceleration = 0;
					return;
				}



				if (acceleration < 1)
					acceleration += 0.02f;
				float lerper = MathHelper.Min(rotationCounter / 8f, loadedCharge / 20f);

				if (!flipping)
				{
					Player.GetModPlayer<StarlightPlayer>().Shake = (int)(2 * lerper);
					Lighting.AddLight(Player.Center, Color.OrangeRed.ToVector3());
					for (int i = 0; i < 4; i++)
					{
						var pos = (Player.Center - new Vector2(4, 4)) - (Player.velocity * Main.rand.NextFloat(2));
						Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<IgnitionGauntletSmoke>(), Vector2.Normalize(-Player.velocity).RotatedByRandom(0.6f) * Main.rand.NextFloat(6.5f));
						dust.scale = Main.rand.NextFloat(0.35f, 0.75f);
						dust.alpha = Main.rand.Next(50);
						dust.rotation = Main.rand.NextFloatDirection();
					}

					for (int j = 0; j < 16; j++)
					{
						var pos = (Player.Center - new Vector2(4, 4)) - (Player.velocity * Main.rand.NextFloat(2));
						Dust dust2 = Dust.NewDustPerfect(pos, ModContent.DustType<IgnitionGauntletSmoke2>(), Vector2.Normalize(-Player.velocity).RotatedByRandom(3.0f) * Main.rand.NextFloat(12.5f));
						dust2.scale = Main.rand.NextFloat(0.25f, 0.45f);
						dust2.alpha = Main.rand.Next(50);
						dust2.rotation = Main.rand.NextFloatDirection();
					}

					Vector2 offset = Vector2.Normalize(Player.velocity.RotatedBy(1.57f)) * Main.rand.Next(-30, 30);
					Dust dust3 = Dust.NewDustPerfect(Player.Center + offset + (Player.velocity * 5), ModContent.DustType<IgnitionGauntletWind>(), Vector2.Normalize(-Player.velocity) * Main.rand.NextFloat(6.5f), 0, Color.White, 1.5f);
					dust3.rotation = dust3.velocity.ToRotation();
					dust3.position -= (dust3.rotation - 1.57f).ToRotationVector2() * 35;
					Player.velocity = Vector2.Lerp(Player.velocity, Player.DirectionTo(Main.MouseWorld) * 20 * (float)Math.Sqrt(lerper), 0.15f * acceleration);
				}
				Player.fullRotationOrigin = Player.Size / 2;

				Player.fullRotation = Player.DirectionTo(Main.MouseWorld).ToRotation() + 1.57f;
				if (flipping)
					Player.fullRotation += 6.28f;
				Player.fullRotation *= lerper;

				if (rotationCounter < 8)
					rotationCounter++;

			}
			else
			{
				rotationCounter = 0;
			}
		}

		public override void PostUpdate()
		{
			if (launching && !flipping)
			{
				Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, 1.57f * Player.direction);
			}
		}
		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			if (launching)
			{
				return false;
			}
			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
		}
	}

	public class IgnitionGauntletCharge : ModProjectile
	{
		int charge = 0;
		public override string Texture => AssetDirectory.Assets + "Invisible";
		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 50;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

		public override void AI()
		{
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = owner.Center;
			if (Main.mouseRight)
			{
				if (modPlayer.charge > charge)
				{
					charge += 3;
				}
				modPlayer.potentialCharge = charge;
				Main.NewText(charge.ToString(), 255, 0, 100);
			}
			else
			{
				modPlayer.potentialCharge = 0;
				if (!owner.GetModPlayer<IgnitionPlayer>().launching)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletLaunch>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
					//owner.velocity = owner.DirectionTo(Main.MouseWorld) * 20;
				}

				//UNCOMMENT THIS BEFORE RELEASE
				//modPlayer.charge -= charge;
				modPlayer.loadedCharge = charge;
				owner.GetModPlayer<IgnitionPlayer>().launching = true;
				Projectile.active = false;
			}
		}
	}

	public class IgnitionGauntletCone : ModProjectile
	{

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public Vector2 directionVector = Vector2.Zero;
		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft = 5;
			Projectile.width = Projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			for (float i = -0.7f; i < 0.7f; i += 0.05f)
			{
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + ((Projectile.rotation + i).ToRotationVector2() * 220 * Projectile.ai[0])))
					return true;
			}
			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 180);
		}

	}

	public class IgnitionGauntletLaunch : ModProjectile
	{
		public override string Texture => AssetDirectory.VitricItem + Name;
		private Player owner => Main.player[Projectile.owner];

		public float noiseRotation;

		public float noiseRotation2;

		public override void Load()
		{
			On.Terraria.Main.DrawDust += DrawCone;
		}

		public override void Unload()
		{
			On.Terraria.Main.DrawDust -= DrawCone;
		}
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 50;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (noiseRotation < 0.02f)
				noiseRotation = Main.rand.NextFloat(6.28f);
			noiseRotation += 0.12f;

			if (noiseRotation2 < 0.02f)
				noiseRotation2 = Main.rand.NextFloat(6.28f);
			noiseRotation2 -= 0.12f;

			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = owner.Center;

			Projectile.rotation = owner.fullRotation;
			if (!modPlayer.launching)
			{
				Projectile.Kill();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		private void DrawCone(On.Terraria.Main.orig_DrawDust orig, Main self) //putting this here so I dont have to load another detour to get it to load in front of the fist
		{
			orig(self);

			//putting this here so I dont have to load another detour to get it to load in front of the fist
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			Main.spriteBatch.End();

			Color color = Color.OrangeRed;
			color.A = 0;

			foreach (Projectile Projectile in Main.projectile)
			{
				Player player = Main.player[Projectile.owner];
				Texture2D starTex = ModContent.Request<Texture2D>(Texture + "_Star").Value;

				if (Projectile.type == ModContent.ProjectileType<IgnitionGauntletLaunch>() && Projectile.active && player.GetModPlayer<IgnitionPlayer>().loadedCharge > 15)
				{
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
					Vector2 handOffset = new Vector2(-10 * player.direction, -16);
					handOffset = handOffset.RotatedBy(player.fullRotation);
					Main.spriteBatch.Draw(starTex, (player.Center + handOffset) - Main.screenPosition, null, Color.White * MathHelper.Min(1, player.GetModPlayer<IgnitionPlayer>().loadedCharge / 15f), (float)Main.GameUpdateCount * 0.085f, starTex.Size() / 2, 1.5f + (0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.285f)), SpriteEffects.None, 0f);
					Main.spriteBatch.End();

					var mp = Projectile.ModProjectile as IgnitionGauntletLaunch;
					Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
					Effect effect = Filters.Scene["ConicalNoise"].GetShader().Shader;
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

					effect.Parameters["vnoise"].SetValue(ModContent.Request<Texture2D>(Texture + "_noise").Value);
					effect.Parameters["rotation"].SetValue(mp.noiseRotation);
					effect.Parameters["transparency"].SetValue(1f);
					effect.Parameters["pallette"].SetValue(ModContent.Request<Texture2D>(Texture + "_pallette").Value);
					effect.Parameters["color"].SetValue(Color.White.ToVector4());
					effect.CurrentTechnique.Passes[0].Apply();

					Main.spriteBatch.Draw(tex, Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2() * 30) - Main.screenPosition, null, color, Projectile.rotation - 1.57f, new Vector2(250, 64), new Vector2(0.4f, 0.4f), SpriteEffects.None, 0f);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

					effect.Parameters["vnoise"].SetValue(ModContent.Request<Texture2D>(Texture + "_noise").Value);
					effect.Parameters["rotation"].SetValue(mp.noiseRotation2);
					effect.Parameters["transparency"].SetValue(1f);
					effect.Parameters["pallette"].SetValue(ModContent.Request<Texture2D>(Texture + "_pallette").Value);
					effect.Parameters["color"].SetValue(Color.White.ToVector4());
					effect.CurrentTechnique.Passes[0].Apply();

					Main.spriteBatch.Draw(tex, Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2() * 30) - Main.screenPosition, null, color, Projectile.rotation - 1.57f, new Vector2(250, 64), new Vector2(0.4f, 0.4f), SpriteEffects.None, 0f);

					Main.spriteBatch.End();
				}
			}
		}
	}

	public class IgnitionGauntletSmoke : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDustThree";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.scale *= 0.3f;
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 40)
				ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 40f);
			else if (dust.alpha < 80)
				ret = Color.Lerp(Color.Orange, Color.Red, (dust.alpha - 40) / 40f);
			else if (dust.alpha < 160)
				ret = Color.Lerp(Color.Red, gray, (dust.alpha - 80) / 80f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData == null)
			{
				dust.customData = 0;
			}

			if ((int)dust.customData < 10)
			{
				dust.scale *= 1.1f;
				dust.customData = (int)dust.customData + 1;
			}
			else
			{
				if (dust.alpha > 60)
				{
					dust.scale *= 0.96f;
				}
				else
				{
					dust.scale *= 0.93f;
				}
			}


			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.85f;
			else
				dust.velocity *= 0.92f;

			if (dust.alpha > 60)
			{
				dust.alpha += 12;
			}
			else
			{
				dust.alpha += 8;
			}

			Lighting.AddLight(dust.position, ((Color)(GetAlpha(dust, Color.White))).ToVector3() * 0.5f);

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
	public class IgnitionGauntletSmoke2 : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDustThree";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.scale *= 0.3f;
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 40)
				ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 40f);
			else if (dust.alpha < 80)
				ret = Color.Lerp(Color.Orange, Color.Red, (dust.alpha - 40) / 40f);
			else if (dust.alpha < 160)
				ret = Color.Lerp(Color.Red, gray, (dust.alpha - 80) / 80f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData == null)
			{
				dust.customData = 0;
			}

			if ((int)dust.customData < 10)
			{
				dust.scale *= 1.07f;
				dust.customData = (int)dust.customData + 1;
			}
			else
			{
				if (dust.alpha > 60)
				{
					dust.scale *= 0.98f;
				}
				else
				{
					dust.scale *= 0.96f;
				}
			}


			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.92f;
			else
				dust.velocity *= 0.96f;

			dust.alpha += 30;

			Lighting.AddLight(dust.position, ((Color)(GetAlpha(dust, Color.White))).ToVector3() * 0.5f);

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
	public class IgnitionGauntletSmoke3 : IgnitionGauntletSmoke
	{
		public override bool Update(Dust dust)
		{
			if (dust.customData == null)
			{
				dust.customData = 0;
			}

			if ((int)dust.customData < 7)
			{
				dust.scale *= 1.13f;
				dust.customData = (int)dust.customData + 1;
			}
			else
			{
				if (dust.alpha > 60)
				{
					dust.scale *= 0.96f;
				}
				else
				{
					dust.scale *= 0.93f;
				}
			}


			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.84f;
			else
				dust.velocity *= 0.93f;

			if (dust.alpha > 60)
			{
				dust.alpha += 6;
			}
			else
			{
				dust.alpha += 3;
			}

			Lighting.AddLight(dust.position, ((Color)(GetAlpha(dust, Color.White))).ToVector3() * 0.5f);

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
	class IgnitionGauntletSpark : BuzzSpark
	{
		public override bool Update(Dust dust)
		{
			if (dust.customData is null)
			{
				dust.position -= new Vector2(2.5f, 25).RotatedBy(dust.rotation) * dust.scale;
				dust.customData = 1;
			}

			dust.frame.Y++;
			dust.frame.Height--;

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.position += dust.velocity;

			dust.color.G -= 8;
			dust.color.A -= 5;

			dust.velocity.X *= 0.98f;
			dust.velocity.Y *= 0.95f;

			dust.velocity.Y += 0.15f;

			float mult = 1;

			if (dust.fadeIn < 5)
				mult = dust.fadeIn / 5f;

			dust.shader.UseSecondaryColor(new Color((int)(255 * (1 - (dust.fadeIn / 20f))), 0, 0) * mult);
			dust.shader.UseColor(dust.color * mult);
			dust.fadeIn += 2;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.02f);

			if (dust.fadeIn > 45)
				dust.active = false;
			return false;
		}
	}
	class IgnitionGauntletWind : BuzzSpark
	{
		public override bool Update(Dust dust)
		{
			if (dust.customData is null)
			{
				dust.position -= new Vector2(2.5f, 25).RotatedBy(dust.rotation) * dust.scale;
				dust.customData = 1;
			}

			dust.frame.Y++;
			dust.frame.Height--;

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.position += dust.velocity;

			dust.color.G -= 8;
			dust.color.A -= 5;

			dust.velocity.X *= 0.98f;
			dust.velocity.Y *= 0.95f;

			dust.velocity.Y += 0.15f;

			float mult = MathHelper.Min(dust.fadeIn / 15f, (45 - dust.fadeIn) / 15f);

			dust.shader.UseSecondaryColor(new Color((int)(255 * (1 - (dust.fadeIn / 20f))), 0, 0) * mult);
			dust.shader.UseColor(dust.color * mult);
			dust.fadeIn += 2;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.02f);

			if (dust.fadeIn > 45)
				dust.active = false;
			return false;
		}
	}
	public class IgnitionGauntletStar : ModDust
	{
		public override string Texture => AssetDirectory.VitricItem + "IgnitionGauntletStar";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = true;
			dust.frame = new Rectangle(0, 0, 18, 18);

		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return Color.White;
		}

		public override bool Update(Dust dust)
        {
			dust.rotation += 0.125f * dust.scale;
			dust.position += dust.velocity;
			dust.fadeIn++;
			dust.scale = 1 + (float)(2 * Math.Sin(dust.fadeIn * 0.157f));
			if (dust.scale <= 0)
				dust.active = false;
			return false;
        }
    }
}