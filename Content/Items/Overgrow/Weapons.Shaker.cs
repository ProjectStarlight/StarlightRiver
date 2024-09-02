﻿using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Overgrow
{
	internal class Shaker : ModItem
	{
		public bool lifting;

		public override string Texture => AssetDirectory.OvergrowItem + "Shaker";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("The Shaker");
			StarlightPlayer.PostUpdateEvent += DoLiftAnimation;
			StarlightPlayer.ResetEffectsEvent += ResetLiftAnimation;
		}

		private void DoLiftAnimation(Player player)
		{
			ModItem instance = player.HeldItem.ModItem;

			if (instance is Shaker)
			{
				if ((instance as Shaker).lifting)
					player.bodyFrame = new Rectangle(0, 56 * 5, 40, 56);
			}
		}

		private void ResetLiftAnimation(StarlightPlayer modPlayer)
		{
			ModItem instance = modPlayer.Player.HeldItem.ModItem;

			if (instance is Shaker)
			{
				if ((instance as Shaker).lifting)
					lifting = false;
			}
		}

		public override void SetDefaults()
		{
			Item.damage = 100;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 20;
			Item.useTime = 60;
			Item.useAnimation = 1;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 4;
			Item.rare = ItemRarityID.Orange;
			Item.channel = true;
			Item.noUseGraphic = true;
		}

		public override bool CanUseItem(Player player)
		{
			return !Main.projectile.Any(n => n.active && Main.player[n.owner] == player && n.type == ProjectileType<ShakerBall>());
		}

		public override bool? UseItem(Player player)
		{
			int proj = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.position + new Vector2(0, -32), Vector2.Zero, ProjectileType<ShakerBall>(), Item.damage, Item.knockBack);
			Main.projectile[proj].owner = player.whoAmI;

			return true;
		}

		public override void HoldItem(Player Player)
		{
			if (Player.channel)
			{
				Player.velocity.X *= 0.95f;
				Player.jump = -1;
				lifting = true;
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.FirstOrDefault(tooltip => tooltip.Name == "Speed" && tooltip.Mod == "Terraria").Text = "Snail Speed";
		}
	}

	internal class ShakerBall : ModProjectile
	{
		public override string Texture => AssetDirectory.OvergrowItem + Name;

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shaker");
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (Projectile.timeLeft < 2)
				Projectile.timeLeft = 2;

			Projectile.scale = Timer < 10 ? (Timer / 10f) : 1;
			Projectile.damage = (int)(Timer * 1.2f * player.GetDamage(DamageClass.Melee).Multiplicative);

			if (Timer == 100)
				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f));

			if (State == 0) //charging/holding
				Projectile.position = player.Top + (Projectile.position - Projectile.Bottom);

			if (State == 0 && Timer < 100) //charge up
			{
				Timer++;

				if (Timer == 100) //full charge FX
				{
					for (int k = 0; k <= 100; k++)
					{
						Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
					}

					Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath7, Projectile.Center);
				}

				Projectile.velocity = Vector2.Zero;

				float rot = Main.rand.NextFloat(6.28f);
				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * 35, DustType<Dusts.GoldWithMovement>(), -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, Timer / 100f);
			}

			if (!player.channel && Timer > 10 && State == 0) //throw if enough charge
			{
				if (player == Main.LocalPlayer)
					Projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * Timer * 0.1f;

				Projectile.tileCollide = true;
				Projectile.friendly = true;
				State = 1;

				Projectile.netUpdate = true;
			}

			if (State == 1) //thrown/falling
			{
				Projectile.velocity.Y += 0.4f;

				if (Projectile.velocity.Y == 0.4f) //when it hits the ground
				{
					Projectile.velocity *= 0;
					Projectile.timeLeft = 120;
					State = 2;

					CameraSystem.shake += (int)(Timer * 0.2f);

					for (int k = 0; k <= 100; k++)
					{
						Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 32), DustType<Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * Timer / 10f);
					}

					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42, Projectile.Center);
				}
			}

			if (State == 2) //retracting
			{
				Projectile.velocity += -Vector2.Normalize(Projectile.Center - player.Center) * 0.1f;

				if (Projectile.velocity.Length() >= 5)
					State = 3;

				if (Vector2.Distance(Projectile.Center, player.Center) <= 30)
					Projectile.timeLeft = 0;

				if (Projectile.timeLeft == 3)
					State = 4;
			}

			if (State == 3) //retracting faster
			{
				Projectile.velocity = -Vector2.Normalize(Projectile.Center - player.Center) * 5;
				Projectile.velocity.Y += 3;

				if (Vector2.Distance(Projectile.Center, player.Center) <= 30)
					Projectile.timeLeft = 0;

				if (Projectile.timeLeft == 3)
					State = 4;
			}

			if (State == 4) //retracting even faster and phasing
			{
				Projectile.velocity = -Vector2.Normalize(Projectile.Center - player.Center) * 18;
				Projectile.tileCollide = false;

				if (Vector2.Distance(Projectile.Center, player.Center) <= 30)
					Projectile.timeLeft = 0;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player Player = Main.player[Projectile.owner];

			if (State != 0)
			{
				Texture2D chainTex = Assets.Items.Overgrow.ShakerChain.Value;

				for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(Player.Center, Projectile.Center) / 16))
				{
					Vector2 pos = Vector2.Lerp(Projectile.Center, Player.Center + new Vector2(0, Main.player[Projectile.owner].gfxOffY), k) - Main.screenPosition;
					Main.spriteBatch.Draw(chainTex, pos, null, lightColor, (Projectile.Center - Player.Center).ToRotation() + 1.58f, chainTex.Size() / 2, 1, 0, 0);
				}
			}

			Vector2 ballPos = Projectile.Center - Main.screenPosition;

			if (State == 0)
				ballPos += new Vector2(0, Player.gfxOffY);

			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, ballPos, TextureAssets.Projectile[Projectile.type].Value.Frame(), Color.White, Projectile.rotation, Projectile.Size / 2, Projectile.scale, 0, 0);

			return false;
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			if (State == 0)
			{
				float colormult = Timer / 100f * 0.7f;
				float scale = 1.2f - Timer / 100f * 0.5f;
				Texture2D tex = Assets.Tiles.Interactive.WispSwitchGlow2.Value;
				Vector2 pos = (Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur();
				spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * colormult, 0, tex.Size() / 2, scale, 0, 0);
			}

			if (Timer == 100)
			{
				Texture2D tex = Assets.Tiles.Interactive.WispSwitchGlow2.Value;
				Vector2 pos = (Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur();
				spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - StarlightWorld.visualTimer) * 0.2f, 0, tex.Size() / 2, StarlightWorld.visualTimer * 0.17f, 0, 0);
				spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.visualTimer + 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.visualTimer + 3.14f) * 0.17f, 0, 0);
				spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.visualTimer - 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.visualTimer - 3.14f) * 0.17f, 0, 0);
			}
		}
	}
}