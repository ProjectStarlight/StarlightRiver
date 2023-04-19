//TODO:
//Clean up code
//Merge boilerplate code in lightsabertypes for swinging

//TODO on white rightclick:
//Make the lightsabers throw in a synergized way
//Move check from shoot to canuseitem
//Sound effects

//TODO on blue rightclick:
//Make it have less boilerplate

//TODO on purple rightclick:
//Make it not geek out with gfxoffset
//Lighting

//TODO on yellow rightclick:
//Make the player not able to clip through walls
//Sound effects
//Make the player not freeze for a second
//Garauntee it's impossible for the player to get stuck dashing
//Adjust color

//TODO on orange rightclick:
//Better effect
//Better description

using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class Lightsabers : GlobalItem
	{
		public static int[] phaseblades = new int[]
		{
			ItemID.RedPhaseblade,
			ItemID.WhitePhaseblade,
			ItemID.PurplePhaseblade,
			ItemID.YellowPhaseblade,
			ItemID.BluePhaseblade,
			ItemID.GreenPhaseblade,
			ItemID.OrangePhaseblade
		};

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (!Main.Configuration.Get<bool>("Lightsabers", true))
				return;

			switch (item.type)
			{
				case ItemID.RedPhaseblade:
					tooltips.Add(new TooltipLine(Mod, "Lightsaber Description", "Right click to pull in enemies, release to slash them for high damage \n'There is no escape'"));
					break;
				case ItemID.WhitePhaseblade:
					tooltips.Add(new TooltipLine(Mod, "Lightsaber Description", "Right click to pull out a second blade\n'I am no Jedi.'"));
					break;
				case ItemID.PurplePhaseblade:
					tooltips.Add(new TooltipLine(Mod, "Lightsaber Description", "Right click to launch force lightning \n'The senate will decide your fate'"));
					break;
				case ItemID.YellowPhaseblade:
					tooltips.Add(new TooltipLine(Mod, "Lightsaber Description", "Right click to dash towards the blade while it's thrown out\n'I've got a bad feeling about this'"));
					break;
				case ItemID.BluePhaseblade:
					tooltips.Add(new TooltipLine(Mod, "Lightsaber Description", "Right click to parry oncoming projectiles \n'May the force be with you'"));
					break;
				case ItemID.GreenPhaseblade:
					tooltips.Add(new TooltipLine(Mod, "Lightsaber Description", "Right click to flip in the air, spinning the blade \n'Do or do not. There is no try'"));
					break;
				case ItemID.OrangePhaseblade:
					tooltips.Add(new TooltipLine(Mod, "Lightsaber Description", "Right click throw to the cursor \nHold right click to leave it there\n'[Insert star wars quote]'"));
					break;
			}
		}

		public override void SetDefaults(Item item)
		{
			if (!Main.Configuration.Get<bool>("Lightsabers", true))
				return;

			switch (item.type)
			{
				case ItemID.RedPhaseblade:
					item.shoot = ModContent.ProjectileType<LightsaberProj_Red>();
					break;
				case ItemID.WhitePhaseblade:
					item.shoot = ModContent.ProjectileType<LightsaberProj_White>();
					break;
				case ItemID.PurplePhaseblade:
					item.shoot = ModContent.ProjectileType<LightsaberProj_Purple>();
					break;
				case ItemID.YellowPhaseblade:
					item.shoot = ModContent.ProjectileType<LightsaberProj_Yellow>();
					break;
				case ItemID.BluePhaseblade:
					item.shoot = ModContent.ProjectileType<BlueLightsaberProjectile>();
					break;
				case ItemID.GreenPhaseblade:
					item.shoot = ModContent.ProjectileType<LightsaberProj_Green>();
					break;
				case ItemID.OrangePhaseblade:
					item.shoot = ModContent.ProjectileType<LightsaberProj_Orange>();
					break;
			}

			if (phaseblades.Contains(item.type))
			{
				item.damage = 16;
				item.noUseGraphic = true;
				item.noMelee = true;
				item.autoReuse = false;
				item.useTime = 12;
				item.useAnimation = 12;
				item.channel = true;
				item.useStyle = ItemUseStyleID.Shoot;
				item.knockBack = 2.5f;
				item.shootSpeed = 14f;
			}
		}
		public override bool AltFunctionUse(Item item, Player player)
		{
			if (!Main.Configuration.Get<bool>("Lightsabers", true))
				return base.AltFunctionUse(item, player);

			if (phaseblades.Contains(item.type) && item.type != ItemID.YellowPhaseblade && item.type != ItemID.OrangePhaseblade)
				return true;

			return base.AltFunctionUse(item, player);
		}

		public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (Main.Configuration.Get<bool>("Lightsabers", true))
			{
				if (item.type == ItemID.WhitePhaseblade && player.GetModPlayer<LightsaberPlayer>().whiteCooldown > 0 && player.altFunctionUse == 2)
					return false;

				if (phaseblades.Contains(item.type))
				{
					var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
					(proj.ModProjectile as LightsaberProj).rightClicked = player.altFunctionUse == 2;
					return false;
				}
			}

			return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
		}
	}

	public abstract class LightsaberProj : ModProjectile
	{
		enum CurrentAttack : int
		{
			Slash1 = 0,
			Slash2 = 1,
			Slash3 = 2,
			Slash4 = 3,
			Throw = 4,
			Reset = 5
		}

		private CurrentAttack currentAttack = CurrentAttack.Slash1;

		protected bool frontHand = true;
		protected bool updatePoints = true;

		protected Vector2 oldScreenPos = Vector2.Zero;
		protected Vector2 anchorPoint = Vector2.Zero;

		protected int afterImageLength = 16;

		protected int throwTimer = 0;
		protected bool thrown = false;
		protected bool turnedAround = false;
		protected Vector2 thrownDirection = Vector2.Zero;

		public bool rightClicked = false;
		protected bool hide = true;
		protected bool canHit = false;

		protected bool initialized = false;

		protected int attackDuration = 0;

		protected float startRotation = 0f;

		protected float midRotation = 0f;
		protected float endMidRotation = 0f;
		protected float startMidRotation = 0f;

		protected float endRotation = 0f;

		protected bool facingRight;

		protected float squish = 1;

		protected float startSquish = 0.4f;

		protected float endSquish = 0.4f;

		protected float rotVel = 0f;

		protected int growCounter = 0;

		protected List<float> oldRotation = new();
		protected List<Vector2> oldPositionDrawing = new();
		protected List<float> oldSquish = new();

		protected List<Vector2> oldPositionCollision = new();

		protected List<NPC> hit = new();

		protected ref float UneasedProgress => ref Projectile.ai[0];

		protected Player Owner => Main.player[Projectile.owner];

		protected virtual Vector3 BladeColor => Color.Green.ToVector3();

		private bool FirstTickOfSwing => Projectile.ai[0] == 0;

		public override string Texture => AssetDirectory.LightsaberItem + "LightsaberProj";

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
			Projectile.Size = new Vector2(150, 150);
			Projectile.penetrate = 1;
			Projectile.ownerHitCheck = true;
			Projectile.extraUpdates = 5;
		}

		public override void AI()
		{
			if (!hide)
				Lighting.AddLight(Projectile.Center, BladeColor * 4.6f);

			if (rightClicked)
			{
				RightClickBehavior();
			}
			else
			{
				hide = false;
				canHit = true;

				if (thrown)
					ThrownBehavior();
				else
					HeldBehavior();

				SafeLeftClickBehavior();
			}

			if (!updatePoints)
				return;

			oldSquish.Add(squish);
			oldRotation.Add(Projectile.rotation);
			oldPositionDrawing.Add(anchorPoint);
			oldPositionCollision.Add(Projectile.Center);

			if (oldRotation.Count > afterImageLength)
				oldRotation.RemoveAt(0);

			if (oldPositionDrawing.Count > afterImageLength)
				oldPositionDrawing.RemoveAt(0);

			if (oldSquish.Count > afterImageLength)
				oldSquish.RemoveAt(0);

			if (oldPositionCollision.Count > afterImageLength)
				oldPositionCollision.RemoveAt(0);

			if (thrown)
			{
				for (int i = 0; i < oldPositionDrawing.Count; i++)
				{
					oldPositionCollision[i] += Projectile.velocity;
					oldPositionDrawing[i] += Projectile.velocity;
					oldSquish[i] = squish;
				}
			}

			if (thrown && throwTimer % Projectile.extraUpdates == Projectile.extraUpdates - 1)
			{
				for (int i = 0; i < oldPositionDrawing.Count; i++)
				{
					oldPositionDrawing[i] += oldScreenPos - Main.screenPosition;
				}
			}

			oldScreenPos = Main.screenPosition;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;

			for (int i = 0; i < oldPositionCollision.Count; i++)
			{
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), oldPositionCollision[i], GetCollisionPoint(i) + oldPositionCollision[i], 40, ref collisionPoint))
				{
					Vector2 dustPos = oldPositionCollision[i] + Vector2.Normalize(GetCollisionPoint(i)) * collisionPoint;
					Vector2 dustVel = Vector2.Normalize(GetCollisionPoint(i)).RotatedBy(1.57f).RotatedByRandom(0.3f);

					for (int j = 0; j < 11; j++)
					{
						float speed = Main.rand.NextFloat();
						int dir = Main.rand.NextBool() ? 1 : -1;
						Dust.NewDustPerfect(dustPos, ModContent.DustType<LightsaberGlow>(), dustVel * dir * speed * 5, default, new Color(BladeColor.X * 2, BladeColor.Y * 2, BladeColor.Z * 2), MathHelper.Lerp(0.95f, 0.55f, speed));
					}

					return true;
				}
			}

			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (hit.Contains(target) || !canHit)
				return false;

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hitinfo, int damageDone)
		{
			Projectile.penetrate++;
			hit.Add(target);

			if (CameraSystem.shake < 20)
				CameraSystem.shake += 2;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Owner.Center.X);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (hide)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D whiteTex = ModContent.Request<Texture2D>(Texture + "_White").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D smallGlowTex = ModContent.Request<Texture2D>(Texture + "_SmallGlow").Value;

			//X = pitch, Y = yaw of drawn projectile (regular rotation is roll)
			Vector2 scaleVec;
			Effect effect = Filters.Scene["3DSwing"].GetShader().Shader;
			effect.Parameters["color"].SetValue(Color.White.ToVector4());
			effect.Parameters["rotation"].SetValue(Projectile.rotation - midRotation);

			for (int i = 0; i < oldPositionDrawing.Count; i++) //disgusting amount of spritebatch restarts but I can't figure out another way to do it
			{
				scaleVec = new Vector2(1, oldSquish[i]) * 4;
				effect.Parameters["rotation"].SetValue(oldRotation[i] - midRotation);

				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(new Vector4(BladeColor * 40, 0) * MathHelper.Max(rotVel * 0.125f, 0.00125f));
				effect.Parameters["pommelToOriginPercent"].SetValue(-0.305f);
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(glowTex, oldPositionDrawing[i], null, Color.White, midRotation, glowTex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);

				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(new Vector4(BladeColor * 40, 0) * MathHelper.Max(rotVel * 0.25f, 0.0025f));
				effect.Parameters["pommelToOriginPercent"].SetValue(0.1f);
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(smallGlowTex, oldPositionDrawing[i], null, Color.White, midRotation, smallGlowTex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);

				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(Color.White.ToVector4());
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(whiteTex, oldPositionDrawing[i], null, Color.White, midRotation, whiteTex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);
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

			return (angleShift + midRotation).ToRotationVector2() * 80;
		}

		private float EaseProgress(float input)
		{
			if (currentAttack == CurrentAttack.Throw)
				return EaseFunction.EaseQuadInOut.Ease(input);

			return EaseFunction.EaseCubicInOut.Ease(input);
		}

		protected virtual void ThrownBehavior()
		{
			rotVel = 0.04f;
			squish = MathHelper.Lerp(squish, 0.6f - Projectile.velocity.Length() * 0.08f, 0.1f);
			anchorPoint = Projectile.Center - Main.screenPosition;
			Projectile.timeLeft = 50;
			Owner.itemTime = Owner.itemAnimation = 2;
			throwTimer++;
			UneasedProgress = (float)Math.Cos(throwTimer * 0.01f);
			float progress = EaseFunction.EaseQuadOut.Ease(Math.Abs(UneasedProgress)) * Math.Sign(Math.Cos(throwTimer * 0.01f));
			bool goingBack = Math.Sign(progress) == -1;

			if (goingBack)
			{
				if (!turnedAround)
				{
					turnedAround = true;
					hit = new List<NPC>();
				}

				thrownDirection = Owner.DirectionTo(Projectile.Center);
			}

			Projectile.velocity = progress * 5 * thrownDirection;
			midRotation = Projectile.velocity.ToRotation();

			Projectile.extraUpdates = 8;
			Projectile.rotation += 0.06f;

			if (Projectile.Distance(Owner.Center) < 20 && goingBack)
				Projectile.active = false;
		}

		protected virtual void HeldBehavior()
		{
			Projectile.velocity = Vector2.Zero;

			if (frontHand)
				Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			else
				Projectile.Center = Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);

			Owner.heldProj = Projectile.whoAmI;

			if (FirstTickOfSwing)
			{
				hit = new List<NPC>();

				if (Owner.DirectionTo(Main.MouseWorld).X > 0)
					facingRight = true;
				else
					facingRight = false;

				float rot = Owner.DirectionTo(Main.MouseWorld).ToRotation();

				if (!frontHand)
					rot += 0.5f * (facingRight ? 1 : -1);

				if (!initialized)
				{
					anchorPoint = Vector2.Zero;
					initialized = true;
					endRotation = rot - 1f * Owner.direction;

					oldRotation = new List<float>();
					oldPositionDrawing = new List<Vector2>();
					oldSquish = new List<float>();
					oldPositionCollision = new List<Vector2>();
				}
				else
				{
					currentAttack = (CurrentAttack)((int)currentAttack + 1);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Owner.Center);

				startRotation = endRotation;
				startSquish = endSquish;
				endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
				startMidRotation = midRotation;

				switch (currentAttack)
				{
					case CurrentAttack.Slash1:
						endSquish = 0.3f;
						endRotation = rot + 3f * Owner.direction;
						attackDuration = 125;
						break;

					case CurrentAttack.Slash2:
						endSquish = 0.2f;
						attackDuration = 129;
						endRotation = rot - 2f * Owner.direction;
						break;

					case CurrentAttack.Slash3:
						endSquish = 0.45f;
						endRotation = rot + 3f * Owner.direction;
						attackDuration = 125;
						break;

					case CurrentAttack.Slash4:
						endSquish = 0.15f;
						endRotation = rot - 3f * Owner.direction;
						attackDuration = 125;
						break;

					case CurrentAttack.Throw:
						endSquish = 0.6f;
						attackDuration = 250;
						endRotation = rot + 10f * Owner.direction;
						break;

					case CurrentAttack.Reset:
						endSquish = 0.6f;
						Projectile.active = false;
						break;
				}
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

			float progress = EaseProgress(Projectile.ai[0]);

			Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + rotVel * 4), 1.3f);

			Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);
			midRotation = MathHelper.Lerp(startMidRotation, endMidRotation, progress);
			squish = MathHelper.Lerp(startSquish, endSquish, progress) + 0.35f * (float)Math.Sin(3.14f * progress);
			anchorPoint = Projectile.Center - Main.screenPosition;

			float wrappedRotation = MathHelper.WrapAngle(Projectile.rotation);
			float throwingAngle = MathHelper.WrapAngle(Owner.DirectionTo(Main.MouseWorld).ToRotation());

			if (currentAttack == CurrentAttack.Throw && Math.Abs(wrappedRotation - throwingAngle) < 0.2f && Projectile.ai[0] > 0.4f)
			{
				hit = new List<NPC>();
				oldScreenPos = Main.screenPosition;
				thrown = true;
				thrownDirection = Owner.DirectionTo(Main.MouseWorld);
				Projectile.velocity = thrownDirection * 1;
				afterImageLength = 50;
			}

			Owner.ChangeDir(facingRight ? 1 : -1);

			if (frontHand)
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			else
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);

			Owner.itemAnimation = Owner.itemTime = 5;

			if (Owner.direction != 1)
				Projectile.rotation += 0.78f;
		}

		protected virtual void RightClickBehavior() { }

		protected virtual void SafeLeftClickBehavior() { }
	}
}