using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vanity
{
	[AutoloadEquip(EquipType.Head)]
    public class MechaHead : ModItem
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mecha Helmet");//temp names
            //Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 10, 0);//todo
            item.rare = ItemRarityID.Green;
            item.vanity = true;
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
        public override string Texture => AssetDirectory.VanityItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mecha Chest");
            //Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 10, 0);//todo
            item.rare = ItemRarityID.Green;
            item.vanity = true;
        }

        public override void UpdateEquip(Player player)
        {
        }

        public override void UpdateArmorSet(Player player)
        {
            //player.slotsMinions += 1f;
        }

        public override void AddRecipes()
        {
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class MechaLegs : ModItem
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mecha Legs");
            //Tooltip.SetDefault("Placeholder");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 10, 0);
            item.rare = ItemRarityID.Green;
            item.vanity = true;
        }

        public override void UpdateEquip(Player player)
        {
        }

        public override void AddRecipes()
        {
        }
    }
}