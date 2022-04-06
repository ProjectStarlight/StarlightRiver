using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

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
			Item.damage = 30;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 65;
			Item.useAnimation = 65;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Orange;
			Item.shoot = ModContent.ProjectileType<SkullBusterProj>();
			Item.shootSpeed = 12f;
			Item.useAmmo = AmmoID.Bullet;
			Item.autoReuse = true;
		}
		public override void HoldItem(Player Player)
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
				EntitySource_ItemUse source = new EntitySource_ItemUse(Player, Item);
				shotCounter = 0;
				reloadCounter = 120;
				Terraria.Audio.SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(Mod, "Sounds/Guns/RevolvingReload"), Player.Center);
				Item.noUseGraphic = true;
				Projectile.NewProjectile(source, Player.Center, Vector2.Zero, ModContent.ProjectileType<SkullBusterReload>(), 0, 0, Player.whoAmI);
			}
		}
		public override bool CanUseItem(Player Player)
		{
			if (reloadCounter > 0)
			{
				return false;
			}
			Item.noUseGraphic = false;
			return base.CanUseItem(Player);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(Mod, "Sounds/Custom/Revolver"));
			if (shotCounter < 1)
			{
				Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SkullBusterProj>(), damage, knockback, player.whoAmI);
				shotsHit = 0;
			}
			shotCounter++;
			Vector2 direction = velocity;
			direction = direction.RotatedBy(spread);
			int proj = Projectile.NewProjectile(source, position, direction, type, damage, knockback, player.whoAmI);
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
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 999999;
			Projectile.ignoreWater = true;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			Player Player = Main.player[Projectile.owner];
			Player.ChangeDir(Main.MouseWorld.X > Player.position.X ? 1 : -1);
			Projectile.Center = Player.Center;

			direction = Main.MouseWorld - (Player.Center);
			Player.itemTime = Player.itemAnimation = 8;
			if (Player.HeldItem.ModItem is SkullBuster Item)
			{
				direction = direction.RotatedBy(Item.recoil);

				if (Main.mouseLeft && !leftClick) //Change to check if its myPlayer holding mouseleft later
				{
					leftClick = true;
					Player.itemTime = 0;
					if (Item.shotCounter >= 3)
					{
						Projectile.active = false;
					}
				}
				
				if (!Main.mouseLeft)              //Change to check if its myPlayer holding mouseleft later
					leftClick = false;

				if (Item.shotCounter >= 4)
				{
					Projectile.active = false;
				}

				Player.itemRotation = direction.ToRotation();
				if (Player.direction != 1)
				{
					Player.itemRotation -= 3.14f;
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
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 999999;
			Projectile.ignoreWater = true;
			Projectile.alpha = 255;
			Main.projFrames[Projectile.type] = 22;
		}
		public override void AI()
		{
			Player Player = Main.player[Projectile.owner];
			Player.ChangeDir(Main.MouseWorld.X > Player.position.X ? 1 : -1);
			Player.itemTime = Player.itemAnimation = 2;
			direction = Main.MouseWorld - (Player.Center);
			direction.Normalize();
			direction*= 15;
			Player.itemRotation = direction.ToRotation();
			Player.heldProj = Projectile.whoAmI;
			Projectile.Center = Player.Center;
			if (Player.direction != 1)
			{
				Player.itemRotation -= 3.14f;
			}
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 5)
			{
				Projectile.frame++;
				Projectile.frameCounter = 0;
				if (Projectile.frame >= Main.projFrames[Projectile.type])
				{
					Projectile.active = false;
					if (Player.HeldItem.ModItem is SkullBuster Item)
					{
						Item.reloadCounter = 0;
					}
				}
			}
			if (Player.HeldItem.ModItem is SkullBuster Item2 && Item2.reloadCounter <= 0)
			{
				Projectile.active = false;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Player Player = Main.player[Projectile.owner];
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int height = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
			int y2 = height * Projectile.frame;
			if (Player.direction == 1)
			{
				SpriteEffects effects1 = SpriteEffects.None;
				Vector2 position = (Projectile.position + (0.5f * direction) + new Vector2((float) Projectile.width, (float) Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), lightColor, direction.ToRotation(), new Vector2((float) texture.Width / 8f, (float) height * .75f), Projectile.scale, effects1, 0.0f);
			}
			else
			{
				SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
				Vector2 position = (Projectile.position - (0.5f * direction) + new Vector2((float) Projectile.width, (float) Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), lightColor, direction.ToRotation() - 3.14f, new Vector2((float) texture.Width / 8f, (float) height * .75f), Projectile.scale, effects1, 0.0f); 
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