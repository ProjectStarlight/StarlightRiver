using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using StarlightRiver.Items.Vitric;
using StarlightRiver.Tiles.Vitric.Blocks;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Armor.Vitric
{
    [AutoloadEquip(EquipType.Head)]
    public class VitricHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Headgear");
            Tooltip.SetDefault("10% increased ranged damage");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 4;
        }

        public override void UpdateEquip(Player player)
        {
            player.rangedDamage += 0.1f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == mod.ItemType("VitricChest") && legs.type == mod.ItemType("VitricLegs");
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Gain up to 12 defense at high HP\nRelease glass shards at low HP";

            for (float k = 0.2f; k <= 0.8f; k += 0.2f)
            {
                if ((float)player.statLife / player.statLifeMax2 > k)
                {
                    player.statDefense += 3;


                }
                if ((float)player.statLife / player.statLifeMax2 < k)
                {
                    if (!Main.projectile.Any(projectile => projectile.type == ProjectileType<Projectiles.WeaponProjectiles.VitricArmorProjectile>() && projectile.active && projectile.localAI[0] == (int)(k * 5) && projectile.owner == player.whoAmI))
                    {
                        int proj = Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<Projectiles.WeaponProjectiles.VitricArmorProjectile>(), 15, 0);
                        Main.projectile[proj].localAI[0] = (int)(k * 5);
                        Main.projectile[proj].owner = player.whoAmI;
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<VitricGem>(), 10);
            recipe.AddIngredient(ItemType<VitricSandItem>(), 20);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class VitricChest : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Chestpiece");
            Tooltip.SetDefault("5% increased ranged critical strike chance");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 6;
        }

        public override void UpdateEquip(Player player)
        {
            player.rangedCrit += 5;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<VitricGem>(), 10);
            recipe.AddIngredient(ItemType<VitricSandItem>(), 20);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class VitricLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Greaves");
            Tooltip.SetDefault("Slightly improved stamina regeneration");
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
            player.GetModPlayer<AbilityHandler>().StatStaminaRegenMax -= 20;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<VitricGem>(), 10);
            recipe.AddIngredient(ItemType<VitricSandItem>(), 20);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}