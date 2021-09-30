using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
    [AutoloadEquip(EquipType.Head)]
    public class MoonstoneHead : ModItem
    {
        private static Item dummySpear = new Item();

        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonstone Helmet");
            Tooltip.SetDefault("2% increased melee critical strike chance\n+20 barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 1;
        }

		public override void UpdateEquip(Player player)
		{
            player.meleeCrit += 2;
            player.GetModPlayer<ShieldPlayer>().MaxShield += 20;

            dummySpear.SetDefaults(ItemType<Datsuzei>());
        }

		public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<MoonstoneChest>() && legs.type == ItemType<MoonstoneLegs>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = ("Accumulate lunar energy by dealing melee damage\ndouble tap DOWN to summon the legendary spear Datsuzei\nDatsuzei consumes lunar energy and dissapears at zero");
        }

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
            var player = Main.LocalPlayer;

            if (IsArmorSet(player.armor[0], player.armor[1], player.armor[2]))
            {
                if (!player.controlUp)
                {
                    TooltipLine spearQuery = new TooltipLine(mod, "StarlightRiver:ArmorSpearQuery", "hold SHIFT for Datsuzei stats");
                    spearQuery.overrideColor = new Color(200, 200, 200);

                    tooltips.Add(spearQuery);
                }

				else
				{
                    TooltipLine spearName = new TooltipLine(mod, "StarlightRiver:ArmorSpearName", "\n Datsuzei");
                    spearName.overrideColor = new Color(100, 255, 255);

                    tooltips.Add(spearName);

                    for (int k = 0; k < dummySpear.ToolTip.Lines; k++)
                        tooltips.Add(new TooltipLine(mod, "StarlightRiver:SpearTooltips", " " + dummySpear.ToolTip.GetLine(k)));
                }
            }
		}

		public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<MoonstoneBar>(), 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class MoonstoneChest : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonstone Chestpiece");
            Tooltip.SetDefault("+35 barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 3;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<ShieldPlayer>().MaxShield += 35;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<MoonstoneBar>(), 15);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class MoonstoneLegs : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonstone Greaves");
            Tooltip.SetDefault("Improved acceleration\n +25 barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 5;
        }

        public override void UpdateEquip(Player player)
        {
            player.runAcceleration *= 1.5f;
            player.GetModPlayer<ShieldPlayer>().MaxShield += 25;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<MoonstoneBar>(), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}