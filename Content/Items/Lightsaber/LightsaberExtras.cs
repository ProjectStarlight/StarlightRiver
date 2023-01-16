using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Lightsaber
{

	public class YellowLightsaberDashProjectile : ModProjectile
	{
		private Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lightsaber");
		}

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 120;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.Center = Owner.Center;

			if (!Owner.GetModPlayer<LightsaberPlayer>().dashing)
				Projectile.active = false;
		}
	}

	class BlueLightsaberLensFlare : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.Keys + "Glow";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Laser");
		}

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 60;
			Projectile.tileCollide = false;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 4;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			float scale = MathHelper.Min(1 - (60 - Projectile.timeLeft) / 60f, 1);

			for (int k = 0; k < 9; k++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), 0, tex.Size() / 2, Projectile.scale * scale * 0.7f, SpriteEffects.None, 0f);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, Projectile.scale * scale * 0.5f, SpriteEffects.None, 0f);
			}
		}
	}

	class BlueLightsaberLaser : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Laser");
		}

		private bool initialized = false;

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 1060;
			Projectile.tileCollide = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, new Vector3(0, 0.1f, 0.255f));
			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

			if (!initialized)
			{
				initialized = true;
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlueLightsaberLensFlare>(), 0, 0, Projectile.owner);
			}
		}

		public override void Kill(int timeLeft)
		{

		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * new Vector2(1, 0.6f) * 1.5f, SpriteEffects.None, 0f);

			for (int i = 0; i < 5; i++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * new Vector2(1, 0.9f), SpriteEffects.None, 0f);
			}

			for (int k = 0; k < 9; k++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale * 0.9f * new Vector2(1, 0.9f), SpriteEffects.None, 0f);
			}

			for (int l = 0; l < 2; l++)
			{
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(0, 0.1f, 0.255f), Projectile.rotation, tex.Size() / 2, Projectile.scale * 2f * new Vector2(1, 0.9f), SpriteEffects.None, 0f);
			}
		}
	}

	class GreenLightsaberShockwave : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		private int TileType => (int)Projectile.ai[0];
		private int ShockwavesLeft => (int)Projectile.ai[1];//Positive and Negitive

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shockwave");
		}

		private bool createdLight = false;

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 1060;
			Projectile.tileCollide = true;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.idStaticNPCHitCooldown = 20;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.extraUpdates = 5;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			if (Projectile.timeLeft > 1000)
			{
				if (Projectile.timeLeft < 1002 && Projectile.timeLeft > 80)
					Projectile.Kill();

				Projectile.velocity.Y = 4f;
			}
			else
			{
				Projectile.velocity.Y = Projectile.timeLeft <= 10 ? 1f : -1f;

				if (Projectile.timeLeft == 19 && Math.Abs(ShockwavesLeft) > 0)
				{
					var proj = Projectile.NewProjectileDirect(Terraria.Entity.InheritSource(Projectile), new Vector2((int)Projectile.Center.X / 16 * 16 + 16 * Math.Sign(ShockwavesLeft)
					, (int)Projectile.Center.Y / 16 * 16 - 32),
					Vector2.Zero, Projectile.type, Projectile.damage, 0, Main.myPlayer, TileType, Projectile.ai[1] - Math.Sign(ShockwavesLeft));
					proj.extraUpdates = (int)(Math.Abs(ShockwavesLeft) / 3f);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft < 21)
				Main.spriteBatch.Draw(TextureAssets.Tile[TileType].Value, Projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.timeLeft > 800)
			{
				var point = new Point16((int)((Projectile.Center.X + Projectile.width / 3f * Projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)(Projectile.Center.Y / 16) + 1));
				Tile tile = Framing.GetTileSafely(point.X, point.Y);

				if (!createdLight)
				{
					createdLight = true;
					Dust.NewDustPerfect(point.ToVector2() * 16, ModContent.DustType<LightsaberLight>(), Vector2.Zero, 0, Color.Green, 1);
				}

				if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.HasTile && Main.tileSolid[tile.TileType])
				{
					Projectile.timeLeft = 20;
					Projectile.ai[0] = tile.TileType;
					Projectile.tileCollide = false;
					Projectile.position.Y += 16;

					for (float num315 = 0.50f; num315 < 3; num315 += 0.25f)
					{
						float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
						Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 2f;
						int dustID = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, (int)(Projectile.height / 2f), ModContent.DustType<LightsaberGlow>(), 0f, 0f, 50, Color.Green, Main.rand.NextFloat(0.45f, 0.95f));
						Main.dust[dustID].velocity = vecangle;
					}
				}
			}

			return false;
		}
	}

	public class LightsaberGProj : GlobalProjectile
	{
		public Entity parent = default;

		public override bool InstancePerEntity => true;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_Parent spawnSource)
				parent = spawnSource.Entity;
		}
	}

	public class LightsaberPlayer : ModPlayer
	{
		public int whiteCooldown = -1;
		public bool dashing = false;

		public bool jumping = false;
		public Vector2 jumpVelocity = Vector2.Zero;

		public float storedBodyRotation = 0f;

		public override void ResetEffects()
		{
			if (whiteCooldown > 1 || Player.itemAnimation == 0)
				whiteCooldown--;
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
		{
			if (dashing)
				return false;

			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
		}

		public override void PreUpdate()
		{
			if (dashing || jumping)
				Player.maxFallSpeed = 2000f;

			if (whiteCooldown == 0)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Player.Center);
				var dust = Dust.NewDustPerfect(Player.Center, ModContent.DustType<LightsaberStar>(), Vector2.Zero, 0, new Color(200, 200, 255, 0), 0.3f);
				dust.customData = Player.whoAmI;
			}
		}

		public override void PostUpdate()
		{
			if (jumping)
			{
				Player.mount?.Dismount(Player);
				storedBodyRotation += 0.3f * Player.direction;
				Player.fullRotation = storedBodyRotation;
				Player.fullRotationOrigin = Player.Size / 2;
			}

			if (Player.velocity.X == 0 || Player.velocity.Y == 0)
				dashing = false;

			if (Player.velocity.Y == 0)
			{
				storedBodyRotation = 0;
				Player.fullRotation = 0;
				jumping = false;
			}
			else
			{
				jumpVelocity = Player.velocity;
			}
		}
	}

	public class LightsaberGlow : Dusts.Glow
	{
		public override bool Update(Dust dust)
		{
			dust.scale *= 0.95f;
			dust.velocity *= 0.98f;
			dust.color *= 1.05f;
			return base.Update(dust);
		}
	}

	public class LightsaberGlowSoft : LightsaberGlow
	{
		public override string Texture => AssetDirectory.Keys + "GlowVerySoft";

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.95f;
			dust.velocity = Vector2.Zero;
			return base.Update(dust);
		}
	}

	public class LightsaberLight : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.96f;
			if (dust.scale < 0.05f)
				dust.active = false;
			Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 2);
			return false;
		}
	}

	public class LightsaberStar : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Keys/GlowStar";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 74, 74);
			dust.noLight = true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * (1 - dust.alpha / 255f);
		}

		public override bool Update(Dust dust)
		{
			Player owner = Main.player[(int)dust.customData];

			dust.position = owner.position + new Vector2(9 * owner.direction, 19);

			dust.alpha += 10;

			if (!dust.noLight)
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.2f);

			if (dust.alpha > 255)
				dust.active = false;
			return false;
		}
	}

	public class LightsaberImpactRing : ModProjectile
	{
		public Color outerColor = Color.Orange;
		public int ringWidth = 28;
		public bool additive = false;

		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		public int timeLeftStart = 10;

		private float Progress => 1 - Projectile.timeLeft / (float)timeLeftStart;

		private float Radius => Projectile.ai[0] * (float)Math.Sqrt(Math.Sqrt(Progress));

		public override string Texture => AssetDirectory.Assets + "Invisible";

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = timeLeftStart;
			Projectile.extraUpdates = 1;
			Projectile.hide = true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lightsaber");
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
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

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			DrawPrimitives();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			float radius = Radius;

			for (int i = 0; i < 33; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = i / 32f * 6.28f;
				var offset = new Vector2((float)Math.Sin(rad) * 0.4f, (float)Math.Cos(rad));
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

			trail ??= new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => ringWidth * (1 - Progress), factor => outerColor);

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 33, new TriangularTip(1), factor => ringWidth * 0.36f * (1 - Progress), factor => Color.White);
			float nextplace = 33f / 32f;
			var offset = new Vector2((float)Math.Sin(nextplace), (float)Math.Cos(nextplace));
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + offset;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			BlendState oldState = Main.graphics.GraphicsDevice.BlendState;
			if (additive)
				Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;
			trail?.Render(effect);
			trail2?.Render(effect);

			Main.graphics.GraphicsDevice.BlendState = oldState;
		}
	}
}