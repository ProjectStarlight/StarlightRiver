using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Armor.Slime
{
    [AutoloadEquip(EquipType.Head)]
    public class SlimeHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Head Slime");//temp names
            Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 10, 0);//todo
            item.rare = ItemRarityID.Green;
            item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
        }

        public override void AddRecipes()
        {
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class SlimeChest : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Body Slime");
            Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 10, 0);//todo
            item.rare = ItemRarityID.Green;
            item.defense = 3;
        }

        public override void UpdateEquip(Player player)
        {
        }

        public override void UpdateArmorSet(Player player)
        {
            player.slotsMinions += 1f;
        }

        public override void AddRecipes()
        {
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class SlimeLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Foot Slime");
            Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 10, 0);//todo
            item.rare = ItemRarityID.Green;
            item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
        }

        public override void AddRecipes()
        {
        }
    }
}