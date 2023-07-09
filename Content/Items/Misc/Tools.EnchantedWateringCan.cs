using StarlightRiver.Core.Systems.AuroraWaterSystem;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	class EnchantedWateringCan : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Watering Can");
			Tooltip.SetDefault("todo");
		}

		public override void SetDefaults()
		{
			Item.width = 36;
			Item.height = 38;

			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 20;

			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ModContent.ProjectileType<EnchantedWateringCanProj>();
			Item.shootSpeed = 14f;

			Item.autoReuse = false;
			Item.noUseGraphic = true;
			Item.noMelee = true;

			Item.value = Item.sellPrice(0, 1, 50, 0);
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.IronBar, 30)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	internal class EnchantedWateringCanProj : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem + "EnchantedWateringCan";

		Player Owner => Main.player[Projectile.owner];	
		private bool FirstTickOfSwing => Projectile.ai[0] == 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Watering Can");
			//Main.projFrames[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(72, 38);
			//Projectile.penetrate = -1;
			//Projectile.ownerHitCheck = true;
			//Projectile.extraUpdates = 3;
		}

		public override void AI()
		{
			if (FirstTickOfSwing)
			{
				Projectile.spriteDirection = Owner.direction;
			}
			Projectile.velocity = Vector2.Zero;
			Owner.heldProj = Projectile.whoAmI;

			Vector2 armOffset = Main.GetPlayerArmPosition(Projectile) - Owner.Center;
			Projectile.Center = Owner.Center + armOffset * new Vector2(4.25f, 1.25f);

			Projectile.rotation = (float)Math.Sin(Projectile.ai[0] * 0.175f * Projectile.spriteDirection /* * 0*/) * 0.3f - 
				(Math.Abs(armOffset.Y) - 6 - Owner.gfxOffY * 1.5f/*slightly fixes rotating when stepping up blocks*/) * 0.05f * -Projectile.spriteDirection;
			//version that points the upper angle downwards instead: //((Math.Abs(direction.Y) - 6) * 0.05f * -Projectile.spriteDirection);

			Projectile.ai[0]++;

			if (Projectile.ai[0] > 28)
				Projectile.active = false;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			//these 2 values are for getting the right rotation point
			float Xoffset = Projectile.spriteDirection == 1 ? (tex.Width * 0.125f) : (tex.Width * 0.875f);
			Vector2 rotPoint = new Vector2(Xoffset, frameHeight * 0.4f);

			Main.spriteBatch.Draw(tex, 
					Projectile.position + 
					rotPoint + 
					new Vector2(Projectile.width / 2 - tex.Width / 2 - 0.6f,//weird specific offset to make it symetrical
					 -Owner.gfxOffY * 1.5f) - //this value is weird and sometimes breaks for no reason?
					Main.screenPosition, 
				frameBox, 
				lightColor, 
				Projectile.rotation, 
				rotPoint, 
				Projectile.scale, 
				Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 
				0f);

			return false;
		}
	}
}