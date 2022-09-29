﻿using StarlightRiver.Core;
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
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 0, 10, 0);//todo
            Item.rare = ItemRarityID.Green;
            Item.vanity = true;
        }

        public override void UpdateEquip(Player Player)
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
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 0, 10, 0);//todo
            Item.rare = ItemRarityID.Green;
            Item.vanity = true;
        }

        public override void UpdateEquip(Player Player)
        {
        }

        public override void UpdateArmorSet(Player Player)
        {
            //Player.slotsMinions += 1f;
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
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Green;
            Item.vanity = true;
        }

        public override void UpdateEquip(Player Player)
        {
        }

        public override void AddRecipes()
        {
        }
    }
}