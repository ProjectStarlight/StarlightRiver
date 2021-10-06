using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class Buzzsaw : ModItem
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Buzzsaw");
			Tooltip.SetDefault("Hit enemies to build up pressure \nRelease to launch buzzsaw");
		}

		public override void SetDefaults()
		{
			item.damage = 30;
			item.width = 65;
			item.height = 21;
			item.useTime = 65;
			item.useAnimation = 65;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 1.5f;
			item.value = Item.sellPrice(0, 10, 0, 0);
			item.rare = ItemRarityID.Red;
			item.autoReuse = false;
			item.shoot = ModContent.ProjectileType<BuzzsawProj>();
			item.shootSpeed = 20f;
			item.melee = true;
			item.channel = true;
			item.noUseGraphic = true;
			item.UseSound = SoundID.DD2_SkyDragonsFuryShot;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-15, 0);
	}
	public class BuzzsawProj : ModProjectile
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public Vector2 direction = Vector2.Zero;

		private const int OFFSET = 30;
		private const int MAXCHARGE = 20;

		private int counter;

		private int charge;
		public override void SetStaticDefaults() => DisplayName.SetDefault("Buzzsaw");

		public override void SetDefaults()
		{
			projectile.hostile = false;
			projectile.melee = true;
			projectile.width = 32;
			projectile.height = 32;
			projectile.aiStyle = -1;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.timeLeft = 999999;
			projectile.ignoreWater = true;
			projectile.alpha = 255;
			Main.projFrames[projectile.type] = 5;
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);

			player.itemTime = 5; // Set item time to 2 frames while we are used
			player.itemAnimation = 5; // Set item animation time to 2 frames while we are used
			direction = Main.MouseWorld - (player.Center);
			direction.Normalize();
			projectile.Center = player.Center + (direction * OFFSET);
			projectile.velocity = Vector2.Zero;
			player.itemRotation = direction.ToRotation();
			player.heldProj = projectile.whoAmI;

			if (player.direction != 1)
				player.itemRotation -= 3.14f;

			if (player.channel)
			{
				projectile.timeLeft = 2;
				counter++;
				projectile.frame = ((counter / 5) % 2) + 2;
			}
			else
            {
				LaunchSaw(player);
				projectile.active = false;
            }
		}

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			if (charge < MAXCHARGE)
				charge++;
			hitDirection = Math.Sign(direction.X);
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) //extremely messy code I ripped from a weapon i made for spirit :trollge:
		{
			Player player = Main.player[projectile.owner];
			Texture2D texture = Main.projectileTexture[projectile.type];
			int height = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
			int y2 = height * projectile.frame;
			Vector2 position = (projectile.position - (0.5f * (direction * OFFSET)) + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition).Floor();

			if (player.direction == 1)
			{
				SpriteEffects effects1 = SpriteEffects.None;
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), lightColor, direction.ToRotation(), new Vector2((float)texture.Width / 2f, (float)height / 2f), projectile.scale, effects1, 0.0f);
			}
			else
			{
				SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), lightColor, direction.ToRotation() - 3.14f, new Vector2((float)texture.Width / 2f, (float)height / 2f), projectile.scale, effects1, 0.0f);
			}

			return false;
		}

		private void LaunchSaw(Player player)
        {

        }
	}
}