using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class RebarLauncher : ModItem
	{
		Vector2 direction = Vector2.Zero;

		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rebar Launcher");
			Tooltip.SetDefault("Impales enemies \nShoot impaled rebar to drive it even deeper into enemies");
		}

		public override void SetDefaults() //TODO: Adjust rarity sellprice and balance
		{
			Item.damage = 40;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 65;
			Item.useAnimation = 65;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.shoot = ModContent.ProjectileType<RebarProj>();
			Item.shootSpeed = 25f;
			Item.autoReuse = true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			Helper.PlayPitched("Guns/RebarLauncher", 0.6f, 0);
			CameraSystem.shake += 6;
			direction = velocity;
			position += velocity * 0.9f;
		}

		public override void HoldItem(Player player)
		{
			if (player.itemTime > 1)
				Dust.NewDustPerfect(player.Center + direction.RotatedBy(-0.1f * player.direction) * 0.75f, ModContent.DustType<BuzzsawSteam>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), Main.rand.Next(30), Color.White, Main.rand.NextFloat(0.2f, 0.5f));

			base.HoldItem(player);
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-20, 0);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HellstoneBar, 12);
			recipe.AddIngredient(ModContent.ItemType<AncientGear>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class RebarProj : ModProjectile, IDrawPrimitive
	{
		const int TRAIL_LENGTH = 150;

		private List<Vector2> cache;
		private List<Vector2> cache2;
		private Trail trail;
		private Trail trail2;

		private List<Vector2> offsetCache;
		private List<Vector2> offsetCache2;

		float trailWidth = 4;

		private Vector2 initialVel;

		private int initialDamage;

		private int cooldown;

		private bool collided = false;

		int enemyID = -1;
		bool stuck = false;
		Vector2 offset = Vector2.Zero;

		public List<Projectile> collidedWith = new();

		public float distanceIn = 0;

		public Vector2 A => Projectile.Center + (Projectile.rotation + 0.3f).ToRotationVector2() * 40; //TODO: These need better names
		public Vector2 B => Projectile.Center - (Projectile.rotation + 0.3f).ToRotationVector2() * 40;
		public Vector2 A2 => Projectile.Center + (Projectile.rotation - 0.3f).ToRotationVector2() * 40;
		public Vector2 B2 => Projectile.Center - (Projectile.rotation - 0.3f).ToRotationVector2() * 40;
		public Vector2 A3 => Projectile.Center + Projectile.rotation.ToRotationVector2() * 30;
		public Vector2 B3 => Projectile.Center - Projectile.rotation.ToRotationVector2() * 30;
		public Vector2 A4 => Projectile.Center + Projectile.rotation.ToRotationVector2() * 35;
		public Vector2 B4 => Projectile.Center + Projectile.rotation.ToRotationVector2() * 35;
		public Vector2 Top => (Projectile.rotation - 1.57f).ToRotationVector2() * 5;

		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.SteampunkItem + "RebarProj");
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rebar");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 150;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.width = Projectile.height = 16;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.extraUpdates = 5;
		}

		public override bool PreAI()
		{
			if (collided || stuck || Projectile.timeLeft % 6 == 0)
			{
				if (trailWidth > 0.3f)
				{
					trailWidth *= 0.995f;

					if (trailWidth < 3.5f)
						trailWidth *= 0.95f;
				}
				else
				{
					trailWidth = 0;
				}
			}

			if (!collided)
			{
				if (stuck)
				{
					NPC target = Main.npc[enemyID];
					cooldown--;

					if (!target.active)
					{
						if (Projectile.timeLeft > 5)
							Projectile.timeLeft = 5;

						Projectile.velocity = Vector2.Zero;
					}
					else
					{
						Projectile collidingProj = Main.projectile.Where(n => n.active && n != Projectile && !collidedWith.Contains(n) && n.ModProjectile is RebarProj modProj && Collision.CheckLinevLine(A, B, modProj.A2, modProj.B2).Length > 0 && !modProj.collidedWith.Contains(Projectile) && modProj.distanceIn < distanceIn).OrderBy(n => n.velocity.Length()).FirstOrDefault();

						if (collidingProj != default)
						{
							float angleBetween = Math.Abs(((collidingProj.rotation - Projectile.rotation) % 6.28f + 9.42f) % 6.28f - 3.14f);

							float effectiveness = (3.14f - angleBetween) / 3.14f;

							if (effectiveness >= 0.5f)
							{
								var mp = collidingProj.ModProjectile as RebarProj;
								mp.collidedWith.Add(collidingProj);

								Vector2 vel = initialVel;
								vel.Normalize();
								vel *= effectiveness;
								vel *= 4;
								collidedWith.Clear();
								if (cooldown < 0)
								{
									Projectile.friendly = true;
								}

								Vector2[] dustPositions = Collision.CheckLinevLine(A, B, mp.A2, mp.B2);

								if (dustPositions.Length > 0 && initialVel.Length() < 1)
								{
									for (float i = 0; i < 15; i++)
										Dust.NewDustPerfect(dustPositions[0], ModContent.DustType<RebarLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 6), 0, new Color(248, 126, 0), Main.rand.NextFloat(0.35f, 0.5f));
								}

								initialVel += vel;

								Projectile.damage = initialDamage + (int)Math.Pow(distanceIn, 0.5f);
							}
						}

						offset += initialVel;
						distanceIn += initialVel.Length();

						if (initialVel.Length() > 0.02f)
							initialVel *= 0.5f;

						NPC targetCollection = Main.npc.Where(n => n.active && !n.townNPC && (Collision.CheckAABBvLineCollision(n.position, n.Size, A3, B3) || Collision.CheckAABBvAABBCollision(Projectile.position, Projectile.Size, n.position, n.Size))).OrderBy(n => 1).FirstOrDefault();
						if (targetCollection == default)
						{
							Projectile.extraUpdates = 0;

							if (Projectile.timeLeft > 80)
								Projectile.timeLeft = 80;

							Projectile.friendly = false;
							Projectile.velocity = Vector2.Zero;

							if (!collided)
							{
								Projectile.alpha = 255;
								Gore.NewGorePerfect(Projectile.GetSource_FromThis(), Projectile.Center, initialVel, Mod.Find<ModGore>("RebarProj").Type);
								collided = true;
							}
						}

						Projectile.position = target.position + offset;
					}
				}

				else
				{
					Projectile.rotation = Projectile.velocity.ToRotation();
					ManageCaches();
				}
			}
			else
			{
				if (enemyID != -1)
				{
					NPC target = Main.npc[enemyID];
					Projectile.position = target.position + offset;
				}

				Projectile.alpha += 4;
			}

			ManageTrail();
			return true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), A3, B3);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.penetrate++;

			if (target.life > 0 && enemyID != target.whoAmI)
			{
				enemyID = target.whoAmI;
				offset = Projectile.position - target.position;
				Projectile.extraUpdates = 0;
			}

			if (stuck)
			{
				cooldown = 30;
				Projectile.friendly = false;
			}

			if (!stuck && target.life > 0)
			{
				Projectile.extraUpdates = 0;
				initialDamage = Projectile.damage;
				initialVel = Projectile.velocity;
				stuck = true;
				Projectile.friendly = false;
				Projectile.tileCollide = false;
				offset -= Projectile.velocity;
				Projectile.timeLeft = 800;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D overlay = ModContent.Request<Texture2D>(Texture + "_White").Value;
			Texture2D glow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Color color = HeatColor(trailWidth / 4f, 0.5f);
			color.A = 0;

			float transparency = 1 - Projectile.alpha / 255f;

			spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, color * transparency, Projectile.rotation, glow.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
			spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, lightColor * transparency, Projectile.rotation, new Vector2(30, 3), Projectile.scale, SpriteEffects.None, 0);
			spriteBatch.Draw(overlay, Projectile.Center - Main.screenPosition, null, HeatColor(trailWidth / 4f, 0.5f) * transparency, Projectile.rotation, new Vector2(30, 3), Projectile.scale, SpriteEffects.None, 0);
			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.extraUpdates = 0;

			if (Projectile.timeLeft > 80)
				Projectile.timeLeft = 80;

			Projectile.friendly = false;
			Projectile.velocity = Vector2.Zero;
			collided = true;
			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				offsetCache = new List<Vector2>();

				for (int i = 0; i < TRAIL_LENGTH; i++)
				{
					cache.Add(A4 + Top);

					offsetCache.Add(Vector2.Zero);
				}
			}

			cache.Add(A4 + Top);
			offsetCache.Add((Projectile.rotation + 1.57f).ToRotationVector2() * Main.rand.NextFloat(-0.3f, 0.3f));

			while (cache.Count > TRAIL_LENGTH)
			{
				cache.RemoveAt(0);
				offsetCache.RemoveAt(0);
			}

			if (cache2 == null)
			{
				cache2 = new List<Vector2>();
				offsetCache2 = new List<Vector2>();
				for (int i = 0; i < TRAIL_LENGTH; i++)
				{
					cache2.Add(A4 + Top);
					offsetCache2.Add(Vector2.Zero);
				}
			}

			cache2.Add(A4 + Top);
			offsetCache2.Add((Projectile.rotation - 1.57f).ToRotationVector2() * Main.rand.NextFloat(-0.3f, 0.3f));

			while (cache2.Count > TRAIL_LENGTH)
			{
				cache2.RemoveAt(0);
				offsetCache2.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, TRAIL_LENGTH, new TriangularTip(40), factor => factor * trailWidth * 4f, factor => Color.Red);

			trail2 ??= new Trail(Main.instance.GraphicsDevice, TRAIL_LENGTH, new TriangularTip(40), factor => factor * trailWidth * 4f, factor => Color.Red);

			if (trailWidth < 3.9f && (collided || stuck || Projectile.timeLeft % 6 == 0))
			{
				for (int i = 0; i < cache.Count; i++)
				{
					cache[i] += offsetCache[i] * 2f;
				}

				for (int i = 0; i < cache2.Count; i++)
				{
					cache2[i] += offsetCache2[i];
				}
			}

			trail.Positions = cache.ToArray();

			trail2.Positions = cache2.ToArray();

			if (!stuck)
			{
				trail.NextPosition = A4 + Projectile.rotation.ToRotationVector2() * 25 + Top;
				trail2.NextPosition = A4 + Projectile.rotation.ToRotationVector2() * 25 + Top;
			}
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["RebarTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "RebarTrailTexture").Value);
			effect.Parameters["noiseTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "RebarNoiseTexture").Value);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["progress"].SetValue(trailWidth / 4f);
			effect.Parameters["repeats"].SetValue(18);
			effect.Parameters["midColor"].SetValue(new Color(248, 126, 0).ToVector3());

			trail?.Render(effect);
			trail2?.Render(effect);
		}

		private Color HeatColor(float progress, float midpoint)
		{
			var orange = new Color(248, 126, 0);

			if (progress > midpoint)
				return Color.Lerp(Color.Red, orange, (progress - midpoint) / (1 - midpoint));
			else
				return Color.Lerp(Color.Red, new Color(0, 0, 0, 0), (midpoint - progress) / midpoint);
		}
	}
}