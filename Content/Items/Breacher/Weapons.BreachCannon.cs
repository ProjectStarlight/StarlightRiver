//TODO:
//Make lasers go through platforms
//Make super laser not pierce
//Make them not sometimes despawn
//Make it break if you break the tile
//Sell price
//Rarity
//Obtainment
//Balance
//waves on bloom
//Grasscutting
//Remove main.newtext
//Improve item usestyle
//Sfx
//Lighting
//Description


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Items.Breacher
{
	class BreachCannon : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breach Cannon");
			Tooltip.SetDefault("Summons a sentry that shoots a laser towards the cursor");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.QueenSpiderStaff);
			Item.damage = 19;
			Item.mana = 12;
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 0, 80, 0);
			Item.rare = ItemRarityID.Green;
			Item.knockBack = 2.5f;
			Item.UseSound = SoundID.Item25;
			Item.shoot = ModContent.ProjectileType<BreachCannonSentry>();
			Item.shootSpeed = 0f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			position = Main.MouseWorld;
			//0 = right
			//1 = bottom
			//2 = left
			//3 = top

			int minDistance = 99;
			int minDirection = 0;

			Vector2 originTile = Vector2.Zero;
			for (int k = 0; k < 4; k++)
			{
				Vector2 testPosition = position / 16;
				testPosition.X = (int)testPosition.X;
				testPosition.Y = (int)testPosition.Y;
				Vector2 direction = Vector2.UnitX.RotatedBy(k * 1.57f);
				int testDistance = 0;
				while (testDistance < 40)
				{
					testPosition += direction;
					testDistance++;
					if (testDistance > minDistance)
						break;

					int i = (int)testPosition.X;
					int j = (int)testPosition.Y;
					Tile testTile = Main.tile[i, j];
					if (testTile.HasTile && Main.tileSolid[testTile.TileType])
					{
						minDistance = testDistance;
						minDirection = k;
						originTile = testPosition;
						break;
					}
				}
			}

			if (minDistance == 99)
			{
				Main.NewText("Placement failed");
				return false;
			}
			Projectile proj = Projectile.NewProjectileDirect(source, (originTile - Vector2.UnitX.RotatedBy(minDirection * 1.57f)) * 16, velocity, type, damage, knockback, player.whoAmI, minDirection);
			var mp = proj.ModProjectile as BreachCannonSentry;
			mp.tileOrigin = originTile;
			proj.originalDamage = Item.damage;
			proj.rotation = (minDirection * 1.57f) + 3.14f;
			player.UpdateMaxTurrets();
			Main.NewText(minDirection.ToString());
			return false;
		}
	}

	public class BreachCannonSentry : ModProjectile, IDrawPrimitive, IDrawAdditive
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		private Player owner => Main.player[Projectile.owner];


		//0 = right
		//1 = bottom
		//2 = left
		//3 = top
		private int originAngle => (int)Projectile.ai[0];

		private float originAngleRad => originAngle * 1.57f;

		public Vector2 tileOrigin = Vector2.Zero;

		public Vector2 tileWorldPos => (tileOrigin * 16) + new Vector2(8, 8);

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private List<Vector2> superCache;
		private Trail superTrail;
		private Trail superTrail2;
		private float laserLength => (laserEndpoint - laserStartpoint).Length();

		public Vector2 laserStartpoint = Vector2.Zero;
		public Vector2 laserEndpoint;

		public bool superLaser = false;
		public bool superLaserContributer = false;
		public int superCharge = 0;
		public Vector2 superLaserStartpoint;
		public Vector2 superLaserEndpoint;

        public override void Load()
        {
			On.Terraria.Main.PreUpdateAllProjectiles += ResetLasers;
        }

        public override void Unload()
        {
			On.Terraria.Main.PreUpdateAllProjectiles -= ResetLasers;
		}

        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breach Cannon");
		}

		public override void SetDefaults()
		{
			Projectile.width = 58;
			Projectile.height = 58;
			Projectile.timeLeft = Projectile.SentryLifeTime;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.sentry = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.ignoreWater = true;
		}

		private void ResetLasers(On.Terraria.Main.orig_PreUpdateAllProjectiles orig, Main self)
        {
			orig(self);
			for (int index = 0; index < Main.projectile.Length; index++)
            {
				Projectile proj = Main.projectile[index];
				if (!proj.active || proj.type != ModContent.ProjectileType<BreachCannonSentry>())
					continue;
				var mp = proj.ModProjectile as BreachCannonSentry;
				mp.superLaser = false;
				mp.superLaserContributer = false;
            }
        }

		public override void AI()
		{
			Projectile.Center = tileWorldPos - originAngleRad.ToRotationVector2() * 26;
			float rotDifference = Helper.RotationDifference(Projectile.DirectionTo(Main.MouseWorld).ToRotation(), Projectile.rotation);
			if (owner.HeldItem.type == ModContent.ItemType<BreachCannon>())
			Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.rotation + rotDifference, 0.07f);

			laserStartpoint = Projectile.Center + (Projectile.rotation.ToRotationVector2().RotatedBy(InvertRotation() ? 0.2f : -0.2f) * 44);
			Vector2 offset = CalculateOffset();
			if (!superLaserContributer)
				laserEndpoint = laserStartpoint + offset;

			if (!superLaserContributer)
				SuperLaserCheck();

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			SpawnParticles();
		}

		public override bool PreDraw(ref Color lightColor)
		{

			var spriteBatch = Main.spriteBatch;

			DrawTrail(spriteBatch);
			Texture2D cannonTex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D baseTex = ModContent.Request<Texture2D>(Texture + "_Base").Value;

			Texture2D cannonGlowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D baseGlowTex = ModContent.Request<Texture2D>(Texture + "_Base_Glow").Value;

			float baseRotation = -1.57f + originAngleRad;
			Vector2 baseOrigin = new Vector2(baseTex.Width / 2, baseTex.Height + 8);

			Main.spriteBatch.Draw(baseTex, tileWorldPos - Main.screenPosition, null, lightColor, baseRotation, baseOrigin, Projectile.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(baseGlowTex, tileWorldPos - Main.screenPosition, null, Color.White, baseRotation, baseOrigin, Projectile.scale, SpriteEffects.None, 0f);

			float cannonRotation = Projectile.rotation;
			Vector2 cannonOrigin = new Vector2(cannonTex.Width * 0.3f, cannonTex.Height * 0.75f);
			SpriteEffects cannonEffects = SpriteEffects.None;
			if (InvertRotation())
			{
				cannonEffects = SpriteEffects.FlipHorizontally;
				cannonOrigin.X = cannonTex.Width - cannonOrigin.X;
				cannonRotation -= 3.14f;
			}
			Main.spriteBatch.Draw(cannonTex, Projectile.Center - Main.screenPosition, null, lightColor, cannonRotation, cannonOrigin, Projectile.scale, cannonEffects, 0f);
			Main.spriteBatch.Draw(cannonGlowTex, Projectile.Center - Main.screenPosition, null, Color.White, cannonRotation, cannonOrigin, Projectile.scale, cannonEffects, 0f);

			//DrawBeamStart(spriteBatch);

			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;
			if (superLaser && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), superLaserStartpoint, superLaserEndpoint, 15 * superCharge, ref collisionPoint))
				return true;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), laserStartpoint, laserEndpoint, 15, ref collisionPoint);
		}

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			hitDirection = Math.Sign(target.Center.X - Projectile.Center.X);
			Rectangle targetHitbox = target.Hitbox;
			float collisionPoint = 0f;
			if (superLaser && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), superLaserStartpoint, superLaserEndpoint, 15 * superCharge, ref collisionPoint))
            {
				damage = (int)(damage * Math.Sqrt(superCharge));
            }
        }

        private void SuperLaserCheck()
        {
			for (int projIndex = 0; projIndex < Main.projectile.Length; projIndex++) //super laser dealings
			{
				Projectile proj = Main.projectile[projIndex];
				if (!proj.active || proj.type != Projectile.type || proj == Projectile)
					continue;
				var mp = proj.ModProjectile as BreachCannonSentry;
				if (mp.superLaser || mp.superLaserContributer)
					continue;

				Vector2 mpTestEndpoint = mp.laserStartpoint + mp.CalculateOffset();
				Vector2[] collisionPoint = Collision.CheckLinevLine(laserStartpoint, laserEndpoint, mp.laserStartpoint, mpTestEndpoint);
				if (collisionPoint.Length > 0)
				{
					mp.laserEndpoint = collisionPoint[0];
					mp.superLaserContributer = true;

					superLaser = true;
					laserEndpoint = collisionPoint[0];
					superCharge = 2;

					Rectangle additionalCheck = new Rectangle((int)(laserEndpoint.X - 15), (int)(laserEndpoint.Y - 15), 30, 30);

					float angleTotal = proj.rotation + Projectile.rotation;
					for (int projIndex2 = 0; projIndex2 < Main.projectile.Length; projIndex2++)
					{
						Projectile proj2 = Main.projectile[projIndex2];
						if (!proj2.active || proj2.type != Projectile.type)
							continue;
						var mp2 = proj2.ModProjectile as BreachCannonSentry;
						if (mp2.superLaser || mp2.superLaserContributer)
							continue;
						float collisionPoint2 = 0f;
						if (Collision.CheckAABBvLineCollision(additionalCheck.TopLeft(), additionalCheck.Size(), mp2.laserStartpoint, mp2.laserStartpoint + mp2.CalculateOffset(), 30, ref collisionPoint2))
						{
							mp2.superLaserContributer = true;
							mp2.laserEndpoint = laserEndpoint;
							angleTotal += proj2.rotation;
							superCharge++;
						}
					}

					float superAngle = angleTotal / superCharge;

					superLaserStartpoint = laserEndpoint;

					Vector2 offset = Vector2.Zero;
					for (int k = 0; k < 50; k++)
					{
						offset = superAngle.ToRotationVector2() * k * 16;

						int i = (int)((superLaserStartpoint.X + offset.X) / 16);
						int j = (int)((superLaserStartpoint.Y + offset.Y) / 16);
						Tile testTile = Main.tile[i, j];
						if (testTile.HasTile && Main.tileSolid[testTile.TileType])
						{
							break;
						}
					}
					superLaserEndpoint = superLaserStartpoint + offset;


					break;
				}
			}
		}
		private void ManageCaches()
		{
			cache = new List<Vector2>();
			for (int i = 0; i < 14; i++)
			{
				cache.Add(Vector2.Lerp(laserStartpoint, laserEndpoint, i / 14f));
			}
			cache.Add(laserEndpoint);

			superCache = new List<Vector2>();
			for (int i = 0; i < 14; i++)
			{
				superCache.Add(Vector2.Lerp(superLaserStartpoint, superLaserEndpoint, i / 14f));
			}
			superCache.Add(superLaserEndpoint);
		}

		private void ManageTrail()
		{
			Color blue = new Color(0, 0, 255);
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 15, factor =>
			{
				return Color.Lerp(Color.Cyan, blue, factor.Y);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = laserEndpoint;

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 7, factor =>
			{
				return Color.White;
			});

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = laserEndpoint;

			superTrail = superTrail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 15 * superCharge, factor =>
			{
				return Color.Lerp(Color.Cyan, blue, factor.Y);
			});

			superTrail.Positions = superCache.ToArray();
			superTrail.NextPosition = superLaserEndpoint;

			superTrail2 = superTrail2 ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 7 * superCharge, factor =>
			{
				return Color.White;
			});

			superTrail2.Positions = superCache.ToArray();
			superTrail2.NextPosition = superLaserEndpoint;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["BreachLaser"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["noiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.25f);
			effect.Parameters["stretch"].SetValue(2f / laserLength);

			effect.Parameters["dilation"].SetValue(0.8f);
			effect.Parameters["falloff"].SetValue(1);

			trail?.Render(effect);
			if (superLaser)
				superTrail?.Render(effect);

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["BreachLaser"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["noiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.25f);
			effect.Parameters["stretch"].SetValue(2f / laserLength);

			effect.Parameters["dilation"].SetValue(0.8f);
			effect.Parameters["falloff"].SetValue(1);

			trail2?.Render(effect);
			if (superLaser)
				superTrail2?.Render(effect);
		}

		private bool InvertRotation()
		{
			Vector2 originVector = Projectile.rotation.ToRotationVector2();
			switch (originAngle)
			{
				case 0:
					return originVector.Y > 0;
				case 1:
					return originVector.X < 0;
				case 2:
					return originVector.Y < 0;
				case 3:
					return originVector.X > 0;
			}
			return false;
		}

		private Vector2 CalculateOffset()
        {
			if (laserStartpoint == Vector2.Zero)
				return Vector2.One;
			Vector2 offset = Vector2.Zero;
			for (int k = 0; k < 50; k++)
			{
				offset = Projectile.rotation.ToRotationVector2() * k * 16;

				int i = (int)((laserStartpoint.X + offset.X) / 16);
				int j = (int)((laserStartpoint.Y + offset.Y) / 16);
				Tile testTile = Main.tile[i, j];
				if (testTile.HasTile && Main.tileSolid[testTile.TileType])
				{
					break;
				}
			}

			Vector2 testEndpoint = laserStartpoint + offset;
			for (int index = 0; index < Main.npc.Length; index++)
			{
				NPC npc = Main.npc[index];
				if (!npc.active)
					continue;
				float collisionPoint = 0f;
				if (Collision.CheckAABBvLineCollision(npc.Hitbox.TopLeft(), npc.Hitbox.Size(), laserStartpoint, testEndpoint, 15, ref collisionPoint))
				{
					testEndpoint = Vector2.Lerp(laserStartpoint, testEndpoint, (float)collisionPoint / (laserStartpoint - testEndpoint).Length());
				}
			}

			return testEndpoint - laserStartpoint;
		}


		public void DrawAdditive(SpriteBatch sb)
		{
			Color blue = Color.Lerp(Color.Cyan, new Color(0, 0, 255), 0.5f);

			//sb.End();
			//sb.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			var tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;

			for (int i = 0; i < 4; i++)
			{
				sb.Draw(tex, laserStartpoint - Main.screenPosition, null, blue, 0, tex.Size() / 2, 0.45f, SpriteEffects.None, 0f);
				sb.Draw(tex, laserStartpoint - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, 0.25f, SpriteEffects.None, 0f);
			}

			for (int i = 0; i < 4; i++)
			{
				sb.Draw(tex, laserEndpoint - Main.screenPosition, null, blue, 0, tex.Size() / 2, 0.35f, SpriteEffects.None, 0f);
				sb.Draw(tex, laserEndpoint - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, 0.2f, SpriteEffects.None, 0f);
			}

			if (superLaser)
            {
				for (int i = 0; i < 4; i++)
				{
					sb.Draw(tex, superLaserStartpoint - Main.screenPosition, null, blue, 0, tex.Size() / 2, 0.45f * superCharge, SpriteEffects.None, 0f);
					sb.Draw(tex, superLaserStartpoint - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, 0.25f * superCharge, SpriteEffects.None, 0f);
				}

				for (int i = 0; i < 4; i++)
				{
					sb.Draw(tex, superLaserEndpoint - Main.screenPosition, null, blue, 0, tex.Size() / 2, 0.35f * superCharge, SpriteEffects.None, 0f);
					sb.Draw(tex, superLaserEndpoint - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, 0.2f * superCharge, SpriteEffects.None, 0f);
				}
			}

			//sb.End();
			//sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private void SpawnParticles()
		{
			Vector2 direction = laserEndpoint.DirectionTo(laserStartpoint);

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(laserEndpoint, ModContent.DustType<BreachImpactGlow>(), direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(12), 0, Color.Cyan, Main.rand.NextFloat(0.25f, 0.6f));
			}

			for (int i = 0; i < 3; i++)
			{
				Vector2 vel = direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(9);
				Dust.NewDustPerfect(laserEndpoint + new Vector2(0, 35) + (vel * 2), ModContent.DustType<BreachImpactSpark>(), vel, 0, Color.Cyan, Main.rand.NextFloat(1.25f, 1.6f));
			}

			if (superLaser)
            {
				direction = superLaserEndpoint.DirectionTo(superLaserStartpoint);

				for (int i = 0; i < 5; i++)
				{
					Dust.NewDustPerfect(superLaserEndpoint, ModContent.DustType<BreachImpactGlow>(), direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(12), 0, Color.Cyan, Main.rand.NextFloat(0.25f, 0.6f));
				}

				for (int i = 0; i < 3; i++)
				{
					Vector2 vel = direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(9);
					Dust.NewDustPerfect(superLaserEndpoint + new Vector2(0, 35) + (vel * 2), ModContent.DustType<BreachImpactSpark>(), vel, 0, Color.Cyan, Main.rand.NextFloat(1.25f, 1.6f));
				}
			}
		}
	}
	public class BreachImpactGlow : Dusts.Glow
	{
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 64, 64);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
			int a = 1;
		}
		public override bool Update(Dust dust)
        {
			dust.scale *= 0.85f;
			return base.Update(dust);
        }
	}
	class BreachImpactSpark : Dusts.BuzzSpark
    {
		public override void OnSpawn(Dust dust)
		{
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 5, 50);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/ShrinkingDust").Value), "ShrinkingDustPass");
		}
		public override bool Update(Dust dust)
		{
			dust.fadeIn ++;
			return base.Update(dust);
		}
	}
}
