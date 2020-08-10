using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Armor.Mecha
{
    [AutoloadEquip(EquipType.Head)]
    public class MechaHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Amylophite Helmet");
            Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Orange;
            item.defense = 5;
        }

        public override void UpdateEquip(Player player)
        {
        }

        public override void AddRecipes()
        {
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class MechaChest : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Amylophite Chestplate");
            Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Orange;
            item.defense = 10;
        }

        public override void UpdateEquip(Player player)
        {
        }

        public override void UpdateArmorSet(Player player)
        {
        }

        public override void AddRecipes()
        {
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class MechaLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Amylophite Boots");
            Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Orange;
            item.defense = 10;
        }

        public override void UpdateEquip(Player player)
        {
        }

        public override void AddRecipes()
        {
        }
    }
}