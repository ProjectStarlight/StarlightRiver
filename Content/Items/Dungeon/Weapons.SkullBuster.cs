using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Audio;
using StarlightRiver.Sounds.Custom;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class SkullBuster : ModItem
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;

		public int shotsHit = 0;
		public int shotCounter = 0;
		public int reloadCounter = 0;
		public float recoil = 0;
		public float spread = 0;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skullbuster");
			Tooltip.SetDefault("Striking enemies with all four bullets creates a fifth guarunteed crit bullet\nSlaying an enemy with this empowered shot instantly reloads\n'Four in the chamber'");

		}

		public override void SetDefaults()
		{
			item.damage = 21;
			item.ranged = true;
			item.width = 24;
			item.height = 24;
			item.useTime = 65;
			item.useAnimation = 65;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 0;
			item.rare = ItemRarityID.Orange;
			item.shoot = ModContent.ProjectileType<SkullBusterProj>();
			item.shootSpeed = 12f;
			item.useAmmo = AmmoID.Bullet;
			item.autoReuse = true;
		}
		public override void HoldItem(Player player)
		{
			reloadCounter--;
			recoil *= 0.965f;
			spread *= 0.97f;
			if (spread > 0.1f)
				spread -= 0.02f;
			if (spread < -0.1f)
				spread += 0.02f;
			if (recoil > 0.01f)
				recoil -= 0.01f;
			if (recoil < -0.01f)
				recoil += 0.01f;
			if (shotCounter >= 4)
			{
				shotCounter = 0;
				reloadCounter = 120;
				Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Guns/RevolvingReload"), player.Center);
				item.noUseGraphic = true;
				Projectile.NewProjectile(player.Center,Vector2.Zero, ModContent.ProjectileType<SkullBusterReload>(), 0, 0, player.whoAmI);
			}
		}
		public override bool CanUseItem(Player player)
		{
			if (reloadCounter > 0)
			{
				return false;
			}
			item.noUseGraphic = false;
			return base.CanUseItem(player);
		}
		public override bool Shoot(Player player, ref Microsoft.Xna.Framework.Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/Revolver"));
			if (shotCounter < 1)
			{
				Projectile.NewProjectile(position, new Vector2(speedX,speedY), ModContent.ProjectileType<SkullBusterProj>(), damage, knockBack, player.whoAmI);
				shotsHit = 0;
			}
			shotCounter++;
			Vector2 direction = new Vector2(speedX,speedY);
			direction = direction.RotatedBy(spread);
			int proj = Projectile.NewProjectile(position, direction, type, damage, knockBack, player.whoAmI);
			Main.projectile[proj].GetGlobalProjectile<SkullBusterGlobalProj>().shotFromGun = true;
			recoil = 0.8f * (player.direction * -1);
			spread = Main.rand.NextFloat(-1.5f, 1.5f);
			return false;
		}

	}
	public class SkullBusterProj : ModProjectile
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;

		public Vector2 direction = Vector2.Zero;

		public bool leftClick = true;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skull Buster");
		}

		public override void SetDefaults()
		{
			projectile.hostile = false;
			projectile.ranged = true;
			projectile.width = 2;
			projectile.height = 2;
			projectile.aiStyle = -1;
			projectile.friendly = false;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.timeLeft = 999999;
			projectile.ignoreWater = true;
			projectile.alpha = 255;
		}

		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
			projectile.Center = player.Center;

			direction = Main.MouseWorld - (player.Center);
			player.itemTime = player.itemAnimation = 8;
			if (player.HeldItem.modItem is SkullBuster item)
			{
				direction = direction.RotatedBy(item.recoil);

				if (Main.mouseLeft && !leftClick) //Change to check if its myplayer holding mouseleft later
				{
					leftClick = true;
					player.itemTime = 0;
					if (item.shotCounter >= 3)
					{
						projectile.active = false;
					}
				}
				
				if (!Main.mouseLeft)              //Change to check if its myplayer holding mouseleft later
					leftClick = false;

				if (item.shotCounter >= 4)
				{
					projectile.active = false;
				}

				player.itemRotation = direction.ToRotation();
				if (player.direction != 1)
				{
					player.itemRotation -= 3.14f;
				}
			}
		}
	}

	public class SkullBusterReload : ModProjectile
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;

		public Vector2 direction = Vector2.Zero;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skull Buster Reload");
		}

		public override void SetDefaults()
		{
			projectile.hostile = false;
			projectile.ranged = true;
			projectile.width = 2;
			projectile.height = 2;
			projectile.aiStyle = -1;
			projectile.friendly = false;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.timeLeft = 999999;
			projectile.ignoreWater = true;
			projectile.alpha = 255;
			Main.projFrames[projectile.type] = 22;
		}
		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
			player.itemTime = player.itemAnimation = 2;
			direction = Main.MouseWorld - (player.Center);
			direction.Normalize();
			direction*= 15;
			player.itemRotation = direction.ToRotation();
			player.heldProj = projectile.whoAmI;
			projectile.Center = player.Center;
			if (player.direction != 1)
			{
				player.itemRotation -= 3.14f;
			}
			projectile.frameCounter++;
			if (projectile.frameCounter >= 5)
			{
				projectile.frame++;
				projectile.frameCounter = 0;
				if (projectile.frame >= Main.projFrames[projectile.type])
				{
					projectile.active = false;
					if (player.HeldItem.modItem is SkullBuster item)
					{
						item.reloadCounter = 0;
					}
				}
			}
			if (player.HeldItem.modItem is SkullBuster item2 && item2.reloadCounter <= 0)
			{
				projectile.active = false;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Player player = Main.player[projectile.owner];
			Texture2D texture = Main.projectileTexture[projectile.type];
			int height = Main.projectileTexture[projectile.type].Height / Main.projFrames[projectile.type];
			int y2 = height * projectile.frame;
			if (player.direction == 1)
			{
				SpriteEffects effects1 = SpriteEffects.None;
				Vector2 position = (projectile.position + (0.5f * direction) + new Vector2((float) projectile.width, (float) projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition).Floor();
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), lightColor, direction.ToRotation(), new Vector2((float) texture.Width / 8f, (float) height * .75f), projectile.scale, effects1, 0.0f);
			}
			else
			{
				SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
				Vector2 position = (projectile.position - (0.5f * direction) + new Vector2((float) projectile.width, (float) projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition).Floor();
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), lightColor, direction.ToRotation() - 3.14f, new Vector2((float) texture.Width / 8f, (float) height * .75f), projectile.scale, effects1, 0.0f); 
			}
			return false;
		}
	}
	public class SkullBusterGlobalProj : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		public bool shotFromGun = false;
		public bool superBullet = false;
	}	
}