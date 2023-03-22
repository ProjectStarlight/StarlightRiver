using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
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
			Down,
			Spin,
			UpperCut,
			Slam
		}

		private AttackType currentAttack = AttackType.Down;

		private int timer = 0;
		private bool curAttackDone = false;
		private bool active = true;

		Player Player => Main.player[Projectile.owner];
		float ArmRotation => Player.direction > 0 ?
			Projectile.rotation - MathHelper.Pi / 3:
			Projectile.rotation - MathHelper.Pi * 7 / 6;

		private Func<float, float> DownEase = Helper.CubicBezier(0.69f, -0.08f, 0.9f, -0.23f);
		private Func<float, float> SpinEase = Helper.CubicBezier(0.5f, -0.25f, 0.5f, 1);
		private Func<float, float> SlamEase = Helper.CubicBezier(0.3f, -2f, 0.95f, -2f);
		
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

			DrawOffsetX = -Projectile.width / 4;
			DrawOriginOffsetX = -Projectile.width / 4;
			DrawOriginOffsetY = -Projectile.height / 4;
		}

		public override bool PreAI()
		{
			Player.heldProj = Projectile.whoAmI;
			Projectile.velocity = Vector2.Zero;
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
					DownAttack();
					break;
			}

			AdjustPlayer();
			AdjustProjectile();
			timer++;

			if (curAttackDone)
				NextAttack();
		}

		public override bool? CanHitNPC(NPC target)
		{
			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{

		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{

		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!active)
				return false;

			float collisionPoint = 0f;
			Vector2 start = Player.MountedCenter;
			Vector2 end = start + Vector2.UnitX.RotatedBy(Projectile.rotation - MathHelper.PiOver4) * Projectile.Size.Length() * 0.75f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 24 * Projectile.scale, ref collisionPoint);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return base.PreDraw(ref lightColor);
		}

		private void AdjustPlayer()
		{
			Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, ArmRotation);
			Player.itemAnimation = Player.itemTime = 5;
		}

		private void AdjustProjectile()
		{
			Projectile.position = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, ArmRotation);
			Projectile.position.Y -= (float)(Projectile.height / 2);
		}

		public void DownAttack()
		{
			float startAngle = -MathHelper.PiOver2 - Player.direction * MathHelper.PiOver2;
			float swingAngle = Player.direction * MathHelper.Pi;

			float progress = DownEase((float)timer / 90);
			Projectile.rotation = startAngle + swingAngle * progress + MathHelper.PiOver4;
			active = progress > 0;

			if (timer == 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 100)
				curAttackDone = true;
		}

		public void SpinAttack()
		{
			float startAngle = -MathHelper.PiOver2 - Player.direction * MathHelper.PiOver2 * 3;
			float swingAngle = Player.direction * MathHelper.Pi * 4.25f;

			float progress = SpinEase((float)timer / 150);
			Projectile.rotation = startAngle + swingAngle * progress + MathHelper.PiOver4;
			active = progress > 0;

			if (timer == 50 || timer == 120)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 160)
				curAttackDone = true;
		}

		public void UppercutAttack()
		{
			float startAngle = -MathHelper.PiOver2 - Player.direction * MathHelper.Pi * 1.25f;
			float swingAngle = Player.direction * -MathHelper.Pi * 2 / 3;

			float progress = DownEase((float)timer / 45);
			Projectile.rotation = startAngle + swingAngle * progress  + MathHelper.PiOver4;
			active = progress > 0;

			if (timer == 0)
				Projectile.ResetLocalNPCHitImmunity();

			if (timer > 90)
				curAttackDone = true;
		}

		public void SlamAttack()
		{
			float startAngle = -MathHelper.PiOver2 + Player.direction * MathHelper.Pi / 12;
			float swingAngle = Player.direction * MathHelper.PiOver2;
			
			float progress = SlamEase((float)timer / 120);
			Projectile.rotation = startAngle + swingAngle * progress + MathHelper.PiOver4;

			if (timer == 60)
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
			
			if (currentAttack < AttackType.Slam)
				currentAttack++;
			else
				currentAttack = AttackType.Down;

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