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

		Player Player => Main.player[Projectile.owner];
		float ArmRotation => Projectile.rotation - ((Player.direction > 0) ?  MathHelper.Pi / 3 : MathHelper.Pi * 2 / 3);
		private float Charge => charge / (float)MAXCHARGE;

		private Func<float, float> StabEase = Helper.CubicBezier(0.69f, -0.08f, 0.9f, -0.23f);
		private Func<float, float> SpinEase = Helper.CubicBezier(0.5f, -0.25f, 0.5f, 1);
		private Func<float, float> SlamEase = Helper.CubicBezier(0.3f, -1.6f, 0.95f, -1.6f);
		
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

			if (currentAttack != AttackType.Slam || timer < 60)
			{
				freezeTimer = 2;
				CameraSystem.shake += 2;
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
			float zRotation = 0;
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D head = Request<Texture2D>(Texture + "_Head").Value;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			SpriteEffects effects = zRotation > 1.57f && zRotation < 4.71f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			if (effects == SpriteEffects.FlipHorizontally && Player.direction < 0)
				effects = SpriteEffects.FlipVertically;

			var origin = new Vector2(140, 10);
			origin.X -= length;
			origin.Y += length;

			if (effects == SpriteEffects.FlipHorizontally)
				origin.X = 150 - origin.X;

			if (effects == SpriteEffects.FlipVertically)
				origin.Y = 150 - origin.Y;

			var scale = new Vector2(Projectile.scale, Projectile.scale);

			if (Player.direction > 0)
				scale.X *= (float)Math.Abs(Math.Cos(zRotation));
			else
				scale.Y *= (float)Math.Abs(Math.Cos(zRotation));

			Vector2 position = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRotation);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Effect effect = Filters.Scene["MoonFireAura"].GetShader().Shader;
			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.01f);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[0].Apply();

			spriteBatch.Draw(head, position - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, scale, effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.01f);
			effect.Parameters["fireHeight"].SetValue(0.03f * Charge);
			effect.Parameters["fnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1").Value);
			effect.Parameters["fnoise2"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2").Value);
			effect.Parameters["vnoise"].SetValue(Request<Texture2D>(AssetDirectory.MoonstoneItem + "QuarterstaffMap").Value);
			effect.CurrentTechnique.Passes[1].Apply();

			spriteBatch.Draw(head, position - Main.screenPosition, null, lightColor, Projectile.rotation + 0.78f, origin, scale, effects, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.Draw(tex, position - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, Charge), Projectile.rotation + 0.78f, origin, scale, effects, 0);
			return false;
		}

		private void AdjustPlayer()
		{
			Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, ArmRotation);
			Player.itemAnimation = Player.itemTime = 5;
		}

		public void StabAttack()
		{
			float progress = StabEase((float)timer / 60);
			length = 60 + 40 * progress;
			Projectile.rotation = initialRotation;
			active = progress > 0;

			if (timer == 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 75)
				curAttackDone = true;
		}

		public void SpinAttack()
		{
			float startAngle = Player.direction > 0 || initialRotation < 0 ? initialRotation : initialRotation - MathHelper.TwoPi;
			float finalAngle = Player.direction > 0 ? MathHelper.Pi * 4.25f : -MathHelper.Pi * 5.25f;
			float swingAngle = finalAngle - startAngle;

			float progress = SpinEase((float)timer / 150);
			Projectile.rotation = startAngle + swingAngle * progress;
			active = progress > 0;

			if ((timer == 10 || timer == 60 || timer == 120) && freezeTimer < 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 160)
				curAttackDone = true;
		}

		public void UppercutAttack()
		{
			float startAngle = -MathHelper.PiOver2 - Player.direction * MathHelper.Pi * 1.25f;
			float swingAngle = Player.direction * -MathHelper.Pi * 2 / 3;

			float progress = StabEase((float)timer / 45);
			Projectile.rotation = startAngle + swingAngle * progress;
			active = progress > 0;

			if (timer == 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 90)
				curAttackDone = true;
		}

		public void SlamAttack()
		{
			float startAngle = -MathHelper.PiOver2 + Player.direction * MathHelper.Pi / 12;
			float swingAngle = Player.direction * MathHelper.PiOver2 * 1.2f;
			
			float progress = SlamEase((float)timer / 120);
			Projectile.rotation = startAngle + swingAngle * progress;

			if (timer == 60 && freezeTimer < 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 150)
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
			
			if (currentAttack < AttackType.Slam)
				currentAttack++;
			else
				currentAttack = AttackType.Stab;

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