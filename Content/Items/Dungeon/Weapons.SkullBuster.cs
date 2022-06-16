using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using StarlightRiver.Content.Items.Misc;

namespace StarlightRiver.Content.Items.Dungeon
{
	public class SkullBuster : ModItem
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;

		private int cooldown = 0;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skullbuster");
			Tooltip.SetDefault("Right click to throw 4 skullbombs \nRelease right click to shoot them all in quick succession");

		}

		public override void SetDefaults()
		{
			Item.damage = 30;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 35;
			Item.useAnimation = 35;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.shoot = ModContent.ProjectileType<SkullBusterProj>();
			Item.shootSpeed = 12f;
			Item.useAmmo = AmmoID.Bullet;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<CoachGun>(), 1);
			recipe.AddIngredient(ItemID.Bone, 10);
			recipe.AddTile(TileID.Anvils);
		}

		public override void HoldItem(Player Player)
		{
			cooldown--;
		}

		public override bool CanUseItem(Player Player)
		{
			if (Player.altFunctionUse == 2)
			{
				Item.useStyle = ItemUseStyleID.Swing;
				Item.noUseGraphic = true;

				Item.useTime = 15;
				Item.useAnimation = 15;
				if (cooldown > 0)
					return false;
			}
			else
			{
				Item.useTime = 35;
				Item.useAnimation = 35;
				Item.useStyle = ItemUseStyleID.Shoot;
				Item.noUseGraphic = false;
			}
			return base.CanUseItem(Player);
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-6, 0);
		}

		public override bool AltFunctionUse(Player Player)
		{
			return true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				Vector2 dir = Vector2.Normalize(velocity) * 9;
				velocity = dir;
				type = ModContent.ProjectileType<SkullBomb>();
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				for (int i = 0; i < 4; i++)
                {
					Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.8f, 1.3f), type, damage, knockback, player.whoAmI);
                }
				Projectile proj = Projectile.NewProjectileDirect(source, position, Vector2.Zero, ModContent.ProjectileType<SkullBusterProj>(), damage, knockback, player.whoAmI);
				(proj.ModProjectile as SkullBusterProj).baseItem = Item;
				cooldown = 130;
			}
			else
			{
				float rot = velocity.ToRotation();
				float spread = 0.4f;

				Vector2 offset = new Vector2(1, -0.1f * player.direction).RotatedBy(rot);
				Vector2 casingOffset = new Vector2(1, -1 * player.direction).RotatedBy(rot);

				for (int k = 0; k < 15; k++)
				{
					var direction = offset.RotatedByRandom(spread);

					Dust.NewDustPerfect(position + (offset * 43), ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
				}

				Helper.PlayPitched("Guns/PlinkLever", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Helper.PlayPitched("Guns/RifleLight", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), position);
				Dust.NewDustPerfect(player.Center + offset * 43, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + offset.RotatedByRandom(spread) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));

				Projectile proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center + offset * 43, velocity * 2, type, damage, knockback, player.whoAmI);

				Projectile.NewProjectile(player.GetSource_ItemUse(Item), position + (offset * 43), Vector2.Zero, ModContent.ProjectileType<CoachGunMuzzleFlash>(), 0, 0, player.whoAmI, rot);

				Gore.NewGore(source, player.Center + (casingOffset * 10), new Vector2(player.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunCasing").Type, 1f);
			}
			return false;
		}
	}

	public class SkullBusterProj : ModProjectile
    {
		public override string Texture => AssetDirectory.DungeonItem + "SkullBuster";

		private Player owner => Main.player[Projectile.owner];

		public Item baseItem = default;

		private bool released = false;

		private int shootTimer = 0;

		private Vector2 direction = Vector2.One;

		private List<Projectile> shotBombs = new List<Projectile>();

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skull Buster");
		}
		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.damage = 0;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 5;
			Projectile.friendly = false;
		}

        public override void AI()
        {
			Projectile.Center = owner.Center;
			owner.heldProj = Projectile.whoAmI;

            if (Main.mouseRight && !released || owner.itemTime > 3)
            {
				Projectile.timeLeft = 30;
				if (owner.itemTime < 3)
					owner.itemTime = owner.itemAnimation = 3;
            }
			else
            {
				released = true;
				if (shootTimer % 7 == 0)
                {
					var targetBomb = Main.projectile.Where(x => x.active && x.owner == owner.whoAmI && x.type == ModContent.ProjectileType<SkullBomb>() && !shotBombs.Contains(x)).OrderBy(n => n.Distance(owner.Center)).FirstOrDefault();
					if (targetBomb != default)
                    {
						owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f);
						float spread = 0.4f;
						Projectile.timeLeft = 30;


						direction = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f).DirectionTo(targetBomb.Center) * 24;
						if (direction.X > 0)
							owner.direction = 1;
						else
							owner.direction = -1;

						Vector2 offset = new Vector2(1, -0.3f * owner.direction).RotatedBy(direction.ToRotation());

						Vector2 casingOffset = new Vector2(1, -1 * owner.direction).RotatedBy(direction.ToRotation());
						Vector2 position = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f);

						int ammoType = -1;
						for (int i = 0; i < owner.inventory.Length; ++i) //Consume ammo here so it's used when shot rather than when clicked
						{
							if (owner.inventory[i].ammo == AmmoID.Bullet)
							{
								if (owner.inventory[i].consumable && VanillaAmmoConsumption(owner, owner.inventory[i].ammo)) //Do not consume ammo if possible
								{
									owner.inventory[i].stack--;
									if (owner.inventory[i].stack <= 0)
										owner.inventory[i].TurnToAir();
								}
								ammoType = owner.inventory[i].shoot;
								break;
							}
						}

						if (ammoType != -1)
						{
							Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_ItemUse_WithPotentialAmmo(baseItem, AmmoID.Bullet), position + (offset * 43), direction, ammoType, 0, Projectile.knockBack, owner.whoAmI);
							proj.GetGlobalProjectile<SkullBusterGProj>().target = targetBomb.whoAmI;
							proj.extraUpdates = 4;
							shotBombs.Add(targetBomb);

							Vector2 gunTip = position + (offset * 46);

							for (int k = 0; k < 15; k++)
							{
								var direction = offset.RotatedByRandom(spread);

								Dust.NewDustPerfect(gunTip, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(8), 125, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
							}

							Helper.PlayPitched("Guns/PlinkLever", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), position);
							Helper.PlayPitched("Guns/RifleLight", 0.7f, Main.rand.NextFloat(-0.1f, 0.1f), position);
							Dust.NewDustPerfect(gunTip, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + offset.RotatedByRandom(spread) * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));


							Projectile.NewProjectile(Projectile.GetSource_FromThis(), gunTip, Vector2.Zero, ModContent.ProjectileType<CoachGunMuzzleFlash>(), 0, 0, owner.whoAmI, direction.ToRotation());

							Gore.NewGore(Projectile.GetSource_FromThis(), position + (casingOffset * 10), new Vector2(owner.direction * -1, -0.5f) * 2, Mod.Find<ModGore>("CoachGunCasing").Type, 1f);
						}
					}
					else if (shootTimer == 0)
                    {
						Projectile.active = false;
                    }
                }

				if (direction.X > 0)
					owner.direction = 1;
				else
					owner.direction = -1;
				bool facingRight = owner.direction == 1;
				owner.itemTime = owner.itemAnimation = 2;
				owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f);
				shootTimer++;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (released)
            {
				float rot = direction.ToRotation();
				Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
				Vector2 origin = new Vector2(10, tex.Height * 0.75f);
				SpriteEffects effects = SpriteEffects.None;
				if (owner.direction != 1)
                {
					rot += 3.14f;
					effects = SpriteEffects.FlipHorizontally;
					origin.X = tex.Width - origin.X;
                }
				Main.spriteBatch.Draw(tex, owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, direction.ToRotation() - 1.57f) - Main.screenPosition, null, lightColor, rot, origin, Projectile.scale, effects, 0f);
            }
			return false;
        }
		public static bool VanillaAmmoConsumption(Player p, int ammo)
		{
			float chance = 0;

			float CombineChances(float p1, float p2) => p1 + p2 - (p1 * p2);

			if (p.ammoBox) //1/5 chance to reduce
				chance = 0.2f;

			if (p.ammoCost75) //1/4 chance to reduce
				chance = CombineChances(chance, 0.25f);

			if (p.ammoCost80) //1/5 chance to reduce
				chance = CombineChances(chance, 0.2f);

			if (p.ammoPotion) //1/5 chance to reduce
				chance = CombineChances(chance, 0.2f);

			if (ammo == AmmoID.Arrow && p.magicQuiver) //1/5 chance to reduce for arrows only
				chance = CombineChances(chance, 0.2f);

			if (p.armor[0].type == ItemID.ChlorophyteHelmet) //1/5 chance to reduce -- seems unique??
				chance = CombineChances(chance, 0.2f);

			return Main.rand.NextFloat(1f) > chance;
		}

	}

	public class SkullBomb : ModProjectile
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;

		private bool shot = false;

		private Player owner => Main.player[Projectile.owner];

		private float crosshairRotation = 0f;

		private float crosshairSin = 0f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Skull Bomb");
		}
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Shuriken);
			Projectile.width = 18;
			Projectile.damage = 0;
			Projectile.height = 18;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 150;
			Projectile.aiStyle = 14;
			Projectile.friendly = false;
		}

		public override void AI()
		{ 

			if (crosshairSin < 1f)
				crosshairSin += 0.025f;

			crosshairSin = MathHelper.Clamp(crosshairSin, 0, 1);
			crosshairRotation += 0.05f * crosshairSin;
			float progress = 1 - (Projectile.timeLeft / 150f);
			for (int i = 0; i < 2; i++)
			{
				Dust sparks = Dust.NewDustPerfect(Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2()) * 12, ModContent.DustType<CoachGunSparks>(), (Projectile.rotation + Main.rand.NextFloat(-0.6f, 0.6f)).ToRotationVector2() * Main.rand.NextFloat(0.4f, 1.2f));
				sparks.fadeIn = progress * 45;
			}

			Rectangle Hitbox = new Rectangle((int)Projectile.Center.X - 50, (int)Projectile.Center.Y - 50, 100, 100);
			var list = Main.projectile.Where(x => x.Hitbox.Intersects(Hitbox));
			foreach (var proj in list)
			{
				if (proj.GetGlobalProjectile<SkullBusterGProj>().target == Projectile.whoAmI && Projectile.timeLeft > 2 && proj.active)
				{
					shot = true;
					Projectile.timeLeft = 2;
					proj.active = false;
				}
			}
		}

		public override void Kill(int timeLeft)
		{
			if (!Main.dedServ)
            {
				DustHelper.DrawDustImage(Projectile.Center, 33, 0.078f, ModContent.Request<Texture2D>(Texture + "_Skull" + Main.rand.Next(1, 4).ToString()).Value, 3, true, Main.rand.NextFloat(-0.2f, 0.2f));
            }

			for (int i = 0; i < 3; i++)
            {
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(7, 7), ProjectileID.Bone, Projectile.damage, Projectile.knockBack, owner.whoAmI);
				proj.friendly = true;
				proj.hostile = false;
				proj.scale = 0.75f;
            }
		}

        public override bool PreDraw(ref Color lightColor)
        {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D whiteTex = ModContent.Request<Texture2D>(Texture + "_White").Value;
			Texture2D crosshairTex = ModContent.Request<Texture2D>(Texture + "_Crosshair").Value;
			
			float progress = 1 - (Projectile.timeLeft / 150f);
			Color overlayColor = Color.White;
			if (progress < 0.5f)
				overlayColor = Color.Lerp(new Color(0, 0, 0, 0), Color.Gray * 0.5f, progress * 2);
			else
				overlayColor = Color.Lerp(Color.Gray * 0.5f, Color.White, (progress - 0.5f) * 2);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(whiteTex, Projectile.Center - Main.screenPosition, null, overlayColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			for (int i = 0; i < 4; i++)
			{
				float rot = (i / 4f) * 6.28f;

				float ease = EaseFunction.EaseQuinticIn.Ease(crosshairSin);
				Vector2 origin = crosshairTex.Size() * (1.75f + (0.25f * (float)Math.Cos(ease * 3.14f)));
				Main.spriteBatch.Draw(crosshairTex, Projectile.Center - Main.screenPosition, null, Color.Red * crosshairSin, crosshairRotation + rot, origin, 1, SpriteEffects.None, 0f);
			}

			return false;
        }
    }

	public class SkullBusterGProj : GlobalProjectile
    {
		public override bool InstancePerEntity => true;

		public int target = -1;
    }
}