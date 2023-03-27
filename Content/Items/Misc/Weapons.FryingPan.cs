using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class FryingPan : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Frying Pan");
			Tooltip.SetDefault("Attacks in close-range melee before being thrown to ring some chrome domes\n" +
			"'Evil-looking runes are inscribed on the bottom'");
		}

		public override void SetDefaults()
		{
			Item.damage = 12;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 20;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6.5f;
			Item.crit = 9;
			Item.shootSpeed = 14f;
			Item.autoReuse = false;
			Item.shoot = ProjectileType<FryingPanProj>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(RecipeGroupID.IronBar, 15)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	internal class FryingPanProj : ModProjectile
	{
		enum PanAttack : int
		{
			Down = 0,
			FirstUp = 1,
			Spin = 2,
			SecondUp = 3,
			Crit = 4,
			Reset = 5
		}

		private PanAttack currentAttack = PanAttack.Down;

		private bool initialized = false;

		private int attackDuration = 0;

		private float startRotation = 0f;

		private float endRotation = 0f;

		private bool facingRight;

		private readonly float zRotation = 0;

		private float rotVel = 0f;

		private int growCounter = 0;

		private Trail trail;
		private List<Vector2> cache;

		private List<float> oldRotation = new();
		private List<Vector2> oldPosition = new();

		private List<NPC> hit = new();

		public override string Texture => AssetDirectory.MiscItem + "FryingPan";

		Player Owner => Main.player[Projectile.owner];

		private bool FirstTickOfSwing => Projectile.ai[0] == 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Frying Pan");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Main.projFrames[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(42, 42);
			Projectile.penetrate = -1;
			Projectile.ownerHitCheck = true;
			Projectile.extraUpdates = 3;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = Main.GetPlayerArmPosition(Projectile);

			if (currentAttack == PanAttack.Spin)
			{
				Vector2 spinOffset = Main.GetPlayerArmPosition(Projectile) - Owner.Center;
				spinOffset.X *= (float)Math.Cos(zRotation);
				Projectile.Center = Owner.Center + spinOffset;
			}

			Owner.heldProj = Projectile.whoAmI;

			if (FirstTickOfSwing)
			{
				hit = new List<NPC>();

				if (Owner.DirectionTo(Main.MouseWorld).X > 0)
					facingRight = true;
				else
					facingRight = false;

				float rot = Owner.DirectionTo(Main.MouseWorld).ToRotation();

				if (!initialized)
				{
					initialized = true;
					endRotation = rot - 1f * Owner.direction;

					oldRotation = new List<float>();
					oldPosition = new List<Vector2>();

				}
				else
				{
					currentAttack = (PanAttack)((int)currentAttack + 1);
				}

				if (currentAttack == PanAttack.Crit)
					Helper.PlayPitched("Effects/HeavyWhoosh", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
				else
					Helper.PlayPitched("Effects/HeavyWhooshShort", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);

				startRotation = endRotation;

				switch (currentAttack)
				{
					case PanAttack.Down:
						endRotation = rot + 2f * Owner.direction;
						attackDuration = 120;
						break;

					case PanAttack.FirstUp:
						endRotation = rot - 2f * Owner.direction;
						attackDuration = 120;
						break;

					case PanAttack.Spin:
						attackDuration = 140;
						endRotation = rot + 2f * Owner.direction;
						break;

					case PanAttack.SecondUp:
						endRotation = rot - 2f * Owner.direction;
						attackDuration = 120;
						break;

					case PanAttack.Crit:
						attackDuration = 180;
						endRotation = rot + 10f * Owner.direction;
						Projectile.ai[0] -= 15f / attackDuration;
						break;

					case PanAttack.Reset:
						Projectile.active = false;
						break;
				}

				Projectile.ai[0] += 30f / attackDuration;
			}

			if (Projectile.ai[0] < 1)
			{
				Projectile.timeLeft = 50;
				Projectile.ai[0] += 1f / attackDuration;
				rotVel = Math.Abs(EaseProgress(Projectile.ai[0]) - EaseProgress(Projectile.ai[0] - 1f / attackDuration)) * 2;
			}
			else
			{
				rotVel = 0f;
				if (Main.mouseLeft)
				{
					Projectile.ai[0] = 0;
					return;
				}
			}
			/*if (currentAttack == CurrentAttack.Spin && Projectile.ai[0] < 1)
			{
				zRotation = 6.28f * EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]);
				owner.UpdateRotation(zRotation + (facingRight ? 3.14f : 0));
			}
			else
				owner.UpdateRotation(0);*/

			float progress = EaseProgress(Projectile.ai[0]);

			Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + rotVel * 4), 1.3f);

			Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);

			Owner.ChangeDir(facingRight ? 1 : -1);

			float wrappedRotation = MathHelper.WrapAngle(Projectile.rotation);

			if (facingRight)
				Owner.itemRotation = MathHelper.Clamp(wrappedRotation, -1.57f, 1.57f);
			else if (wrappedRotation > 0)
				Owner.itemRotation = MathHelper.Clamp(wrappedRotation, 1.57f, 4.71f);
			else
				Owner.itemRotation = MathHelper.Clamp(wrappedRotation, -1.57f, -4.71f);
			Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation - (facingRight ? 0 : MathHelper.Pi));
			Owner.itemAnimation = Owner.itemTime = 5;

			float throwingAngle = MathHelper.WrapAngle(Owner.DirectionTo(Main.MouseWorld).ToRotation());

			if (currentAttack == PanAttack.Crit && Math.Abs(wrappedRotation - throwingAngle) < 0.3f && Projectile.ai[0] > 0.6f)
			{
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Owner.DirectionTo(Main.MouseWorld) * 12, ModContent.ProjectileType<FryingPanThrownProj>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI).rotation = Projectile.rotation + 0.78f;
				Projectile.active = false;
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			oldRotation.Add(Projectile.rotation);
			oldPosition.Add(Projectile.Center);

			if (oldRotation.Count > 16)
				oldRotation.RemoveAt(0);
			if (oldPosition.Count > 16)
				oldPosition.RemoveAt(0);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (rotVel < 0.005f)
				return false;

			float collisionPoint = 0f;

			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + 42 * Projectile.rotation.ToRotationVector2(), 20, ref collisionPoint))
				return true;

			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (hit.Contains(target))
				return false;

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			hit.Add(target);
			Helper.PlayPitched("Impacts/PanBonkSmall", 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), target.Center);
			CameraSystem.shake += 2;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = Math.Sign(target.Center.X - Owner.Center.X);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			//DrawTrail(Main.spriteBatch);
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			bool flip = false;
			SpriteEffects effects = SpriteEffects.None;
			/*if (zRotation > 1.57f && zRotation < 4.71f)
			{
				flip = true;
				effects = facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically;
			}*/

			var origin = new Vector2(0, tex.Height);

			Vector2 scaleVec = Vector2.One;
			/*if (flip)
            {
				if (facingRight)
				{
					scaleVec.X = (float)Math.Abs(Math.Cos(zRotation));
					origin = new Vector2(tex.Width, tex.Height);
				}
				else
				{
					scaleVec.Y = (float)Math.Abs(Math.Cos(zRotation));
					origin = new Vector2(0, 0);
				}
            }*/
			for (int k = 16; k > 0; k--)
			{

				float progress = 1 - (float)((16 - k) / (float)16);
				Color color = lightColor * EaseFunction.EaseQuarticOut.Ease(progress) * 0.1f;
				if (k > 0 && k < oldRotation.Count)
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, oldRotation[k] + 0.78f, origin, Projectile.scale * scaleVec, effects, 0f);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, Projectile.scale * scaleVec, effects, 0f);
			return false;
		}

		private void ManageCaches()
		{
			Vector2 off = Projectile.rotation.ToRotationVector2() * 35;
			off.X *= (float)Math.Cos(zRotation);

			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 60; i++)
				{
					cache.Add(Projectile.Center + off);
				}
			}

			cache.Add(Projectile.Center + off);

			while (cache.Count > 60)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			Vector2 off = (Projectile.rotation + rotVel).ToRotationVector2() * 35;
			off.X *= (float)Math.Cos(zRotation);

			trail ??= new Trail(Main.instance.GraphicsDevice, 60, new TriangularTip(4), factor => 12, factor =>
			{
				Color trailColor = Color.DarkGray * MathHelper.Min(rotVel * 18, 0.75f);
				return trailColor;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + off;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CoachBombTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value);

			trail?.Render(effect);

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private float EaseProgress(float input)
		{
			return currentAttack switch
			{
				PanAttack.Down => EaseFunction.EaseCircularInOut.Ease(input),
				PanAttack.FirstUp => EaseFunction.EaseCircularInOut.Ease(input),
				PanAttack.Spin => EaseFunction.EaseCircularInOut.Ease(input),
				PanAttack.SecondUp => EaseFunction.EaseCircularInOut.Ease(input),
				PanAttack.Crit => EaseFunction.EaseQuadInOut.Ease(input),
				_ => input,
			};
		}
	}

	internal class FryingPanThrownProj : ModProjectile
	{
		private List<float> oldRotation = new();
		private List<Vector2> oldPosition = new();
		private bool initialized = false;

		public override string Texture => AssetDirectory.MiscItem + "FryingPan";

		private Player Owner => Main.player[Projectile.owner];

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.aiStyle = 3;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 700;
			Projectile.extraUpdates = 1;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			if (!initialized)
			{
				oldRotation = new List<float>();
				oldPosition = new List<Vector2>();
				initialized = true;
			}

			Owner.itemTime = Owner.itemAnimation = 5;
			Owner.itemRotation = Owner.DirectionTo(Projectile.Center).ToRotation();
			Owner.itemRotation = MathHelper.WrapAngle(Owner.itemRotation - ((Projectile.Center.X > Owner.Center.X) ? 0 : MathHelper.Pi));

			oldRotation.Add(Projectile.rotation);
			oldPosition.Add(Projectile.Center);

			if (oldRotation.Count > 8)
				oldRotation.RemoveAt(0);

			if (oldPosition.Count > 8)
				oldPosition.RemoveAt(0);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			SpriteEffects effects = SpriteEffects.None;

			Vector2 origin = new Vector2(tex.Width, tex.Height) / 2;

			for (int k = 8; k > 0; k--)
			{
				float progress = 1 - (float)((8 - k) / (float)8);
				Color color = lightColor * EaseFunction.EaseQuarticOut.Ease(progress) * 0.2f;

				if (k > 0 && k < oldRotation.Count)
					Main.spriteBatch.Draw(tex, oldPosition[k] - Main.screenPosition, null, color, oldRotation[k], origin, Projectile.scale, effects, 0f);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);

			return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Bonk(target.Center);
			/*Dust.NewDustPerfect(target.Center - new Vector2(54, 47), ModContent.DustType<FryingPanBonkBG>(), dir);
			Dust.NewDustPerfect(target.Center - new Vector2(46, 23), ModContent.DustType<FryingPanBonk>(), dir);*/
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Bonk(Projectile.Center);
			return base.OnTileCollide(oldVelocity);
		}

		private void Bonk(Vector2 position)
		{
			Projectile.friendly = false;
			Helper.PlayPitched("Impacts/PanBonkBig", 0.7f, Main.rand.NextFloat(-0.2f, 0.2f), position);

			CameraSystem.shake += 10;
			for (int j = 0; j < 17; j++)
			{
				Vector2 direction = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Dust.NewDustPerfect(position + direction * 20 + new Vector2(0, 40), DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(2, 10), 0, new Color(255, 255, 60) * 0.8f, 1.6f);
			}

			Vector2 dir = -Vector2.UnitY.RotatedByRandom(0.3f) * 6;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, dir, ProjectileType<FryingPanBonk>(), 0, 0, Owner.whoAmI);
		}
	}

	internal class FryingPanBonk : ModProjectile
	{
		float bgRotation = 0;

		public override string Texture => AssetDirectory.MiscItem + "FryingPanBonk";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Frying Pan");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(32, 32);
			Projectile.penetrate = -1;
			Projectile.timeLeft = 90;
		}

		public override void AI()
		{
			Projectile.alpha++;

			Projectile.velocity *= 0.92f;

			Projectile.rotation -= 0.002f;
			bgRotation += 0.002f;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float colorMult = Math.Min(1, Math.Min((90 - Projectile.alpha) / 15f, Projectile.alpha / 15f));
			Color bgColor = Color.White * colorMult;
			Texture2D bgTex = Request<Texture2D>(Texture + "_BG").Value;
			Texture2D tex = Request<Texture2D>(Texture).Value;

			Color color = new Color(255, 255, 255, 0) * colorMult;

			Main.spriteBatch.Draw(bgTex, Projectile.Center - Main.screenPosition, null, bgColor, bgRotation, bgTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, bgColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}