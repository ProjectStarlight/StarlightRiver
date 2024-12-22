using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Bosses.BrainRedux;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Projectiles;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	public class BearPoker : ModItem
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bear Poker");
			Tooltip.SetDefault("This needle is sharp enough to penetrate a thinker's cocoon\nThis is a terrible idea");
		}

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Generic;
			Item.width = 32;
			Item.height = 32;
			Item.damage = 1;
			Item.crit = -4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Orange;
			Item.shoot = ModContent.ProjectileType<BearPokerProjectile>();
			Item.shootSpeed = 20;
			Item.UseSound = SoundID.Item1;
			Item.consumable = true;

			Item.value = Item.sellPrice(copper: 1);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (!NPC.downedBoss3 && !Main.hardMode)
			{
				var warningLine = new TooltipLine(Mod, "Warning", "Warning: This encounter may exceed your current capabilities")
				{
					OverrideColor = new Color(255, 150, 150)
				};

				tooltips.Add(warningLine);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 5);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}

	public class BearPokerProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.CrimsonItem + "BearPoker";

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.width = 38;
			Projectile.height = 38;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return target.type == ModContent.NPCType<TheThinker>();
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.4f;
			Projectile.velocity.X *= 0.99f;
			Projectile.rotation = Projectile.velocity.ToRotation() + 3.14f * 1.25f;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.type == ModContent.NPCType<TheThinker>())
				Projectile.active = false;
		}

		public override void OnKill(int timeLeft)
		{
			if (Main.myPlayer == Projectile.owner)
				Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<BearPoker>());

			for(int k = 0; k < 20; k++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
			}

			SoundEngine.PlaySound(SoundID.Tink, Projectile.Center);
		}
	}

	public class BearPokerEndless : BearPoker
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gilded Bear Poker");
			Tooltip.SetDefault("This needle is sharp enough to penetrate a thinker's cocoon\nThis is a terrible idea\nInfinite uses");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.consumable = false;
			Item.shoot = ModContent.ProjectileType<BearPokerEndlessProjectile>();
		}
	}

	public class BearPokerEndlessProjectile : BearPokerProjectile
	{
		public override void OnKill(int timeLeft)
		{
			for (int k = 0; k < 20; k++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
			}

			SoundEngine.PlaySound(SoundID.Tink, Projectile.Center);
		}
	}
}
