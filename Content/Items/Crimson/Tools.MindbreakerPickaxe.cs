using StarlightRiver.Content.Items.Vitric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class MindbreakerPickaxe : ModItem
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Mindbreaker Pickaxe");
			Tooltip.SetDefault("Can mine gray matter");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.pick = 101;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.Orange;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 30);
			recipe.AddIngredient(ModContent.ItemType<ImaginaryTissue>(), 15);
			recipe.AddIngredient(ItemID.DeathbringerPickaxe);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
