//TODO:
//Neck
//Sellprice
//Rarity
//Visuals
//Clean up code

using ReLogic.Content;
using StarlightRiver.Core.Systems.AuroraWaterSystem;
using System.Collections.Generic;
using System.Threading;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Permafrost
{
	class TentacleHook : ModItem
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
			Item.useStyle = 5;
			Item.shootSpeed = 10f;
			Item.width = 18;
			Item.height = 28;
			Item.UseSound = SoundID.Item1;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.rare = 1;
			Item.noMelee = true;
			Item.value = 20000;
			Item.shootSpeed = 12f; // how quickly the hook is shot.
			Item.shoot = ModContent.ProjectileType<TentacleHookProj>();
		}
	}

	internal class TentacleHookProj : ModProjectile
	{
		public override string Texture => AssetDirectory.PermafrostItem + "TentacleHookProj";

		private static Asset<Texture2D> chainTexture;

		private List<Vector2> followPoints = new List<Vector2>();

		private bool pulling = true;

		private int timer = 0;

		private Player Owner => Main.player[Projectile.owner];

		public override void Load()
		{ // This is called once on mod (re)load when this piece of content is being loaded.
		  // This is the path to the texture that we'll use for the hook's chain. Make sure to update it.
			chainTexture = ModContent.Request<Texture2D>(Texture + "_Chain");
		}

		public override void Unload()
		{ // This is called once on mod reload when this piece of content is being unloaded.
		  // It's currently pretty important to unload your static fields like this, to avoid having parts of your mod remain in memory when it's been unloaded.
			chainTexture = null;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tentacle Hook");
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.GemHookAmethyst); // Copies the attributes of the Amethyst hook's projectile.
		}

		public override void AI()
		{
			if (pulling)
			{
				if (timer++ % 3 == 0)
					followPoints.Add(Projectile.Center);

				if (timer < 120)
					Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * Projectile.velocity.Length();
				else
					Projectile.velocity = Owner.DirectionTo(Main.MouseWorld) * Projectile.velocity.Length();
			}
		}

		// Return true if it is like: Hook, CandyCaneHook, BatHook, GemHooks
		 public override bool? SingleGrappleHook(Player player)
		 {
			return true;
		 }

		// Use this to kill oldest hook. For hooks that kill the oldest when shot, not when the newest latches on: Like SkeletronHand
		// You can also change the projectile like: Dual Hook, Lunar Hook
		// public override void UseGrapple(Player player, ref int type)
		// {
		//	int hooksOut = 0;
		//	int oldestHookIndex = -1;
		//	int oldestHookTimeLeft = 100000;
		//	for (int i = 0; i < 1000; i++)
		//	{
		//		if (Main.projectile[i].active && Main.projectile[i].owner == projectile.whoAmI && Main.projectile[i].type == projectile.type)
		//		{
		//			hooksOut++;
		//			if (Main.projectile[i].timeLeft < oldestHookTimeLeft)
		//			{
		//				oldestHookIndex = i;
		//				oldestHookTimeLeft = Main.projectile[i].timeLeft;
		//			}
		//		}
		//	}
		//	if (hooksOut > 1)
		//	{
		//		Main.projectile[oldestHookIndex].Kill();
		//	}
		// }

		// Amethyst Hook is 300, Static Hook is 600.
		public override float GrappleRange()
		{
			return 400f;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks)
		{
			numHooks = 1; // The amount of hooks that can be shot out
		}

		// default is 11, Lunar is 24
		public override void GrappleRetreatSpeed(Player player, ref float speed)
		{
			pulling = false;
			speed = 15f; // How fast the grapple returns to you after meeting its max shoot distance
		}

		public override void GrapplePullSpeed(Player player, ref float speed)
		{
			speed = 10; // How fast you get pulled to the grappling hook projectile's landing position
		}

		// Adjusts the position that the player will be pulled towards. This will make them hang 50 pixels away from the tile being grappled.
		public override void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY)
		{
			if (followPoints.Count < 1)
			{
				return;
			}
			grappleX = followPoints[0].X;
			grappleY = followPoints[0].Y;

			if (player.Distance(followPoints[0]) < 10)
				followPoints.RemoveAt(0);
		}

		// Draws the grappling hook's chain.
		public override bool PreDrawExtras()
		{
			Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 directionToPlayer = playerCenter - Projectile.Center;
			float chainRotation = directionToPlayer.ToRotation();
			float distanceToPlayer = directionToPlayer.Length();

			int chainNum = 0;
			while (distanceToPlayer > 20f && !float.IsNaN(distanceToPlayer))
			{
				directionToPlayer /= distanceToPlayer; // get unit vector
				directionToPlayer *= chainTexture.Height(); // multiply by chain link length

				center += directionToPlayer; // update draw position
				directionToPlayer = playerCenter - center; // update distance
				distanceToPlayer = directionToPlayer.Length();

				Color drawColor = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16));

				// Draw chain
				if (chainNum == 0)
				{
					Main.EntitySpriteDraw(ModContent.Request<Texture2D>(Texture + "_Neck").Value, center - Main.screenPosition,
					chainTexture.Value.Bounds, drawColor, chainRotation,
					chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
				}
				else
				{
					Main.EntitySpriteDraw(chainTexture.Value, center - Main.screenPosition,
						chainTexture.Value.Bounds, drawColor, chainRotation,
						chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
				}
				chainNum++;
			}
			// Stop vanilla from drawing the default chain.
			return false;
		}
	}
}
