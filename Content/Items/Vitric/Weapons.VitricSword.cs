using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	internal class VitricSword : ModItem
	{
		public bool broken = false;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ancient Vitric Blade");
			Tooltip.SetDefault("Shatters into enchanted glass shards \nUnable to be used while shattered");
		}

		public override void SetDefaults()
		{
			Item.damage = 35;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 38;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 7.5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;
			Item.useTurn = true;
		}

		public override bool? CanHitNPC(Player Player, NPC target)
		{
			return !broken;
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!broken)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item107);
				Projectile.NewProjectile(player.GetSource_ItemUse(Item), target.Center, Vector2.Normalize(player.Center - target.Center) * -32, ModContent.ProjectileType<VitricSwordProjectile>(), 24, 0, player.whoAmI);
				Projectile.NewProjectile(player.GetSource_ItemUse(Item), target.Center, Vector2.Normalize(player.Center - target.Center).RotatedBy(0.3) * -16, ModContent.ProjectileType<VitricSwordProjectile>(), 24, 0, player.whoAmI);
				Projectile.NewProjectile(player.GetSource_ItemUse(Item), target.Center, Vector2.Normalize(player.Center - target.Center).RotatedBy(-0.25) * -24, ModContent.ProjectileType<VitricSwordProjectile>(), 24, 0, player.whoAmI);

				for (int k = 0; k <= 20; k++)
				{
					Dust.NewDust(Vector2.Lerp(player.Center, target.Center, 0.4f), 8, 8, ModContent.DustType<Dusts.Air>(), (Vector2.Normalize(player.Center - target.Center) * -2).X, (Vector2.Normalize(player.Center - target.Center) * -2).Y);

					float vel = Main.rand.Next(-300, -100) * 0.1f;
					int dus = Dust.NewDust(Vector2.Lerp(player.Center, target.Center, 0.4f), 16, 16, ModContent.DustType<Dusts.GlassAttracted>(), (Vector2.Normalize(player.Center - target.Center) * vel).X, (Vector2.Normalize(player.Center - target.Center) * vel).Y);
					Main.dust[dus].customData = player;
				}

				broken = true;
			}
		}

		public override bool CanUseItem(Player Player)
		{
			if (Main.projectile.Any(Projectile => Projectile.type == ModContent.ProjectileType<VitricSwordProjectile>() && Projectile.owner == Player.whoAmI && Projectile.active))
				return false;
			else
				broken = false;

			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<SandstoneChunk>(), 8);
			recipe.AddIngredient(ModContent.ItemType<VitricOre>(), 12);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class VitricSwordProjectile : ModProjectile
	{
		private float progress = 1;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = false;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Enchanted Glass");
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27);
		}

		public override void AI()
		{
			progress += 0.1f;
			Player Player = Main.player[Projectile.owner];
			Projectile.position += Vector2.Normalize(Player.Center - Projectile.Center) * progress;
			Projectile.velocity *= 0.94f;
			Projectile.rotation = (Player.Center - Projectile.Center).Length() * 0.1f;

			if ((Player.Center - Projectile.Center).Length() <= 32 && Projectile.timeLeft < 110)
			{
				Projectile.timeLeft = 0;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item101);
			}
		}
	}
}