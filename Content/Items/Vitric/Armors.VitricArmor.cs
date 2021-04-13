using Microsoft.Xna.Framework;
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

namespace StarlightRiver.Content.Items.Vitric
{
    [AutoloadEquip(EquipType.Head)]
    public class VitricHead : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

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

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreHurtEvent += SetBonusPrehurt;
            return true;
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
            player.setBonus = "Gain 5% attack strength, lose 3 defense, and release protective shards for every 20% max health lost\nAfter taking a hit a shard breaks for 10 seconds";

            for (float k = 0.2f; k <= 0.8f; k += 0.2f)
                if ((float)player.statLife / player.statLifeMax2 < k)
                {
                    player.statDefense -= 3;
                    player.BoostAllDamage(0.05f, 0);

                    int index = (int)(k * 5);

                    Projectile findproj = Main.projectile.FirstOrDefault(projectile => projectile.type == ProjectileType<VitricArmorProjectile>() && projectile.active && projectile.localAI[0] == index && projectile.owner == player.whoAmI);

                    if (findproj == default)
                    {
                        int proj = Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<VitricArmorProjectile>(), 0, 0);
                        Main.projectile[proj].localAI[0] = index;
                        Main.projectile[proj].ai[0] = 30;
                        Main.projectile[proj].owner = player.whoAmI;
                        Main.projectile[proj].netUpdate = true;
                    }
                    else
                        findproj.ai[1] = 3;
                }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 5);
            recipe.AddIngredient(ItemType<VitricOre>(), 15);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        private bool SetBonusPrehurt(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (player.armor[0].type == ItemType<VitricHead>() && player.armor[1].type == ItemType<VitricChest>() && player.armor[2].type == ItemType<VitricLegs>())//Better way to do this?
                foreach (Projectile shard in Main.projectile.Where(proj => proj.active && proj.owner == player.whoAmI && proj.modProjectile != null && proj.modProjectile is VitricArmorProjectile))
                {
                    VitricArmorProjectile moddedproj = shard.modProjectile as VitricArmorProjectile;
                    if (moddedproj.projectile.ai[0] < 1)
                    {
                        moddedproj.Shatter();
                        damage = (int)(damage * 0.750f);
                        break;
                    }
                }
            return true;
        }

    }

    [AutoloadEquip(EquipType.Body)]
    public class VitricChest : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

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
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 5);
            recipe.AddIngredient(ItemType<VitricOre>(), 25);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class VitricLegs : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

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
            player.GetHandler().StaminaRegenRate += 0.1f;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 5);
            recipe.AddIngredient(ItemType<VitricOre>(), 20);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class VitricArmorPlayer : ModPlayer
    {
        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            void backTarget(PlayerDrawInfo s) => DrawShards(s, false); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
            PlayerLayer backLayer = new PlayerLayer("VitricLayer", "Vitric Armor Effect", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
            layers.Insert(layers.IndexOf(layers.First()), backLayer); //Insert the layer at the appropriate index. 

            void frontTarget(PlayerDrawInfo s) => DrawShards(s, true); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
            PlayerLayer frontLayer = new PlayerLayer("VitricLayer", "Vitric Armor Effect", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
            layers.Insert(layers.IndexOf(layers.Last()), frontLayer); //Insert the layer at the appropriate index. 

            void DrawShards(PlayerDrawInfo info, bool back)
            {
                List<VitricArmorProjectile> allshards = new List<VitricArmorProjectile>();

                foreach (Projectile shard in Main.projectile.Where(proj => proj.active && proj.owner == player.whoAmI && proj.modProjectile != null && proj.modProjectile is VitricArmorProjectile))
                {
                    VitricArmorProjectile moddedproj = shard.modProjectile as VitricArmorProjectile;
                    allshards.Add(moddedproj);
                }

                allshards = allshards.OrderBy((x) => x.projectile.Center.Y).ToList();

                foreach (VitricArmorProjectile modshard in allshards)
                {
                    double angle = Math.Sin(-modshard.projectile.localAI[1]);
                    if (angle > 0 && !back ||
                        angle <= 0 && back)
                        Main.playerDrawData.Add(modshard.Draw());
                }
            }
        }
    }

}