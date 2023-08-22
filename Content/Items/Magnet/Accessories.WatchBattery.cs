using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Manabonds;
using StarlightRiver.Helpers;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Magnet
{
	internal class WatchBattery : SmartAccessory
	{
		public override string Texture => AssetDirectory.MagnetItem + Name;

		public WatchBattery() : base("Watch Battery", "Your sentries shock nearby enemies when placed") { }

		public override void Load()
		{
			StarlightProjectile.PostAIEvent += SpawnShock;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}

		private void SpawnShock(Projectile projectile)
		{
			Player owner = Main.player[projectile.owner];

			if (owner != null && Equipped(owner) && owner == Main.LocalPlayer)
			{
				if (projectile.timeLeft == (Projectile.SentryLifeTime - 1))
				{
					NPC target = projectile.TargetClosestNPC(400, true, false);

					if (target != null)
					{
						Shock.parentToAssign = projectile;
						Shock.initialTargetToAssign = target;
						Projectile.NewProjectileDirect(Item.GetSource_FromThis(), projectile.Center, Vector2.Zero, ModContent.ProjectileType<Shock>(), 5, 0, projectile.owner);
					}
				}
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<ChargedMagnet>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}