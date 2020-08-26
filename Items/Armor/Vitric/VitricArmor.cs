using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Abilities;
using StarlightRiver.Projectiles.WeaponProjectiles;
using StarlightRiver.Items.Vitric;
using StarlightRiver.Tiles.Vitric.Blocks;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
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
            player.setBonus = "Gain 5% attack strength, lose 3 defense, and releaee protective shards for every 20% max health lost\nAfter taking a hit a shard breaks for 10 seconds";

            for (float k = 0.2f; k <= 0.8f; k += 0.2f)
            {
                if ((float)player.statLife / player.statLifeMax2 < k)
                {
                    player.statDefense -= 3;
                    player.BoostAllDamage(0.05f, 0);
                }
                if ((float)player.statLife / player.statLifeMax2 < k)
                {
                    if (!Main.projectile.Any(projectile => projectile.type == ProjectileType<VitricArmorProjectile>() && projectile.active && projectile.localAI[0] == (int)(k * 5) && projectile.owner == player.whoAmI))
                    {
                        int proj = Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<VitricArmorProjectile>(), 0, 0);
                        Main.projectile[proj].localAI[0] = (int)(k * 5);
                        Main.projectile[proj].ai[0] = 30;
                        Main.projectile[proj].owner = player.whoAmI;
                        Main.projectile[proj].netUpdate = true;
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

    public class VitricPlayer : ModPlayer
    {
        public float counter = 0;
        public int[] tablets = new int[3];
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            foreach (Projectile shard in Main.projectile.Where(proj => proj.active && proj.owner == player.whoAmI && proj.modProjectile != null && proj.modProjectile is VitricArmorProjectile))
            {
                VitricArmorProjectile moddedproj = shard.modProjectile as VitricArmorProjectile;
                if (moddedproj.projectile.ai[0] < 1)
                {
                    moddedproj.Shatter();
                    damage = (int)(damage*0.750f);
                    break;
                }
            }
            return true;
        }
        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            Action<PlayerDrawInfo> backTarget = s => DrawShards(s, false); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
            PlayerLayer backLayer = new PlayerLayer("VitricLayer", "Vitric Armor Effect", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
            layers.Insert(layers.IndexOf(layers.First()), backLayer); //Insert the layer at the appropriate index. 

            Action<PlayerDrawInfo> frontTarget = s => DrawShards(s, true); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
            PlayerLayer frontLayer = new PlayerLayer("VitricLayer", "Vitric Armor Effect", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
            layers.Insert(layers.IndexOf(layers.Last()), frontLayer); //Insert the layer at the appropriate index. 

            void DrawShards(PlayerDrawInfo info, bool back)
            {
                List<VitricArmorProjectile> allshard = new List<VitricArmorProjectile>();

                foreach (Projectile shard in Main.projectile.Where(proj => proj.active && proj.owner == player.whoAmI && proj.modProjectile != null && proj.modProjectile is VitricArmorProjectile))
                {
                    VitricArmorProjectile moddedproj = shard.modProjectile as VitricArmorProjectile;
                    allshard.Add(moddedproj);
                }
                allshard = allshard.OrderBy((x) => x.projectile.Center.Y).ToList();

                foreach (VitricArmorProjectile modshard in allshard)
                {
                    double angle = Math.Sin(-modshard.projectile.localAI[1]);
                    if ((angle > 0 && !back) ||
                        (angle <= 0 && back))
                    Main.playerDrawData.Add(modshard.Draw(player.Center));
                }
            }
        }
    }

}