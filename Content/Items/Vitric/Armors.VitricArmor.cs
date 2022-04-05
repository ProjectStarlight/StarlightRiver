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

        public override string Texture => AssetDirectory.VitricItem + Name;

		public override void Load()
		{
            StarlightItem.PickAmmoEvent += PickShardsWhenLoaded;
            On.Terraria.Player.KeyDoubleTap += LoadShots;
			
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Headgear");
            Tooltip.SetDefault("10% increased ranged damage");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 4;
        }

        public override void UpdateInventory(Player Player)
        {
            shardTimer = 0;
            shardCount = 0;
            loaded = false;
        }

		public override void UpdateEquip(Player Player)
        {
            Player.rangedDamage += 0.1f;

            if (!IsArmorSet(Player.armor[0], Player.armor[1], Player.armor[2]))
            {
                shardTimer = 0;
                shardCount = 0;
                loaded = false;
            }
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<VitricChest") && legs.type == Mod.ItemType("VitricLegs>();
        }

        public override void UpdateArmorSet(Player Player)
        {
            Player.setBonus = "Accumulate powerful glass shards over time\nDouble tap DOWN to load these shards into your bow\nShards fired from bows have high velocity and damage";

            if (Player.whoAmI != Main.myPlayer)
                return;

            if (shardCount < 3 && !loaded)
            {
                shardTimer++;

                if (shardTimer == 210)
                {
                    int i = Projectile.NewProjectile(Player.Center, Vector2.Zero, ProjectileType<VitricArmorProjectileIdle>(), 1, 1, Player.whoAmI, 0, shardCount);
                    var proj = Main.projectile[i].ModProjectile as VitricArmorProjectileIdle;
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

        private void LoadShots(On.Terraria.Player.orig_KeyDoubleTap orig, Player Player, int keyDir)
        {
            if (keyDir == 0 && Player.armor[0].type == ItemType<VitricHead>())
            {
                var helm = Player.armor[0].ModItem as VitricHead;
                helm.loaded = true;
            }

            orig(Player, keyDir);
        }

        private void PickShardsWhenLoaded(Item weapon, Item ammo, Player Player, ref int type, ref float speed, ref int damage, ref float knockback)
        {
            if (Player.armor[0].type == ItemType<VitricHead>() && ammo.ammo == AmmoID.Arrow)
            {
                var helm = Player.armor[0].ModItem as VitricHead;

                if (helm.loaded && helm.shardCount > 0)
                {
                    Helper.PlayPitched("Magic/FireHit", 1, 0, Player.Center);
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
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 5);
            recipe.AddIngredient(ItemType<VitricOre>(), 15);
            recipe.AddIngredient(ItemType<MagmaCore>());
            recipe.AddTile(TileID.Anvils);
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
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 6;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.GetCritChance(DamageClass.Ranged) += 5;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 5);
            recipe.AddIngredient(ItemType<VitricOre>(), 25);
            recipe.AddIngredient(ItemType<MagmaCore>());
            recipe.AddTile(TileID.Anvils);
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
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.GetHandler().StaminaRegenRate += 0.1f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 5);
            recipe.AddIngredient(ItemType<VitricOre>(), 20);
            recipe.AddIngredient(ItemType<MagmaCore>());
            recipe.AddTile(TileID.Anvils);
        }
    }
}