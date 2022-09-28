//TODO:
//Clean up code
//Better Collision
//Throwing
//Parrying
//Better sound effects
//Obtainment
//Balance
//Sellprice
//Rarity
//Make it look good when swinging to the left
//Less spritebatch restarts
//Make all 6 lightsaber types
//Lighting
//Make the bloom not disappear during the throw
//Description


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

		private bool initialized = false;

		private int attackDuration = 0;

		private float startRotation = 0f;

		private float midRotation = 0f;
		private float endMidRotation = 0f;
		private float startMidRotation = 0f;

		private float endRotation = 0f;

		private bool facingRight;

		private float squish = 1;

		private float startSquish = 1;

		private float endSquish = 1;

		private float rotVel = 0f;

		private int growCounter = 0;

		private List<float> oldRotation = new List<float>();
		private List<Vector2> oldPosition = new List<Vector2>();
		private List<float> oldSquish = new List<float>();

		private List<Vector2> oldPositionCollision = new List<Vector2>();

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
			Projectile.penetrate = 1;
			Projectile.ownerHitCheck = true;
			Projectile.extraUpdates = 4;
		}

		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, Color.Blue.ToVector3() * 1.6f);
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
					oldSquish = new List<float>();
					oldPositionCollision = new List<Vector2>();
				}
				else
				{
					currentAttack = (CurrentAttack)((int)currentAttack + 1);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f,0.1f)}, owner.Center);

				startRotation = endRotation;
				startSquish = 0.4f;
				endMidRotation = rot + Main.rand.NextFloat(-0.25f, 0.25f);
				startMidRotation = rot + Main.rand.NextFloat(-0.25f, 0.25f);

				switch (currentAttack)
				{
					case CurrentAttack.Slash1:
						endSquish = 0.3f;
						endRotation = rot + (3f * owner.direction);
						attackDuration = 100;
						break;
					case CurrentAttack.Slash2:
						endSquish = 0.2f;
						attackDuration = 120;
						endRotation = rot - (2f * owner.direction);
						break;
					case CurrentAttack.Slash3:
						endSquish = 0.45f;
						endRotation = rot + (3f * owner.direction);
						attackDuration = 100;
						break;
					case CurrentAttack.Slash4:
						endSquish = 0.15f;
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

			float progress = EaseProgress(Projectile.ai[0]);

			/*if (Main.netMode != NetmodeID.Server)
			{
				if (trailCounter % 5 == 0 || (progress > 0.1f && progress < 0.9f))
				{
					ManageCaches();
					ManageTrail();
				}
			}*/

			Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + (rotVel * 4)), 1.3f);

			Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);
			midRotation = MathHelper.Lerp(startMidRotation, endMidRotation, progress);
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

			if (owner.direction != 1)
				Projectile.rotation += 0.78f;

			oldSquish.Add(squish);
			oldRotation.Add(Projectile.rotation);
			oldPosition.Add(Projectile.Center - Main.screenPosition);
			oldPositionCollision.Add(Projectile.Center);

			if (oldRotation.Count > 16)
				oldRotation.RemoveAt(0);
			if (oldPosition.Count > 16)
				oldPosition.RemoveAt(0);
			if (oldSquish.Count > 16)
				oldSquish.RemoveAt(0);
			if (oldPositionCollision.Count > 16)
				oldPositionCollision.RemoveAt(0);

		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;

			for (int i = 0; i < oldPositionCollision.Count; i++) 
			{
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), oldPositionCollision[i], GetCollisionPoint(i) + oldPositionCollision[i], 40, ref collisionPoint))
					return true;
			}
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
			Projectile.penetrate++;
			hit.Add(target);
			Helper.PlayPitched("Impacts/PanBonkSmall", 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), target.Center);
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = Math.Sign(target.Center.X - owner.Center.X);
        }

        public override bool PreDraw(ref Color lightColor)
        {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D whiteTex = ModContent.Request<Texture2D>(Texture + "_White").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D smallGlowTex = ModContent.Request<Texture2D>(Texture + "_SmallGlow").Value;

			Vector2 scaleVec = new Vector2(1, squish) * 4;
            Effect effect = Filters.Scene["3DSwing"].GetShader().Shader;
            effect.Parameters["pommelToOriginPercent"].SetValue(0.0f);
            effect.Parameters["color"].SetValue(Color.White.ToVector4());
			effect.Parameters["rotation"].SetValue(Projectile.rotation - midRotation);
			for (int i = 0; i < oldPosition.Count; i++) //disgusting amount of spritebatch restarts but I can't figure out another way to do it
			{
				scaleVec = new Vector2(1, oldSquish[i]) * 4;
				effect.Parameters["rotation"].SetValue(oldRotation[i] - midRotation);

				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(new Vector4(0, 10f, 25f, 0) * MathHelper.Max(rotVel * 0.125f, 0.00125f));
				effect.Parameters["pommelToOriginPercent"].SetValue(-0.305f);
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(glowTex, oldPosition[i], null, Color.White, midRotation, (glowTex.Size() / 2f), scaleVec, SpriteEffects.None, 0f);


				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(new Vector4(0, 10f, 25f, 0) * MathHelper.Max(rotVel * 0.25f, 0.0025f));
				effect.Parameters["pommelToOriginPercent"].SetValue(0.0f);
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(smallGlowTex, oldPosition[i], null, Color.White, midRotation, smallGlowTex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);

				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(Color.White.ToVector4());
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(whiteTex, oldPosition[i], null, Color.White, midRotation, whiteTex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);
			}
			scaleVec = new Vector2(1, squish) * 4;
			Main.spriteBatch.End();

			effect.Parameters["color"].SetValue(lightColor.ToVector4());
			effect.Parameters["rotation"].SetValue(Projectile.rotation - midRotation);

			Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, midRotation, tex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

		private Vector2 GetCollisionPoint(int i)
        {
			float angleShift = oldRotation[i] - midRotation;

			//Get the coordinates of the angle shift.
			Vector2 anglePoint = angleShift.ToRotationVector2();

			//Squish the angle point's coordinate
			anglePoint.X *= 1;
			anglePoint.Y *= oldSquish[i];

			//And back into an angle
			angleShift = anglePoint.ToRotation();

			return ((angleShift + midRotation).ToRotationVector2() * 80);
		}

		private float EaseProgress(float input)
		{
			if (currentAttack == CurrentAttack.Throw)
				return EaseFunction.EaseQuadInOut.Ease(input);
			return EaseFunction.EaseCubicInOut.Ease(input);
		}
	}
}