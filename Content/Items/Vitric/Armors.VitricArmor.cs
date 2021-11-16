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

namespace StarlightRiver.Content.Items.Vitric
{
	[AutoloadEquip(EquipType.Head)]
    public class VitricHead : ModItem
    {
        public int shardTimer = 0;
        public int shardCount = 0;
        public bool loaded = false;

        public List<Projectile> idleShards = new List<Projectile>();

        public override string Texture => AssetDirectory.VitricItem + Name;

		public override bool Autoload(ref string name)
		{
            StarlightItem.PickAmmoEvent += PickShardsWhenLoaded;
            On.Terraria.Player.KeyDoubleTap += LoadShots;
			return base.Autoload(ref name);
		}

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

        public override void UpdateInventory(Player player)
        {
            shardTimer = 0;
            shardCount = 0;
            loaded = false;
        }

		public override void UpdateEquip(Player player)
        {
            player.rangedDamage += 0.1f;

            if (!IsArmorSet(player.armor[0], player.armor[1], player.armor[2]))
            {
                shardTimer = 0;
                shardCount = 0;
                loaded = false;
            }
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == mod.ItemType("VitricChest") && legs.type == mod.ItemType("VitricLegs");
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Accumulate powerful glass shards over time\nDouble tap DOWN to load these shards into your bow\nShards fired from bows have high velocity and damage";

            if (player.whoAmI != Main.myPlayer)
                return;

            if (shardCount < 3 && !loaded)
            {
                shardTimer++;

                if (shardTimer == 210)
                {
                    int i = Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<VitricArmorProjectileIdle>(), 1, 1, player.whoAmI, 0, shardCount);
                    var proj = Main.projectile[i].modProjectile as VitricArmorProjectileIdle;
                    proj.parent = this;
                }

                if (shardTimer >= 240)
                {
                    shardCount++;
                    shardTimer = shardCount > 0 ? 60 : 0;
                }
            }
            else
                shardTimer = 0;

            if (shardCount <= 0)
                loaded = false; //failsafe
        }

        private void LoadShots(On.Terraria.Player.orig_KeyDoubleTap orig, Player player, int keyDir)
        {
            if (keyDir == 0 && player.armor[0].type == ItemType<VitricHead>())
            {
                var helm = player.armor[0].modItem as VitricHead;
                helm.loaded = true;
            }

            orig(player, keyDir);
        }

        private void PickShardsWhenLoaded(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback)
        {
            if (player.armor[0].type == ItemType<VitricHead>() && ammo.ammo == AmmoID.Arrow)
            {
                var helm = player.armor[0].modItem as VitricHead;

                if (helm.loaded && helm.shardCount > 0)
                {
                    Helper.PlayPitched("Magic/FireHit", 1, 0, player.Center);
                    type = ProjectileType<VitricArmorProjectile>();
                    speed = 10;
                    damage = weapon.damage + 100;
                    helm.shardCount--;

                    if (helm.shardCount <= 0)
                        helm.loaded = false;
                }
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 5);
            recipe.AddIngredient(ItemType<VitricOre>(), 15);
            recipe.AddIngredient(ItemType<MagmaCore>());
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
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
            recipe.AddIngredient(ItemType<MagmaCore>());
            recipe.AddTile(TileID.Anvils);
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
            recipe.AddIngredient(ItemType<MagmaCore>());
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}