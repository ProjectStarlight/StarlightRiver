using ReLogic.Content;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Graphics.Effects;
using StarlightRiver.Content.Dusts;

namespace StarlightRiver.Content.Items.Magnet
{
	public class ThunderBeads : ModItem
	{
		public override string Texture => AssetDirectory.MagnetItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magnet Beads");
			Tooltip.SetDefault("Whip enemies to stick the beads to them \nRepeatedly click to shock affected enemies");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToWhip(ModContent.ProjectileType<ThunderBeadsProj>(), 30, 1.2f, 5f, 25);
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Orange;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<ChargedMagnet>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class ThunderBeadsProj : BaseWhip
	{
		public NPC target = default;

		public bool embedded = false;

		public int embedTimer = 150;

		public bool ableToHit = false;
		public bool leftClick = true;

		public float fade = 0.1f;

		public Color baseColor = new(200, 230, 255);
		public Color endColor = Color.Purple;

		private Trail trail;
		private Trail trail2;
		private List<Vector2> cache;
		private List<Vector2> cache2;

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.MagnetItem + Name;

		public ThunderBeadsProj() : base("Thunder Beads", 15, 1.2f, Color.Transparent)
		{
			xFrames = 1;
			yFrames = 5;
		}

		public override void Load()
		{
			On.Terraria.Projectile.FillWhipControlPoints += OverrideWhipControlPoints;
			base.Load();
		}

		public override int SegmentVariant(int segment)
		{
			return 1;
		}

		public override void SafeSetDefaults()
		{
			Projectile.localNPCHitCooldown = 1;
		}

		public override bool PreAI()
		{
			ManageCache();
			if (!Main.dedServ)
				ManageTrails();

			for (int i = 0; i < cache.Count - 1; i++)
			{
				if (i % 3 != 0)
					continue;

				Lighting.AddLight(cache[i], Color.Cyan.ToVector3() * fade * 0.5f);
			}

			if (fade > 0.1f)
				fade -= 0.05f;

			if (embedded)
			{
				if (!leftClick && Main.mouseLeft)
				{
					ableToHit = true;
					leftClick = true;
				}

				if (!Main.mouseLeft)
					leftClick = false;

				flyTime = Owner.itemAnimationMax * Projectile.MaxUpdates;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				Projectile.spriteDirection = (!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : -1;

				Owner.heldProj = Projectile.whoAmI;
				Owner.itemAnimation = Owner.itemAnimationMax - (int)(Projectile.ai[0] / Projectile.MaxUpdates);
				Owner.itemTime = Owner.itemAnimation;

				embedTimer--;
				if (embedTimer < 0 || !target.active)
				{
					Projectile.friendly = false;
					embedded = false;
					return false;
				}

				Projectile.Center = target.Center;
				Projectile.WhipPointsForCollision.Clear();
				SetPoints(Projectile.WhipPointsForCollision);
				return false;
			}
			return base.PreAI();
		}

		public override void ArcAI()
		{
			xFrame = 0;
		}

		public override bool ShouldDrawSegment(int segment)
		{
			return segment % 3 == 0;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			ableToHit = false;
			if (!embedded)
			{
				Projectile.ownerHitCheck = false;
				Projectile.damage /= 2;
				this.target = target;
				embedded = true;
			}
			else
			{
				Helper.PlayPitched("Magic/LightningShortest1", 0.5f, Main.rand.NextFloat(0f, 0.2f), target.Center);
				fade = 1;

				for (int i = 2; i < cache.Count - 2; i++)
				{
					Vector2 vel = cache[i].DirectionTo(Owner.Center).RotatedByRandom(0.2f) * Main.rand.NextFloat(1, 2);
					Dust.NewDustPerfect(cache[i] + Main.rand.NextVector2Circular(6,6), ModContent.DustType<GlowLineFast>(), vel, 0, Color.Cyan, 0.4f);
				}
			}
		}

		public override bool? CanHitNPC(NPC target)
		{ 
			if (embedded)
				return target == this.target && ableToHit;

			return base.CanHitNPC(target);
		}

		public override void SetPoints(List<Vector2> controlPoints)
		{
			if (embedded)
			{
				Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
				for (int i = 0; i < segments + 1; i++)
				{
					float lerper = i / (float)segments;
					controlPoints.Add(Vector2.Lerp(playerArmPosition, target.Center, lerper));
				}
			}
			else
			{
				base.SetPoints(controlPoints);
			}
		}

		public override void DrawBehindWhip(ref Color lightColor)
		{
			DrawPrimitives();
			if (embedded)
			{
				Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "Glow").Value;
				for (int i = 0; i < cache.Count - 1; i++)
				{
					if (i % 3 != 0)
						continue;

					Main.spriteBatch.Draw(bloomTex, cache[i] - Main.screenPosition, null, Color.White * 0.7f, 0, bloomTex.Size() / 2, fade * 0.4f, SpriteEffects.None, 0f);
				}

				Main.spriteBatch.End();
			}
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private void ManageCache()
		{
			cache = new List<Vector2>();
			SetPoints(cache);

			cache2 = new List<Vector2>();
			for (int i = 0; i < cache.Count; i++)
			{
				Vector2 point = cache[i];
				Vector2 endPoint = embedded ? target.Center : cache[i];
				Vector2 nextPoint = i == cache.Count - 1 ? endPoint : cache[i + 1];
				Vector2 dir = Vector2.Normalize(nextPoint - point).RotatedBy(Main.rand.NextBool() ? -1.57f : 1.57f);

				if (i > cache.Count - 3 || dir == Vector2.Zero)
					cache2.Add(point);
				else
					cache2.Add(point + dir * Main.rand.NextFloat(8) * fade);
			}
		}

		private void ManageTrails()
		{
			Vector2 endPoint = embedded ? target.Center : cache[segments];
			trail ??= new Trail(Main.instance.GraphicsDevice, segments + 1, new TriangularTip(4), factor => 16, factor =>
			{
				if (factor.X > 0.99f)
					return Color.Transparent;

				return new Color(160, 220, 255) * fade * 0.1f * EaseFunction.EaseCubicOut.Ease(1 - factor.X);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = endPoint;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, segments + 1, new TriangularTip(4), factor => 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.Lerp(baseColor, endColor, EaseFunction.EaseCubicIn.Ease(1 - progress)) * fade * progress;
			});

			trail2.Positions = cache2.ToArray();
			trail2.NextPosition = endPoint;
		}

		public void DrawPrimitives()
		{
			Main.spriteBatch.End();
			Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail?.Render(effect);
			trail2?.Render(effect);
		}

		private void OverrideWhipControlPoints(On.Terraria.Projectile.orig_FillWhipControlPoints orig, Projectile proj, List<Vector2> controlPoints)
		{
			orig(proj, controlPoints);
			if (proj.ModProjectile is ThunderBeadsProj modProj && modProj.embedded)
			{
				proj.WhipPointsForCollision.Clear();
				modProj.SetPoints(proj.WhipPointsForCollision);
			}
		}
	}
}