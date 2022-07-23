using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Vitric
{
	public class IgnitionGauntlets : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public int handCounter = 0;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
			Tooltip.SetDefault("Rapidly barrages enemies with your fists while building Ignition Charge\n" +
							   "Hold RMB to consume charge, propelling yourself forward at escape velocity\n" +
							   "Attack while still flying to vent remaining charge in a large blast cone|n" +
							   "'Heroes always arrive from the skies'");
		}

		public override void SetDefaults()
		{
			Item.damage = 32;
			Item.DamageType = DamageClass.Melee;
			Item.width = 5;
			Item.height = 5;
			Item.useTime = 3;
			Item.useAnimation = 3;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 7;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.shoot = ModContent.ProjectileType<IgnitionPunchPhantom>();
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
		}

		public override void HoldItem(Player player)
		{
			IgnitionPlayer modPlayer = player.GetModPlayer<IgnitionPlayer>();
			Lighting.AddLight(player.Center, Color.OrangeRed.ToVector3() * modPlayer.charge / 150f * Main.rand.NextFloat());

			if (Main.rand.NextBool(2) && modPlayer.charge - modPlayer.potentialCharge > 0)
			{
				Dust dust = Dust.NewDustPerfect(player.Center, ModContent.DustType<IgnitionChargeDustPassive>(), default, default, Color.OrangeRed);
				dust.customData = player.whoAmI;
				dust.scale = Main.rand.NextFloat(0.25f, 0.45f);
				dust.alpha = Main.rand.Next(100);

				if (modPlayer.charge - modPlayer.potentialCharge  >= 75)
					dust.alpha += 100;

				if (modPlayer.charge - modPlayer.potentialCharge >= 150)
					dust.alpha += 100;
			}
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2)
			{
				if (player.GetModPlayer<IgnitionPlayer>().charge > 20 && player.ownedProjectileCounts[ModContent.ProjectileType<IgnitionGauntletCharge>()] == 0)
					Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletCharge>(), damage, knockback, player.whoAmI);

				return false;
			}
			if (player.ownedProjectileCounts[ModContent.ProjectileType<IgnitionGauntletCharge>()] == 0)
			{
				IgnitionPlayer modPlayer = player.GetModPlayer<IgnitionPlayer>();

				if (modPlayer.loadedCharge > 20)
				{
					float damagelerper = (modPlayer.loadedCharge - 15) / 135f;
					damagelerper = MathHelper.Max(damagelerper, 0.5f);

					Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletCone>(), (int)(damage * 4 * damagelerper), knockback, player.whoAmI, 1);

					damagelerper = (float)Math.Sqrt(damagelerper);
					damagelerper = 1;

					modPlayer.loadedCharge = 20;
					modPlayer.flipping = true;

					for (int i = 0; i < 30; i++)
					{
						var pos = position;
						float lerper = Main.rand.NextFloat();
						Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<IgnitionGauntletSmoke3>(), Vector2.Normalize(player.DirectionTo(Main.MouseWorld)).RotatedByRandom(0.8f) * 50.5f * lerper * damagelerper);
						dust.scale = Main.rand.NextFloat(0.75f, 1.25f) * MathHelper.Lerp(0.4f, 1f, lerper) * damagelerper;
						dust.alpha = Main.rand.Next(50);
						dust.rotation = Main.rand.NextFloatDirection();
					}

					for (int k = 0; k < 12; k++)
					{
						Dust.NewDustPerfect(position + new Vector2(0, 35), ModContent.DustType<IgnitionGauntletSpark>(), Vector2.Normalize(player.DirectionTo(Main.MouseWorld)).RotatedByRandom(1.2f) * Main.rand.Next(3, 30) * damagelerper, 0, Color.Yellow, 2.4f * damagelerper); ;
					}

					for (int k = 0; k < 12; k++)
					{
						Dust.NewDustPerfect(position + new Vector2(0, 35), ModContent.DustType<IgnitionGauntletSpark>(), Vector2.Normalize(player.DirectionTo(Main.MouseWorld)).RotatedByRandom(0.5f) * Main.rand.Next(3, 15) * damagelerper, 0, Color.Yellow, 1.3f * damagelerper);
					}

					Vector2 handOffset = new Vector2(-10 * player.direction, -16);
					handOffset = handOffset.RotatedBy(player.fullRotation);
					Dust star = Dust.NewDustPerfect(handOffset + player.Center, ModContent.DustType<IgnitionGauntletStar>(), player.DirectionTo(Main.MouseWorld) * 5);
					star.scale = 1.5f;
					star.rotation = Main.GameUpdateCount * 0.085f;

					Projectile proj = Projectile.NewProjectileDirect(source, position, player.DirectionTo(Main.MouseWorld) * 10 * damagelerper, ModContent.ProjectileType<IgnitionGauntletsImpactRing>(), 0, 0, player.whoAmI, Main.rand.Next(150, 250) * damagelerper, player.DirectionTo(Main.MouseWorld).ToRotation());
					var mp = proj.ModProjectile as IgnitionGauntletsImpactRing;
					mp.timeLeftStart = 50;
					proj.timeLeft = 50;
					proj.extraUpdates = 3;

					player.velocity *= -0.75f;
					player.itemTime = player.itemAnimation = 20;
					player.direction *= Math.Sign(player.Center.Y - Main.MouseWorld.Y);
					Core.Systems.CameraSystem.Shake += (int)(12 * damagelerper);
					return false;
				}
			}
			/*if (player.velocity.Length() < 6 && !(player.controlUp || player.controlDown || player.controlLeft || player.controlRight))
            {
				Vector2 dir = player.DirectionTo(Main.MouseWorld) * 0.6f;
            }*/
			handCounter++;

			if (handCounter % 2 == 0)
			{
				Projectile proj = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, (handCounter % 4) / 2);
				var mp = proj.ModProjectile as IgnitionPunchPhantom;
				mp.directionVector = player.DirectionTo(Main.MouseWorld).RotatedByRandom(0.2f);

				int offsetFactor = Main.rand.Next(10, 20);
				int offset = Main.rand.Next(-20, 20);
				Vector2 vel = new Vector2(7, offset * 0.4f);
				Vector2 offsetV = new Vector2(Main.rand.Next(-offsetFactor, offsetFactor), offset);
				float rot = position.DirectionTo(Main.MouseWorld).ToRotation();
				Projectile.NewProjectileDirect(source, position + offsetV.RotatedBy(rot), vel.RotatedBy(rot), ModContent.ProjectileType<IgnitionPunch>(), damage, knockback, player.whoAmI, (handCounter % 4) / 2);
			}
			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HellstoneBar, 10);
			recipe.AddIngredient(ModContent.ItemType<SandstoneChunk>(), 8);
			recipe.AddIngredient(ModContent.ItemType<MagmaCore>(), 2);
			recipe.AddTile(TileID.Anvils);
		}
	}

	public class IgnitionPlayer : ModPlayer
	{
		public int charge = 0;

		public int potentialCharge = 0;

		public int loadedCharge = 0;
		public bool launching = false;
		public bool flipping = false;
		public float acceleration;

		private int rotationCounter = 0;

		public override void PreUpdate() //TODO: some rotdifference shenanagins here to make the rotation transition smoother
		{
			if (launching)
			{
				loadedCharge--;
				if (loadedCharge <= 0)
				{
					launching = false;
					Player.fullRotation = 0;
					flipping = false;
					acceleration = 0;
					return;
				}



				if (acceleration < 1)
					acceleration += 0.02f;
				float lerper = MathHelper.Min(rotationCounter / 8f, loadedCharge / 20f);

				if (!flipping)
				{
					Core.Systems.CameraSystem.Shake = (int)(2 * lerper);
					Lighting.AddLight(Player.Center, Color.OrangeRed.ToVector3());
					for (int i = 0; i < 4; i++)
					{
						var pos = (Player.Center + new Vector2(9, 9)) - (Player.velocity * Main.rand.NextFloat(2));
						Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<IgnitionGauntletSmoke>(), Vector2.Normalize(-Player.velocity).RotatedByRandom(0.6f) * Main.rand.NextFloat(6.5f));
						dust.scale = Main.rand.NextFloat(0.35f, 0.75f);
						dust.alpha = Main.rand.Next(50);
						dust.rotation = Main.rand.NextFloatDirection();
					}

					for (int j = 0; j < 16; j++)
					{
						var pos = (Player.Center + new Vector2(9, 9)) - (Player.velocity * Main.rand.NextFloat(2));
						Dust dust2 = Dust.NewDustPerfect(pos, ModContent.DustType<IgnitionGauntletSmoke2>(), Vector2.Normalize(-Player.velocity).RotatedByRandom(3.0f) * Main.rand.NextFloat(12.5f));
						dust2.scale = Main.rand.NextFloat(0.25f, 0.45f);
						dust2.alpha = Main.rand.Next(50);
						dust2.rotation = Main.rand.NextFloatDirection();
					}

					var pos2 = (Player.Center + new Vector2(9, 9)) - (Player.velocity * Main.rand.NextFloat(2));
					Dust dust3 = Dust.NewDustPerfect(pos2, ModContent.DustType<IgnitionGlowDust>(), Vector2.Normalize(-Player.velocity).RotatedByRandom(2.6f) * Main.rand.NextFloat(6.5f), 0, Color.OrangeRed);
					dust3.scale = Main.rand.NextFloat(0.35f, 1.35f);
					dust3.alpha = Main.rand.Next(50);

					Vector2 offset = Vector2.Normalize(Player.velocity.RotatedBy(1.57f)) * Main.rand.Next(-30, 30);
					Dust dust4 = Dust.NewDustPerfect(Player.Center + offset + (Player.velocity * 5), ModContent.DustType<IgnitionGauntletWind>(), Vector2.Normalize(-Player.velocity) * Main.rand.NextFloat(6.5f), 0, Color.White, 1.5f);
					dust4.rotation = dust4.velocity.ToRotation();
					dust4.position -= (dust4.rotation - 1.57f).ToRotationVector2() * 35;
					Player.velocity = Vector2.Lerp(Player.velocity, Player.DirectionTo(Main.MouseWorld) * 20 * (float)Math.Sqrt(lerper), 0.15f * acceleration);
				}
				Player.fullRotationOrigin = Player.Size / 2;

				Player.fullRotation = Player.DirectionTo(Main.MouseWorld).ToRotation() + 1.57f;
				if (flipping)
					Player.fullRotation += 6.28f;
				Player.fullRotation *= lerper;

				if (rotationCounter < 8)
					rotationCounter++;

			}
			else
			{
				rotationCounter = 0;
			}
		}

		public override void PostUpdate()
		{
			if (launching && !flipping)
				Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, 1.57f * Player.direction);
		}
		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
		{
			if (launching)
				return false;

			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
		}
	}
}