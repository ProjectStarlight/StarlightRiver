using StarlightRiver.Core;
using StarlightRiver.Items.Herbology.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using static Terraria.ModLoader.ModContent;

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
            Item.width = 28;
            Item.height = 28;
            Item.value = 8000;
            Item.defense = 2;
        }

        public override void UpdateEquip(Player Player) => Player.GetCritChance(DamageClass.Ranged) += 2;

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<Ivy>(), 8);
            recipe.AddTile(TileID.Anvils);
            recipe.Create();
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
            Item.width = 34;
            Item.height = 20;
            Item.value = 6000;
            Item.defense = 4;
        }

        public override void UpdateEquip(Player Player) => Player.GetCritChance(DamageClass.Ranged) += 2;

        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ModContent.ItemType<ForestIvyHead>() && legs.type == ModContent.ItemType<ForestIvyLegs>();

        // TODO: Forest ivy armor setbonus
        public override void UpdateArmorSet(Player Player)
        {
            Player.setBonus = "After 5 seconds of not taking damage, your next attack will ensnare and cause bleeding.";
            /*StarlightPlayer starlightPlayer = Player.GetModPlayer<StarlightPlayer>();
            starlightPlayer.ivyArmorComplete = true;*/
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Ivy>(), 12);
            recipe.AddTile(TileID.Anvils);
            recipe.Create();
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
            Item.width = 30;
            Item.height = 20;
            Item.value = 4000;
            Item.defense = 2;
        }

        public override void UpdateEquip(Player Player) => Player.moveSpeed += 0.2f;

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Ivy>(), 8);
            recipe.AddTile(TileID.Anvils);
            recipe.Create();
        }
    }
}