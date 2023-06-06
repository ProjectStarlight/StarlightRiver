using StarlightRiver.Helpers;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class Gunchucks : ModItem
	{
		private int combo;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gunchucks");
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.shootSpeed = 1f;
			Item.knockBack = 4f;
			Item.UseSound = SoundID.Item116;
			Item.shoot = ModContent.ProjectileType<GunchuckProj>();
			Item.value = Item.sellPrice(gold: 4);
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.autoReuse = true;
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 9;
			Item.useAmmo = AmmoID.Bullet;
			Item.rare = ItemRarityID.Green;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			combo++;
			float distanceMult = Main.rand.NextFloat(0.8f, 1.2f);
			float curvatureMult = 0.7f;

			Vector2 direction = Vector2.Normalize(velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f))) * 7;
			var proj = Projectile.NewProjectileDirect(source, position, direction, ModContent.ProjectileType<GunchuckProj>(), damage, knockback, player.whoAmI);

			if (proj.ModProjectile is GunchuckProj modProj)
			{
				modProj.Flip = combo % 2 == 0;
				modProj.SwingTime = (int)(Item.useTime * 2f);
				modProj.SwingDistance = 20 * distanceMult;
				modProj.Curvature = 0.33f * curvatureMult;
				modProj.Ammo = type;
				modProj.FireTime = Main.rand.NextFloat(0.55f, 0.65f);
			}

			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);

			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Chain, 10);
			recipe.AddIngredient(ItemID.Boomstick, 2);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override float UseTimeMultiplier(Player Player)
		{
			return base.UseTimeMultiplier(Player) * Player.GetTotalAttackSpeed(DamageClass.Melee); //Scale with melee speed buffs, like whips
		}
	}

	public class GunchuckProj : ModProjectile
	{
		public const float THROW_RANGE = 120; //Peak distance from Player when thrown out, in pixels

		public int SwingTime;
		public float SwingDistance;
		public float Curvature;
		public bool Flip;
		public int Ammo;
		public float FireTime;

		public Vector2 CurrentBase = Vector2.Zero;

		private bool shot = false;
		private float newRotation = 0f;

		public override string Texture => AssetDirectory.MiscItem + Name;

		private Player Owner => Main.player[Projectile.owner];

		public ref float Timer => ref Projectile.ai[0];
		public ref float AiState => ref Projectile.ai[1];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sanguine Flayer");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.Size = new Vector2(85, 85);
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
		}

		public override void AI()
		{
			if (Projectile.timeLeft > 2) //Initialize chain control points on first tick, in case of Projectile hooking in on first tick
			{
				_chainMidA = Projectile.Center;
				_chainMidB = Projectile.Center;
			}

			Projectile.timeLeft = 2;

			ThrowOutAI();

			float progress = Timer / SwingTime;
			progress = EaseFunction.EaseQuadOut.Ease(progress);

			if (progress > FireTime && !shot)
			{
				Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;

				Vector2 projBottom = Projectile.Center - new Vector2(projTexture.Width / 2, 0).RotatedBy(Projectile.rotation + MathHelper.PiOver2) * 0.75f;

				float angleMaxDeviation = MathHelper.Pi * 0.85f;
				float angleOffset = Owner.direction * (Flip ? -1 : 1) * MathHelper.Lerp(angleMaxDeviation, -angleMaxDeviation / 4, progress);

				_chainMidA = Owner.MountedCenter + GetSwingPosition(progress).RotatedBy(angleOffset) * Curvature;
				_chainMidB = Owner.MountedCenter + GetSwingPosition(progress).RotatedBy(angleOffset / 2) * Curvature * 2.5f;

				var curve = new BezierCurve(new Vector2[] { Owner.MountedCenter, _chainMidA, _chainMidB, projBottom });

				int numPoints = 10; //Should make dynamic based on curve length, but I'm not sure how to smoothly do that while using a bezier curve
				Vector2[] chainPositions = curve.GetPoints(numPoints).ToArray();

				newRotation = (projBottom - chainPositions[^2]).ToRotation();

				shot = true;
				Vector2 direction = newRotation.ToRotationVector2();

				for (int i = 0; i < 5; i++)
				{
					Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center + direction * 25, direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(15, 20), Ammo, Projectile.damage, Projectile.knockBack, Owner.whoAmI);
				}

				Helper.PlayPitched("Guns/Scrapshot", 0.4f, 0, Projectile.Center);

				float spread = 0.4f;

				for (int k = 0; k < 15; k++)
				{
					Vector2 dustDirection = direction.RotatedByRandom(spread);
					Dust.NewDustPerfect(Projectile.Center + direction * 25, ModContent.DustType<Dusts.Glow>(), dustDirection * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
				}
			}

			Owner.itemRotation = MathHelper.WrapAngle(Owner.AngleTo(Main.MouseWorld) - (Owner.direction < 0 ? MathHelper.Pi : 0));
		}

		private Vector2 GetSwingPosition(float progress)
		{
			//Starts at owner center, goes to peak range, then returns to owner center
			float distance = MathHelper.Clamp(SwingDistance, THROW_RANGE * 0.1f, THROW_RANGE) * MathHelper.Lerp((float)Math.Sin(progress * MathHelper.Pi), 1, 0.04f);

			float angleMaxDeviation = MathHelper.Pi / 1.2f;
			float angleOffset = Owner.direction * (Flip ? -1 : 1) * MathHelper.Lerp(-angleMaxDeviation, angleMaxDeviation, progress); //Moves clockwise if Player is facing right, counterclockwise if facing left
			return Projectile.velocity.RotatedBy(angleOffset) * distance;
		}

		private void ThrowOutAI()
		{
			Projectile.rotation = Projectile.AngleFrom(Owner.Center);
			Vector2 position = Owner.MountedCenter;
			float progress = ++Timer / SwingTime; //How far the Projectile is through its swing
			progress = EaseFunction.EaseQuadOut.Ease(progress);

			Projectile.Center = position + GetSwingPosition(progress);
			Projectile.direction = Projectile.spriteDirection = -Owner.direction * (Flip ? -1 : 1);

			if (Timer >= SwingTime)
				Projectile.Kill();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft > 2)
				return false;

			Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;

			//End control point for the chain
			Vector2 projBottom = Projectile.Center + new Vector2(projTexture.Width / 2, 0).RotatedBy(Projectile.rotation + MathHelper.PiOver2) * 0.75f;
			DrawChainCurve(Main.spriteBatch, projBottom, out Vector2[] chainPositions);

			//Adjust rotation to face from the last point in the bezier curve
			newRotation = (projBottom - chainPositions[^2]).ToRotation();

			//Draw from bottom center of texture
			var origin = new Vector2(0, projTexture.Height / 2);
			SpriteEffects flip = (Projectile.spriteDirection < 0) ? SpriteEffects.FlipVertically : SpriteEffects.None;

			lightColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));

			Main.EntitySpriteDraw(projTexture, projBottom - Main.screenPosition, null, lightColor, newRotation, origin, Projectile.scale, flip, 0);

			CurrentBase = projBottom + (newRotation - 1.57f).ToRotationVector2() * (projTexture.Height / 2);

			return false;
		}

		//Control points for drawing chain bezier, update slowly when hooked in
		private Vector2 _chainMidA;
		private Vector2 _chainMidB;
		private void DrawChainCurve(SpriteBatch spriteBatch, Vector2 projBottom, out Vector2[] chainPositions)
		{
			Texture2D chainTex = ModContent.Request<Texture2D>(Texture + "_Chain").Value;

			float progress = Timer / SwingTime;

			progress = EaseFunction.EaseQuadOut.Ease(progress);

			float angleMaxDeviation = MathHelper.Pi * 0.85f;
			float angleOffset = Owner.direction * (Flip ? -1 : 1) * MathHelper.Lerp(angleMaxDeviation, -angleMaxDeviation / 4, progress);

			_chainMidA = Owner.MountedCenter + GetSwingPosition(progress).RotatedBy(angleOffset) * Curvature;
			_chainMidB = Owner.MountedCenter + GetSwingPosition(progress).RotatedBy(angleOffset / 2) * Curvature * 2.5f;

			var curve = new BezierCurve(new Vector2[] { Owner.MountedCenter, _chainMidA, _chainMidB, projBottom });

			int numPoints = 10; //Should make dynamic based on curve length, but I'm not sure how to smoothly do that while using a bezier curve
			chainPositions = curve.GetPoints(numPoints).ToArray();

			//Draw each chain segment, skipping the very first one, as it draws partially behind the Player
			for (int i = 1; i < numPoints; i++)
			{
				Vector2 position = chainPositions[i];

				float rotation = (chainPositions[i] - chainPositions[i - 1]).ToRotation() - MathHelper.PiOver2; //Calculate rotation based on direction from last point
				float yScale = Vector2.Distance(chainPositions[i], chainPositions[i - 1]) / chainTex.Height; //Calculate how much to squash/stretch for smooth chain based on distance between points

				var scale = new Vector2(1, yScale); // Stretch/Squash chain segment
				Color chainLightColor = Lighting.GetColor((int)position.X / 16, (int)position.Y / 16); //Lighting of the position of the chain segment
				var origin = new Vector2(chainTex.Width / 2, chainTex.Height); //Draw from center bottom of texture
				spriteBatch.Draw(chainTex, position - Main.screenPosition, null, chainLightColor, rotation, origin, scale, SpriteEffects.None, 0);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			var curve = new BezierCurve(new Vector2[] { Owner.MountedCenter, _chainMidA, _chainMidB, Projectile.Center });

			int numPoints = 32;
			Vector2[] chainPositions = curve.GetPoints(numPoints).ToArray();
			float collisionPoint = 0;
			for (int i = 1; i < numPoints; i++)
			{
				Vector2 position = chainPositions[i];
				Vector2 previousPosition = chainPositions[i - 1];
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), position, previousPosition, 6, ref collisionPoint))
					return true;
			}

			return base.Colliding(projHitbox, targetHitbox);
		}
	}
}