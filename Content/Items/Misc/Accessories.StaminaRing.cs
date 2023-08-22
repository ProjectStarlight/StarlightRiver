﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Vitric;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class StaminaRing : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public StaminaRing() : base("Band of Starlight", "+2 Starlight regeneration") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 90);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			AbilityHandler mp = Player.GetHandler();
			mp.StaminaRegenRate += 2;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<StaminaGel>(), 10);
			recipe.AddIngredient(ItemType<VitricOre>(), 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}