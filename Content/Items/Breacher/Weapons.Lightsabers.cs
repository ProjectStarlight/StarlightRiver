using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Breacher
{
	public class Lightsaber_Blue : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lightsaber");
			Tooltip.SetDefault("[ph]");
		}

		public override void SetDefaults()
		{
			Item.damage = 22;
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
			Item.shoot = ProjectileType<LightsaberProj_Blue>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.rare = ItemRarityID.Blue;
		}
    }

	enum CurrentAttack : int
	{
		Slash1 = 0,
		Slash2 = 1,
		Slash3 = 2,
		Slash4 = 3,
		Throw = 4,
		Reset = 5
	}

	public class LightsaberProj_Blue : ModProjectile
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		private CurrentAttack currentAttack = CurrentAttack.Slash1;

		private List<Vector2> cache;
		private Trail trail;

		private List<Vector2> cache2;
		private Trail trail2;

		private List<float> oldSquish = new List<float>();

		private bool initialized = false;

		private int attackDuration = 0;

		private float startRotation = 0f;

		private float midRotation = 0f;

		private float endRotation = 0f;

		private bool facingRight;

		private float squish = 1;

		private float startSquish = 1;

		private float endSquish = 1;

		private float rotVel = 0f;

		private int growCounter = 0;

		private List<float> oldRotation = new List<float>();
		private List<Vector2> oldPosition = new List<Vector2>();

		private List<NPC> hit = new List<NPC>();

		Player owner => Main.player[Projectile.owner];

		private bool FirstTickOfSwing
		{
			get => Projectile.ai[0] == 0;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lightsaber");
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
			Projectile.extraUpdates = 4;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.heldProj = Projectile.whoAmI;

			if (FirstTickOfSwing)
			{
				hit = new List<NPC>();
				if (owner.DirectionTo(Main.MouseWorld).X > 0)
					facingRight = true;
				else
					facingRight = false;

				float rot = owner.DirectionTo(Main.MouseWorld).ToRotation();
				if (!initialized)
				{
					initialized = true;
					endRotation = rot - (1f * owner.direction);

					oldRotation = new List<float>();
					oldPosition = new List<Vector2>();

				}
				else
				{
					currentAttack = (CurrentAttack)((int)currentAttack + 1);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f,0.1f)}, owner.Center);

				startRotation = endRotation;
				startSquish = endSquish;
				midRotation = rot;

				switch (currentAttack)
				{
					case CurrentAttack.Slash1:
						endSquish = 0.4f;
						endRotation = rot + (2f * owner.direction);
						attackDuration = 100;
						break;
					case CurrentAttack.Slash2:
						endSquish = 0.3f;
						attackDuration = 120;
						endRotation = rot - (1f * owner.direction);
						break;
					case CurrentAttack.Slash3:
						endSquish = 0.4f;
						endRotation = rot + (2f * owner.direction);
						attackDuration = 100;
						break;
					case CurrentAttack.Slash4:
						endSquish = 0.1f;
						endRotation = rot - (3f * owner.direction);
						attackDuration = 100;
						break;
					case CurrentAttack.Throw:
						endSquish = 0.6f;
						attackDuration = 200;
						endRotation = rot + (10f * owner.direction);
						break;
					case CurrentAttack.Reset:
						endSquish = 0.6f;
						Projectile.active = false;
						break;
				}
				Projectile.ai[0] += 30f / attackDuration;
			}

			if (Projectile.ai[0] < 1)
			{
				Projectile.timeLeft = 50;
				Projectile.ai[0] += 1f / attackDuration;
				rotVel = Math.Abs(EaseProgress(Projectile.ai[0]) - EaseProgress(Projectile.ai[0] - (1f / attackDuration))) * 2;
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

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			float progress = EaseProgress(Projectile.ai[0]);

			Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + (rotVel * 4)), 1.3f);

			Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);
			squish = MathHelper.Lerp(startSquish, endSquish, progress) + (0.35f * (float)Math.Sin(3.14f * progress));

			float wrappedRotation = MathHelper.WrapAngle(Projectile.rotation);

			float throwingAngle = MathHelper.WrapAngle(owner.DirectionTo(Main.MouseWorld).ToRotation());

			/*if (currentAttack == CurrentAttack.Throw && Math.Abs(wrappedRotation - throwingAngle) < 0.2f && Projectile.ai[0] > 0.4f)
			{
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, owner.DirectionTo(Main.MouseWorld) * 12, ModContent.ProjectileType<Misc.FryingPanThrownProj>(), Projectile.damage, Projectile.knockBack, owner.whoAmI).rotation = Projectile.rotation + 0.78f;
				Projectile.active = false;
			}*/

			owner.ChangeDir(facingRight ? 1 : -1);

			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.itemAnimation = owner.itemTime = 5;

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
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + (42 * Projectile.rotation.ToRotationVector2()), 20, ref collisionPoint))
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
			Core.Systems.CameraSystem.Shake += 2;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = Math.Sign(target.Center.X - owner.Center.X);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 scaleVec = new Vector2(1, squish) * 4;
            Effect effect = Filters.Scene["3DSwing"].GetShader().Shader;
            effect.Parameters["rotation"].SetValue(Projectile.rotation - midRotation);
            effect.Parameters["pommelToOriginPercent"].SetValue(0.05f);
            effect.Parameters["color"].SetValue(Color.White.ToVector4());
            Main.spriteBatch.End();
            DrawPrimitives();
            Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, midRotation, tex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

		private void ManageCaches()
		{

			if (cache == null)
			{
				cache = new List<Vector2>();
				oldSquish = new List<float>();
				for (int i = 0; i < 20; i++)
				{
					cache.Add(TrailPosAtProgressScuffed());
					oldSquish.Add((float)Math.Sqrt(1 + (squish * squish)));
				}
				oldSquish.Add((float)Math.Sqrt(1 + (squish * squish)));
			}

			if (cache2 == null)
			{
				cache2 = new List<Vector2>();
				for (int i = 0; i < 18; i++)
					cache2.Add(TrailPosAtProgressScuffed());
			}

			float progress = EaseProgress(Projectile.ai[0]);
			cache.Add(TrailPosAtProgressScuffed());
			cache2.Add(TrailPosAtProgressScuffed());
			oldSquish.Add(squish);

			while (cache.Count > 20)
			{
				cache.RemoveAt(0);
				oldSquish.RemoveAt(0);
			}

			while (cache2.Count > 18)
				cache2.RemoveAt(0);
		}

		public Vector2 TrailPosAtProgressScuffed()
		{
			float angleShift = ((Projectile.rotation - midRotation) - 0.78f);

			//Get the coordinates of the angle shift.
			Vector2 anglePoint = angleShift.ToRotationVector2();

			anglePoint *= new Vector2(1, squish);

			//And back into an angle
			angleShift = anglePoint.ToRotation();

			return Projectile.Center + (((angleShift) + midRotation).ToRotationVector2() * (float)Math.Sqrt(1 + (squish * squish)) * 30);
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(4), factor => 62 * oldSquish[(int)((factor) * 20f)], factor =>
			{
				if (factor.X > 0.98f)
					return Color.Transparent;

				float progress = EaseProgress(Projectile.ai[0]);
				return new Color(0, 100, 255, 0) * MathHelper.Min(Math.Min(progress * 10, (1 - progress) * 10), 1);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 18, new TriangularTip(4), factor => 46 * oldSquish[(int)((factor) * 20f)], factor =>
			{
				if (factor.X > 0.98f)
					return Color.Transparent;

				float progress = EaseProgress(Projectile.ai[0]);
				return Color.White * MathHelper.Min(Math.Min(progress * 10, (1 - progress) * 10), 1);
			});

			trail2.Positions = cache2.ToArray();
			trail2.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["LightsaberTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>(Texture + "_Trail").Value);
			trail2?.Render(effect);
		}

		private float EaseProgress(float input)
		{
			if (currentAttack == CurrentAttack.Throw)
				return EaseFunction.EaseQuadInOut.Ease(input);
			return EaseFunction.EaseCubicInOut.Ease(input);
		}
	}
}