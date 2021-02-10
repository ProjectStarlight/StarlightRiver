using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

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
                Tooltip.SetDefault("IDK lol");
            }

            public override void SetDefaults()
            {
                item.width = 18;
                item.height = 18;
                item.value = Item.sellPrice(0, 0, 90, 0);
                item.rare = ItemRarityID.Blue;
                item.defense = 6;
            }

            public override void UpdateEquip(Player player)
            {

            }

            public override bool IsArmorSet(Item head, Item body, Item legs)
            {
                return body.type == ItemType<AluminumChest>() && legs.type == ItemType<AluminumLegs>();
            }

            public override void UpdateArmorSet(Player player)
            {
                player.setBonus = "Cum. Thats all.";
            }

            public override void AddRecipes()
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemType<AluminumBar>(), 10);
                recipe.AddTile(TileID.Anvils);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        [AutoloadEquip(EquipType.Body)]
        public class AluminumChest : ModItem
        {
            public override string Texture => AssetDirectory.AluminumItem + "AluminumChest";

            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Astral Aluminum Suit");
                Tooltip.SetDefault("IDK lol");
            }

            public override void SetDefaults()
            {
                item.width = 18;
                item.height = 18;
                item.value = Item.sellPrice(0, 0, 60, 0);
                item.rare = ItemRarityID.Blue;
                item.defense = 7;
            }

            public override void UpdateEquip(Player player)
            {
            }

            public override void AddRecipes()
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemType<AluminumBar>(), 20);
                recipe.AddTile(TileID.Anvils);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }

        [AutoloadEquip(EquipType.Legs)]
        public class AluminumLegs : ModItem
        {
            public override string Texture => AssetDirectory.AluminumItem + "AluminumLegs";

            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Astral Aluminum Leggings");
                Tooltip.SetDefault("IDK lol");
            }

            public override void SetDefaults()
            {
                item.width = 18;
                item.height = 18;
                item.value = Item.sellPrice(0, 0, 60, 0);
                item.rare = ItemRarityID.Blue;
                item.defense = 5;
            }

            public override void UpdateEquip(Player player)
            {

            }

            public override void AddRecipes()
            {
                ModRecipe recipe = new ModRecipe(mod);
                recipe.AddIngredient(ItemType<AluminumBar>(), 15);
                recipe.AddTile(TileID.Anvils);
                recipe.SetResult(this);
                recipe.AddRecipe();
            }
        }
    }
}
