using StarlightRiver.Core;
using StarlightRiver.Items.Herbology.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.ForestIvy
{
    [AutoloadEquip(EquipType.Head)]
    public class ForestIvyHead : ModItem
    {
        public override string Texture => AssetDirectory.IvyItem + "ForestIvyHead";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Forest Ivy Helm");
            Tooltip.SetDefault("2% increased ranged critical strike change");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.value = 8000;
            item.defense = 2;
        }

        public override void UpdateEquip(Player player) => player.rangedCrit += 2;

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Ivy>(), 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class ForestIvyChest : ModItem
    {
        public override string Texture => AssetDirectory.IvyItem + "ForestIvyChest";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Forest Ivy Chestplate");
            Tooltip.SetDefault("2% increased ranged critical strike change");
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 20;
            item.value = 6000;
            item.defense = 4;
        }

        public override void UpdateEquip(Player player) => player.rangedCrit += 2;

        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ModContent.ItemType<ForestIvyHead>() && legs.type == ModContent.ItemType<ForestIvyLegs>();

        // TODO: Forest ivy armor setbonus
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "After 5 seconds of not taking damage, your next attack will ensnare and cause bleeding.";
            /*StarlightPlayer starlightPlayer = player.GetModPlayer<StarlightPlayer>();
            starlightPlayer.ivyArmorComplete = true;*/
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Ivy>(), 12);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class ForestIvyLegs : ModItem
    {
        public override string Texture => AssetDirectory.IvyItem + "ForestIvyLegs";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Forest Ivy Leggings");
            Tooltip.SetDefault("Slightly increased movement speed");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
            item.value = 4000;
            item.defense = 2;
        }

        public override void UpdateEquip(Player player) => player.moveSpeed += 0.2f;

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Ivy>(), 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}