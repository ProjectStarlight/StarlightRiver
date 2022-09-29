﻿using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Food
{
	internal class SeaSalt : Ingredient
    {
        public SeaSalt() : base("Food buffs are 10% more effective\nYou can breathe under water\nTest effect\nTest effect 2\nTest effect with a really long description so it will have to go on multiple lines", 1200, IngredientType.Seasoning) { }

        public override void SafeSetDefaults()
        {
            Item.createTile = TileType<Tiles.Cooking.SeaSalt>();
            Item.rare = ItemRarityID.Blue;
        }

        public override void BuffEffects(Player Player, float multiplier)
        {
            Player.GetModPlayer<FoodBuffHandler>().Multiplier += 0.1f;
            Player.gills = true;
        }
    }
}