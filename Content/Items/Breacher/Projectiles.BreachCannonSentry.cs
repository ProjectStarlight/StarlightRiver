
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Linq;
using System.Collections.Generic;

using Terraria;
using Terraria.Enums;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Items.Breacher
{ 
	class BreacherLaserUpdater : ModSystem
	{
		public override void PreUpdateProjectiles()
		{
			for (int index = 0; index < Main.projectile.Length; index++)
			{
				Projectile proj = Main.projectile[index];
				if (!proj.active || proj.type != ModContent.ProjectileType<BreachCannonSentry>())
					continue;
				var mp = proj.ModProjectile as BreachCannonSentry;
				mp.superLaser = false;
				mp.superLaserContributer = false;
			}

			base.PreUpdateProjectiles();
		}
	}

	public class BreachCannonSentry : ModProjectile, IDrawPrimitive, IDrawAdditive
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;


		public Vector2 tileOrigin = Vector2.Zero;

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private float laserSizeMult = 0;

		private List<Vector2> superCache;
		private Trail superTrail;
		private Trail superTrail2;

		public Vector2 laserStartpoint = Vector2.Zero;
		public Vector2 laserEndpoint;

		public bool superLaser = false;
		public bool superLaserContributer = false;
		public int superCharge = 0;
		public Vector2 superLaserStartpoint;
		public Vector2 superLaserEndpoint;

		private float superLaserSizeMult = 0;

		//0 = right
		//1 = bottom
		//2 = left
		//3 = top
		private ref float originAngle => ref Projectile.ai[0];

		private float originAngleRad => originAngle * 1.57f;

		private float laserLength => (laserEndpoint - laserStartpoint).Length();

		public Vector2 tileWorldPos => (tileOrigin * 16) + new Vector2(8, 8);

		private Player owner => Main.player[Projectile.owner];

		public override void Load()
        {
			for (int k = 1; k <= 5; k++)
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "Gore" + k);
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
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.sentry = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			Projectile.Center = tileWorldPos - originAngleRad.ToRotationVector2() * 26;
			float rotDifference = Helper.RotationDifference(Projectile.DirectionTo(Main.MouseWorld).ToRotation(), Projectile.rotation);

			if (owner.HeldItem.type == ModContent.ItemType<BreachCannon>())
				Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.rotation + rotDifference, 0.07f);

			laserStartpoint = Projectile.Center + (Projectile.rotation.ToRotationVector2().RotatedBy(InvertRotation() ? 0.2f : -0.2f) * 44);
			Vector2 offset = CalculateOffset(laserStartpoint, Projectile.rotation, 15, 1);

			if (!superLaserContributer)
			{
				laserEndpoint = laserStartpoint + offset;
				SuperLaserCheck();
			}

			if (superLaser)
				superLaserSizeMult = MathHelper.Lerp(superLaserSizeMult, superCharge, 0.05f);
			else
				superLaserSizeMult *= 0.95f;

			laserSizeMult = MathHelper.Lerp(laserSizeMult, 1, 0.07f);

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			SpawnParticles();

			if (!Main.tile[(int)tileOrigin.X, (int)tileOrigin.Y].HasTile && Projectile.timeLeft > 2)
			{
				//Main.NewText("Killed at (" + ((int)tileOrigin.X).ToString() + "," + ((int)tileOrigin.Y).ToString() + ")");
				Projectile.timeLeft = 2;
			}

			Color color = Color.Lerp(Color.Cyan, new Color(0, 0, 255), 0.5f);
			Lighting.AddLight(laserStartpoint, color.ToVector3() * laserSizeMult);
		}

        public override void Kill(int timeLeft)
        {
			//Main.NewText(Projectile.Center.X.ToString());
			for (int k = 1; k <= 5; k++)
				Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("BreachCannonSentryGore" + k).Type);
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

			DrawBalls(Main.spriteBatch, BlendState.AlphaBlend, new Color(0,0,255), Color.Cyan, 0.65f);
			//DrawBeamStart(spriteBatch);

			return false;
		}

		public override bool? CanCutTiles()
		{
			return true;
		}

		public override void CutTiles()
		{
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Utils.PlotTileLine(laserStartpoint, laserEndpoint, 15, DelegateMethods.CutTiles);
			if (superLaser)
				Utils.PlotTileLine(superLaserStartpoint, superLaserEndpoint, 15 * superCharge, DelegateMethods.CutTiles);
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
				damage = (int)(damage * Math.Sqrt(superCharge));
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

				Vector2 mpTestEndpoint = mp.laserStartpoint + mp.CalculateOffset(mp.laserStartpoint, proj.rotation, 15, 1);
				Vector2[] collisionPoint;

				if (laserStartpoint == laserEndpoint || mp.laserStartpoint == mp.laserEndpoint)
					collisionPoint = new Vector2[0];
				else
					collisionPoint = Collision.CheckLinevLine(laserStartpoint, laserEndpoint, mp.laserStartpoint, mpTestEndpoint);

				if (collisionPoint.Length > 0)
				{
					mp.laserEndpoint = collisionPoint[0];
					mp.superLaserContributer = true;

					superLaser = true;
					laserEndpoint = collisionPoint[0];
					superCharge = 2;

					Rectangle additionalCheck = new Rectangle((int)(laserEndpoint.X - 15), (int)(laserEndpoint.Y - 15), 30, 30);

					float superAngle = MathHelper.Lerp(Projectile.rotation, Projectile.rotation + Helper.RotationDifference(proj.rotation, Projectile.rotation), 0.5f);
					
					for (int projIndex2 = 0; projIndex2 < Main.projectile.Length; projIndex2++)
					{
						Projectile proj2 = Main.projectile[projIndex2];
						if (!proj2.active || proj2.type != Projectile.type)
							continue;

						var mp2 = proj2.ModProjectile as BreachCannonSentry;
						if (mp2.superLaser || mp2.superLaserContributer)
							continue;

						float collisionPoint2 = 0f;

						Vector2 mp2Offset = mp2.CalculateOffset(mp2.laserStartpoint, proj2.rotation, 15, 1);
						if (mp2Offset == Vector2.Zero)
							continue;

						if (Collision.CheckAABBvLineCollision(additionalCheck.TopLeft(), additionalCheck.Size(), mp2.laserStartpoint, mp2.laserStartpoint + mp2Offset, 30, ref collisionPoint2))
						{
							mp2.superLaserContributer = true;
							mp2.laserEndpoint = laserEndpoint;
							superAngle = MathHelper.Lerp(superAngle, superAngle + Helper.RotationDifference(proj2.rotation, superAngle), 1.0f / superCharge);
							superCharge++;
						}
					}


					superLaserStartpoint = laserEndpoint;
					superLaserEndpoint = superLaserStartpoint + CalculateOffset(superLaserStartpoint, superAngle, 15 * superCharge, superCharge);
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
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 15 * laserSizeMult, factor =>
			{
				return Color.Lerp(Color.Cyan, blue, factor.Y);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = laserEndpoint;

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 7 * laserSizeMult, factor =>
			{
				return Color.White;
			});

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = laserEndpoint;

			superTrail = superTrail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 15 * superLaserSizeMult, factor =>
			{
				return Color.Lerp(Color.Cyan, blue, factor.Y);
			});

			superTrail.Positions = superCache.ToArray();
			superTrail.NextPosition = superLaserEndpoint;

			superTrail2 = superTrail2 ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 7 * superLaserSizeMult, factor =>
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

		private Vector2 CalculateOffset(Vector2 start, float rot, int width, int pierce)
        {
			if (start == Vector2.Zero)
				return Vector2.One;

			Vector2 offset = Vector2.Zero;

			for (int k = 0; k < 50; k++)
			{
				offset = rot.ToRotationVector2() * k * 16;

				int i = (int)((start.X + offset.X) / 16);
				int j = (int)((start.Y + offset.Y) / 16);
				Tile testTile = Main.tile[i, j];

				if (testTile.HasTile && Main.tileSolid[testTile.TileType] && !TileID.Sets.Platforms[testTile.TileType])
				{
					break;
				}
			}

			Vector2 testEndpoint = start + offset;
			NPC[] sortedNPC = Main.npc.Where(n => n.active && !n.friendly && !n.CountsAsACritter).OrderBy(n => (n.Center - start).Length()).ToArray();

			if (sortedNPC.Length == 0)
				return testEndpoint - start;

			int pierceLeft = pierce;

			for (int index = 0; index < sortedNPC.Length; index++)
			{
				NPC npc = sortedNPC[index];
				float collisionPoint = 0f;

				if (Collision.CheckAABBvLineCollision(npc.Hitbox.TopLeft(), npc.Hitbox.Size(), start, testEndpoint, width, ref collisionPoint))
				{
					pierce--;
					if (pierce <= 0)
                    {
						testEndpoint = Vector2.Lerp(start, testEndpoint, (float)(collisionPoint + 8) / (start - testEndpoint).Length());
						break;
					}
				}
			}

			return testEndpoint - start;
		}

		private void DrawBalls(SpriteBatch sb, BlendState endState, Color topColor, Color bottomColor, float scale)
        {
			Texture2D ballTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;

			float ballRotation = (laserEndpoint - laserStartpoint).ToRotation();

			sb.End();

			Effect effect = Filters.Scene["BreachLaserBloom"].GetShader().Shader;
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["noiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.25f);
			effect.Parameters["stretch"].SetValue(1);

			effect.Parameters["dilation"].SetValue(0.8f);
			effect.Parameters["falloff"].SetValue(1);

			effect.Parameters["topColor"].SetValue(topColor.ToVector3());
			effect.Parameters["bottomColor"].SetValue(bottomColor.ToVector3());

			sb.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

			sb.Draw(ballTex, laserStartpoint - Main.screenPosition, null, Color.White, ballRotation, ballTex.Size() / 2, scale * laserSizeMult, SpriteEffects.None, 0f);
			sb.Draw(ballTex, laserEndpoint - Main.screenPosition, null, Color.White, ballRotation, ballTex.Size() / 2, scale * laserSizeMult * 0.85f, SpriteEffects.None, 0f);

			if (superLaser)
			{
				float superRotation = (superLaserEndpoint - superLaserStartpoint).ToRotation();
				sb.Draw(ballTex, superLaserStartpoint - Main.screenPosition, null, Color.White, superRotation, ballTex.Size() / 2, scale * superLaserSizeMult, SpriteEffects.None, 0f);
				sb.Draw(ballTex, superLaserEndpoint - Main.screenPosition, null, Color.White, superRotation, ballTex.Size() / 2, scale * 0.85f * superLaserSizeMult, SpriteEffects.None, 0f);
			}


			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, endState, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		public void DrawAdditive(SpriteBatch sb)
		{
			Color blue = new Color(0,0,255);
			Color blueCyan = Color.Lerp(Color.Cyan, blue, 0.5f);
			var tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;

			sb.Draw(tex, laserStartpoint - Main.screenPosition, null, blueCyan, 0, tex.Size() / 2, 0.45f * laserSizeMult, SpriteEffects.None, 0f);
			sb.Draw(tex, laserEndpoint - Main.screenPosition, null, blueCyan, 0, tex.Size() / 2, 0.35f * laserSizeMult, SpriteEffects.None, 0f);

			if (superLaser)
			{
				sb.Draw(tex, superLaserStartpoint - Main.screenPosition, null, blueCyan, 0, tex.Size() / 2, 0.45f * superLaserSizeMult, SpriteEffects.None, 0f);
				sb.Draw(tex, superLaserEndpoint - Main.screenPosition, null, blueCyan, 0, tex.Size() / 2, 0.35f * superLaserSizeMult, SpriteEffects.None, 0f);
			}

			DrawBalls(sb, BlendState.Additive, Color.White, Color.White, 0.35f);
		}

		private void SpawnParticles()
		{
			Vector2 direction = laserEndpoint.DirectionTo(laserStartpoint);

			Color color1 = Color.Cyan;
			Color color2 = new Color(0, 0, 255);

			for (int i = 0; i < 5; i++)
				Dust.NewDustPerfect(laserEndpoint, ModContent.DustType<BreachImpactGlow>(), direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(12), 0, Color.Lerp(color1, color2, Main.rand.NextFloat()), Main.rand.NextFloat(0.25f, 0.6f));

			for (int i = 0; i < 3; i++)
			{
				Vector2 vel = direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(9);
				Dust.NewDustPerfect(laserEndpoint + new Vector2(0, 35) + (vel * 2), ModContent.DustType<BreachImpactSpark>(), vel, 0, Color.Lerp(color1, color2, Main.rand.NextFloat()), Main.rand.NextFloat(1.25f, 1.6f));
			}

			if (superLaser)
            {
				direction = superLaserEndpoint.DirectionTo(superLaserStartpoint);

				for (int i = 0; i < 5; i++)
					Dust.NewDustPerfect(superLaserEndpoint, ModContent.DustType<BreachImpactGlow>(), direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(12), 0, Color.Lerp(color1, color2, Main.rand.NextFloat()), Main.rand.NextFloat(0.25f, 0.6f));

				for (int i = 0; i < 3; i++)
				{
					Vector2 vel = direction.RotatedByRandom(0.6f) * Main.rand.NextFloat(9);
					Dust.NewDustPerfect(superLaserEndpoint + new Vector2(0, 35) + (vel * 2), ModContent.DustType<BreachImpactSpark>(), vel, 0, Color.Lerp(color1, color2, Main.rand.NextFloat()), Main.rand.NextFloat(1.25f, 1.6f));
				}
			}
		}
	}
}
