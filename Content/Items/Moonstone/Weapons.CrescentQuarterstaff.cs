using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class CrescentQuarterstaff : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
			Tooltip.SetDefault("Update this egshels");
		}

		public override void SetDefaults()
		{
			Item.damage = 100;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 20;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6.5f;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.crit = 4;
			Item.rare = 2;
			Item.shootSpeed = 14f;
			Item.autoReuse = false;
			Item.shoot = ProjectileType<CrescentQuarterstaffProj>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<MoonstoneBarItem>(), 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class CrescentQuarterstaffProj : ModProjectile
	{
		enum AttackType : int
		{
			Stab,
			Spin,
			UpperCut,
			Slam
		}

		private const int MAXCHARGE = 10;

		private AttackType currentAttack = AttackType.Stab;

		private int timer = 0;
		private int freezeTimer = 0;
		private bool active = true;
		private bool curAttackDone = false;

		private float length = 100;
		private float initialRotation = 0;

		private int charge = 0;
		private float zRotation = 0;

		Player Player => Main.player[Projectile.owner];
		float ArmRotation => Projectile.rotation - ((Player.direction > 0) ?  MathHelper.Pi / 3 : MathHelper.Pi * 2 / 3);
		private float Charge => charge / (float)MAXCHARGE;

		private Func<float, float> StabEase = Helper.CubicBezier(0.09f, 0.71f, 0.08f, 1.62f);
		private Func<float, float> SpinEase = Helper.CubicBezier(0.6f, -0.3f, .3f, 1f);
		private Func<float, float> UppercutEase = Helper.CubicBezier(0.6f, -0.3f, .3f, 1.22f);
		private Func<float, float> SlamEase = Helper.CubicBezier(0.5f, -1.6f, 0.9f, -1.6f);
		
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Quarterstaff");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(150, 150);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ownerHitCheck = true;

		}

		public override void OnSpawn(IEntitySource source)
		{
			initialRotation = (Main.MouseWorld - Player.MountedCenter).ToRotation();
		}

		public override bool PreAI()
		{
			Player.heldProj = Projectile.whoAmI;
			Projectile.velocity = Vector2.Zero;
			Lighting.AddLight(Projectile.Center, new Vector3(0.905f, 0.89f, 1) * Charge);
			return true;
		}

		public override void AI()
		{

			switch (currentAttack)
			{
				case AttackType.Spin:
					SpinAttack();
					break;
				case AttackType.UpperCut:
					UppercutAttack();
					break;
				case AttackType.Slam:
					SlamAttack();
					break;
				default:
					StabAttack();
					break;
			}

			Projectile.Center = Player.Center;

			AdjustPlayer();
			if (freezeTimer < 0)
				timer++;

			freezeTimer--;

			if (curAttackDone)
				NextAttack();
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (!active)
				return false;

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (charge < MAXCHARGE)
				charge++;

			if (currentAttack != AttackType.Slam || timer < 50)
			{
				freezeTimer = 4;
				CameraSystem.shake += 4;
			}
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{

		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;
			Vector2 start = Player.MountedCenter;
			Vector2 end = start + Vector2.UnitX.RotatedBy(Projectile.rotation) * length * 1.5f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 40 * Projectile.scale, ref collisionPoint);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D head = Request<Texture2D>(Texture + "_Head").Value;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			SpriteEffects effects = SpriteEffects.None;

			var origin = new Vector2(140, 10);
			origin.X -= length;
			origin.Y += length;

			var scale = new Vector2(Projectile.scale, Projectile.scale);

			Vector2 position = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRotation);

			float rotation = Projectile.rotation;
			bool flipped = false;

			if (currentAttack == AttackType.Spin)
			{
				if (Player.direction > 0 && (zRotation < MathHelper.PiOver2 || zRotation > MathHelper.PiOver2 * 3))
				{
					rotation = (Vector2.UnitX.RotatedBy(Projectile.rotation) * new Vector2(-1, 1)).ToRotation();
					flipped = true;
				}
				if (Player.direction < 0 && zRotation > MathHelper.PiOver2 && zRotation < MathHelper.PiOver2 * 3)
				{
					rotation = (Vector2.UnitX.RotatedBy(Projectile.rotation) * new Vector2(-1, 1)).ToRotation();
					flipped = true;
				}
			}

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Effect effect = Filters.Scene["MoonFireAura"].GetShader().Shader;
			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.01f);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["origin"].SetValue(origin / 150);
			effect.Parameters["zRotation"].SetValue(zRotation + MathHelper.Pi);
			effect.Parameters["projAngle"].SetValue(Projectile.rotation);
			effect.Parameters["flipped"].SetValue(flipped);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[0].Apply();

			spriteBatch.Draw(head, position - Main.screenPosition, null, lightColor, rotation + 0.78f, origin, scale, effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.01f);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["origin"].SetValue(origin / 150);
			effect.Parameters["zRotation"].SetValue(zRotation + MathHelper.Pi);
			effect.Parameters["projAngle"].SetValue(Projectile.rotation);
			effect.Parameters["flipped"].SetValue(flipped);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[1].Apply();

			spriteBatch.Draw(head, position - Main.screenPosition, null, lightColor, rotation + 0.78f, origin, scale, effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			effect.Parameters["time"].SetValue(0);
			effect.Parameters["fireHeight"].SetValue(0);
			effect.Parameters["origin"].SetValue(origin / 150);
			effect.Parameters["zRotation"].SetValue(zRotation + MathHelper.Pi);
			effect.Parameters["projAngle"].SetValue(Projectile.rotation);
			effect.Parameters["flipped"].SetValue(flipped);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[2].Apply();

			spriteBatch.Draw(tex, position - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, Charge), rotation + 0.78f, origin, scale, effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		private void AdjustPlayer()
		{
			if (currentAttack != AttackType.Spin)
				Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, ArmRotation);
			else
				Player.itemRotation = ArmRotation;

			Player.itemAnimation = Player.itemTime = 5;
		}

		public void StabAttack()
		{
			float swingAngle = Player.direction * -MathHelper.Pi / 10;
			float realInitRot = Player.direction > 0 || initialRotation < 0 ? initialRotation : initialRotation - MathHelper.TwoPi;
			float droop = Player.direction > 0 ? (MathHelper.PiOver2 - Math.Abs(realInitRot)) : (MathHelper.PiOver2 - Math.Abs(realInitRot + MathHelper.Pi) );
			float progress = StabEase((float)timer / 15);

			length = 60 + 40 * progress;
			Projectile.rotation = initialRotation + swingAngle * (1 - progress) * droop;
			active = progress > 0;

			if (timer < 9 && freezeTimer < 0)
			{
				Vector2 vel = Vector2.UnitX.RotatedBy(Projectile.rotation) * progress * progress / 2;
				vel.Y *= 0.4f;
				Player.velocity += vel;
			}

			if (timer == 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 15)
				curAttackDone = true;
		}

		public void SpinAttack()
		{
			float startAngle = Player.direction > 0 || initialRotation < 0 ? initialRotation : initialRotation - MathHelper.TwoPi;
			float finalAngle = Player.direction > 0 ? MathHelper.Pi * 0.25f : -MathHelper.Pi * 1.25f;
			float swingAngle = finalAngle - startAngle;

			float progress = SpinEase((float)timer / 120);
			Projectile.rotation = startAngle + swingAngle * progress;
			zRotation = (MathHelper.TwoPi * 2 * progress + ((Player.direction > 0) ? MathHelper.Pi : 0)) % MathHelper.TwoPi;
			Player.UpdateRotation(zRotation);
			active = progress > 0;

			if ((timer == 10 || timer == 60 || timer == 80) && freezeTimer < 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 120)
				curAttackDone = true;
		}

		public void UppercutAttack()
		{
			float startAngle = -MathHelper.PiOver2 - Player.direction * MathHelper.Pi * 1.25f;
			float swingAngle = Player.direction * -MathHelper.Pi * 2 / 3;

			float progress = UppercutEase((float)timer / 25);
			Projectile.rotation = startAngle + swingAngle * progress;
			active = progress > 0;

			if (timer == 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 25)
				curAttackDone = true;
		}

		public void SlamAttack()
		{
			float startAngle = -MathHelper.PiOver2 + Player.direction * MathHelper.Pi / 12;
			float swingAngle = Player.direction * MathHelper.PiOver2 * 1f;
			
			float progress = SlamEase((float)timer / 60);
			Projectile.rotation = startAngle + swingAngle * progress;

			Vector2 tilePos = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRotation) + Vector2.UnitX.RotatedBy(Projectile.rotation) * length;
			tilePos.Y += 15;
			tilePos /= 16;

			if (progress != 1 && freezeTimer < 0)
			{
				if (Main.tile[(int)tilePos.X, (int)tilePos.Y].BlockType == BlockType.Solid && progress > 0.6 && progress != 1 && Math.Sign(Projectile.rotation.ToRotationVector2().X) == Player.direction)
				{
					for (int i = 0; i < 13; i++)
					{
						Vector2 dustVel = Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f)) * Main.rand.NextFloat(-2, -0.5f);
						dustVel.X *= 10;

						if (Math.Abs(dustVel.X) < 6)
							dustVel.X += Math.Sign(dustVel.X) * 6;

						Dust.NewDustPerfect(tilePos * 16 - new Vector2(Main.rand.Next(-20, 20), 17), ModContent.DustType<Dusts.CrescentSmoke>(), dustVel, 0, new Color(236, 214, 146) * 0.15f, Main.rand.NextFloat(0.5f, 1));
					}

					if (Charge > 0)
					{
						// var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, 7), ProjectileType<QuarterOrb>(), (int)MathHelper.Lerp(0, Projectile.damage, Charge), 0, Projectile.owner, 0, 0);

						// if (proj.ModProjectile is QuarterOrb modproj)
							// modproj.moveDirection = new Vector2(-Player.direction, -1);
					}

					CameraSystem.shake += 12;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Player.MountedCenter + Vector2.UnitX.RotatedBy(Projectile.rotation) * length, Vector2.Zero, ProjectileType<GravediggerSlam>(), 0, 0, Player.whoAmI);
				}
			}

			if (timer == 30 && freezeTimer < 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 100)
				curAttackDone = true;
		}

		public void NextAttack()
		{
			if (!Player.channel)
			{
				Projectile.Kill();
			}

			timer = 0;
			freezeTimer = 0;

			zRotation = 0;
			Player.UpdateRotation(0);

			if (currentAttack < AttackType.Slam)
			{
				currentAttack++;
			}
			else
			{
				currentAttack = AttackType.Stab;
				initialRotation = (Main.MouseWorld - Player.MountedCenter).ToRotation();
			}

			Player.direction = Main.MouseWorld.X > Player.position.X ? 1 : -1;
			curAttackDone = false;
		}
	}

	public class QuarterOrb : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			DisplayName.SetDefault("Lunar Orb");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.width = Projectile.height = 36;
			Projectile.timeLeft = 150;
			Projectile.ignoreWater = true;
		}
		public override void AI()
		{

		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{

		}
	}
}