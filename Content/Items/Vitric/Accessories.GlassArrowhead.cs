using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class GlassArrowhead : SmartAccessory
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public GlassArrowhead() : base("Glass Arrowhead", "Critical strikes cause fired arrows to shatter into glass shards") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
		}

		public override void Unload()
		{
			StarlightPlayer.OnHitNPCWithProjEvent -= OnHitNPCWithProjAccessory;
		}

		private void OnHitNPCWithProjAccessory(Player Player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Player) && proj.arrow && info.Crit && Main.myPlayer == Player.whoAmI)
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 velocity = proj.velocity.RotatedByRandom(MathHelper.Pi / 6f);
					velocity *= Main.rand.NextFloat(0.5f, 0.75f);
					//Projectile.NewProjectile(Player.GetSource_Accessory(Item), proj.Center, velocity, ModContent.ProjectileType<Vitric.VitricArrowShattered>(), (int)(damage * 0.2f), knockback * 0.15f, Player.whoAmI);
					//TODO: Replace with new projectile
				}
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<VitricOre>(), 5);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();
		}
	}
}