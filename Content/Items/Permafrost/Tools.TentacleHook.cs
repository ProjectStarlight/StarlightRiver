using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	internal class TentacleHook : ModItem
	{
		public override string Texture => AssetDirectory.PermafrostItem + "TentacleHook";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tentacle Hook");
			Tooltip.SetDefault("Move your mouse to hook around objects");
		}

		public override void SetDefaults()
		{
			Item.noUseGraphic = true;
			Item.damage = 0;
			Item.knockBack = 7f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shootSpeed = 10f;
			Item.width = 18;
			Item.height = 28;
			Item.UseSound = SoundID.Item1;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.noMelee = true;
			Item.value = Item.sellPrice(0, 0, 80, 0);
			Item.rare = ItemRarityID.Green;
			Item.shootSpeed = 18f; // how quickly the hook is shot.
			Item.shoot = ModContent.ProjectileType<TentacleHookProj>();
		}
	}

	internal class TentacleHookProj : ModProjectile
	{
		public override string Texture => AssetDirectory.PermafrostItem + "TentacleHookProj";

		private List<Vector2> followPoints = new();

		private bool launching = true;

		private int timer = 0;

		private int stillTimer = 0;

		private Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tentacle Hook");
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst);
		}

		public override void AI()
		{
			if (launching)
			{
				if (timer++ % 6 == 0)
					followPoints.Add(Projectile.Center);

				if (timer < 120)
				{
					if (Projectile.Distance(Main.MouseWorld) > 25)
					{
						Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * Projectile.velocity.Length();
						stillTimer = 0;
					}
					else
					{
						Projectile.Center = Main.MouseWorld;
						stillTimer++;
						if (stillTimer > 15)
							timer = 121;
					}
				}
				else
				{
					Projectile.velocity = Owner.DirectionTo(Main.MouseWorld) * Projectile.velocity.Length();
				}
			}
		}

		public override bool? SingleGrappleHook(Player player)
		{
			return true;
		}

		public override float GrappleRange()
		{
			return 300f;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks)
		{
			numHooks = 1;
		}

		public override void GrappleRetreatSpeed(Player player, ref float speed)
		{
			launching = false;
			speed = 15f;
		}

		public override void GrapplePullSpeed(Player player, ref float speed)
		{
			speed = 18;
		}

		public override void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY)
		{
			launching = false;
			if (followPoints.Count < 1)
			{
				return;
			}

			grappleX = followPoints[0].X;
			grappleY = followPoints[0].Y;

			if (player.Distance(followPoints[0]) < 10)
				followPoints.RemoveAt(0);
		}

		public override bool PreDrawExtras()
		{
			Texture2D chainTexture = ModContent.Request<Texture2D>(Texture + "_Chain").Value;
			Texture2D neckTexture = ModContent.Request<Texture2D>(Texture + "_Neck").Value;
			Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 directionToPlayer = playerCenter - Projectile.Center;
			float chainRotation = directionToPlayer.ToRotation();
			float distanceToPlayer = directionToPlayer.Length();

			int chainNum = 0;
			while (distanceToPlayer > 20f && !float.IsNaN(distanceToPlayer))
			{
				directionToPlayer /= distanceToPlayer; // get unit vector
				directionToPlayer *= chainTexture.Height; // multiply by chain link length

				center += directionToPlayer; // update draw position
				directionToPlayer = playerCenter - center; // update distance
				distanceToPlayer = directionToPlayer.Length();

				Color drawColor = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16));

				// Draw chain
				if (chainNum == 0)
				{
					Main.EntitySpriteDraw(neckTexture, center - Main.screenPosition,
					neckTexture.Bounds, drawColor, chainRotation,
					neckTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
				}
				else
				{
					Main.EntitySpriteDraw(chainTexture, center - Main.screenPosition,
						chainTexture.Bounds, drawColor, chainRotation,
						chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
				}

				chainNum++;
			}

			return false;
		}
	}
}
