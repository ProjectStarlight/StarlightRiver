using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.AstralMeteor
{
	class AluminumArmor
    {
        [AutoloadEquip(EquipType.Head)]
        public class AluminumHead : ModItem
        {
            public override string Texture => AssetDirectory.AluminumItem + "AluminumHead";

            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Astral Aluminum Helmet");
                Tooltip.SetDefault("Scrapped Item");
            }

            public override void SetDefaults()
            {
                Item.width = 18;
                Item.height = 18;
                Item.value = Item.sellPrice(0, 0, 90, 0);
                Item.rare = ItemRarityID.Blue;
                Item.defense = 6;
            }

            public override void UpdateEquip(Player Player)
            {

            }

            public override bool IsArmorSet(Item head, Item body, Item legs)
            {
                return body.type == ItemType<AluminumChest>() && legs.type == ItemType<AluminumLegs>();
            }

            public override void UpdateArmorSet(Player Player)
            {
                Player.setBonus = "No description";
            }

            public override void AddRecipes()
            {
                //Recipe recipe = CreateRecipe();
                //recipe.AddIngredient(ItemType<AluminumBarItem>(), 10);
                //recipe.AddTile(TileID.Anvils);
            }
        }

        [AutoloadEquip(EquipType.Body)]
        public class AluminumChest : ModItem
        {
            public override string Texture => AssetDirectory.AluminumItem + "AluminumChest";

            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Astral Aluminum Suit");
                Tooltip.SetDefault("Scrapped Item");
            }

            public override void SetDefaults()
            {
                Item.width = 18;
                Item.height = 18;
                Item.value = Item.sellPrice(0, 0, 60, 0);
                Item.rare = ItemRarityID.Blue;
                Item.defense = 7;
            }

            public override void UpdateEquip(Player Player)
            {
            }

            public override void AddRecipes()
            {
                //Recipe recipe = CreateRecipe();
                //recipe.AddIngredient(ItemType<AluminumBarItem>(), 20);
                //recipe.AddTile(TileID.Anvils);
            }
        }

        [AutoloadEquip(EquipType.Legs)]
        public class AluminumLegs : ModItem
        {
            public override string Texture => AssetDirectory.AluminumItem + "AluminumLegs";

            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Astral Aluminum Leggings");
                Tooltip.SetDefault("Scrapped Item");
            }

            public override void SetDefaults()
            {
                Item.width = 18;
                Item.height = 18;
                Item.value = Item.sellPrice(0, 0, 60, 0);
                Item.rare = ItemRarityID.Blue;
                Item.defense = 5;
            }

            public override void UpdateEquip(Player Player)
            {

            }

            public override void AddRecipes()
            {
                //Recipe recipe = CreateRecipe();
                //recipe.AddIngredient(ItemType<AluminumBarItem>(), 15);
                //recipe.AddTile(TileID.Anvils);
            }
        }
    }
}
