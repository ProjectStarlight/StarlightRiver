using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Underground
{
	internal class GloomGauntlet : SmartAccessory
	{
		public override string Texture => "StarlightRiver/Assets/Items/Underground/GloomGauntlet";

		public GloomGauntlet() : base("Gloom Gauntlet", "Gain melee damage based on the speed of your held weapon\nSlower weapons grant more damage") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			if (player.HeldItem.DamageType == DamageClass.Melee && player.HeldItem.damage > 0)
				player.GetDamage(DamageClass.Melee) += player.HeldItem.useTime / 60f * 0.5f;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.FeralClaws, 1);
			recipe.AddIngredient(ModContent.ItemType<GloomGel>(), 25);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}