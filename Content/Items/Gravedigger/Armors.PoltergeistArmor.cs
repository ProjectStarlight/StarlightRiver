using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Gravedigger
{
    [AutoloadEquip(EquipType.Head)]
    public class PoltergeistHead : ModItem
    {
        public List<Projectile> minions = new List<Projectile>();
        public int Timer;
        public int sleepTimer;

        public override string Texture => AssetDirectory.GravediggerItem + Name;

        public override void Load()
        {
            On.Terraria.Player.KeyDoubleTap += HauntItem;
            StarlightItem.CanUseItemEvent += ControlItemUse;          
        }

		public override void Unload()
		{
            On.Terraria.Player.KeyDoubleTap -= HauntItem;
            StarlightItem.CanUseItemEvent -= ControlItemUse;
        }

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Haunting Hood");
            Tooltip.SetDefault("15% increased magic critical strike damage");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<CritMultiPlayer>().MagicCritMult += 0.15f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<PoltergeistChest>() && legs.type == ItemType<PoltergeistLegs>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = 
                "Double tap DOWN with a magic weapon to haunt or unhaunt it, or with an empty hand to unhaunt all\n" +
                "Haunted weapons float around you and attack automatically, but decrease your max mana\n" +
                "Haunted weapons become disinterested in non-magic users and can't be used while haunted";

            minions.RemoveAll(n => !n.active || n.type != ProjectileType<PoltergeistMinion>());

            Timer++;

            for (int k = 0; k < minions.Count; k++)
            {
                var proj = minions[k].ModProjectile as PoltergeistMinion;
                player.GetModPlayer<ResourceReservationPlayer>().ReserveMana((int)(proj.Item.mana * (60f / proj.Item.useTime) * 2));
            }

            if (player == Main.LocalPlayer && sleepTimer == 1 && minions.Count > 0) //warning message
                Main.NewText("Your haunted weapons seem bored...", new Color(200, 120, 255));

            if (sleepTimer > 0) //decrement sleep timer
                sleepTimer--;
            
        }

        private void HauntItem(On.Terraria.Player.orig_KeyDoubleTap orig, Player player, int keyDir)
        {
            if (keyDir == 0 && player.armor[0].type == ItemType<PoltergeistHead>())
            {
                var item = player.HeldItem;
                var helm = player.armor[0].ModItem as PoltergeistHead;
                var mp = player.GetModPlayer<ResourceReservationPlayer>();

                if (item.IsAir) //clear from empty hand
				{
                    helm.minions.Clear();
                    orig(player, keyDir);
                    return;
                }

                if (helm.minions.Any(n => (n.ModProjectile as PoltergeistMinion).Item.type == item.type)) //removal
                {
                    helm.minions.RemoveAll(n => (n.ModProjectile as PoltergeistMinion).Item.type == item.type);
                    orig(player, keyDir);
                    return;
                }

                if (item.DamageType.Type == DamageClass.Magic.Type && item.mana > 0 && !item.channel && item.shoot > 0 && mp.TryReserveMana((int)(item.mana * (60f / item.useTime) * 2))) //addition
                {                  
                    int i = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ProjectileType<PoltergeistMinion>(), 0, 0, player.whoAmI);
                    var proj = Main.projectile[i];
                    (proj.ModProjectile as PoltergeistMinion).Item = item.Clone();

                    helm.minions.Add(proj);
                    helm.sleepTimer = 1200;
                }
            }

            orig(player, keyDir);
        }

        private bool ControlItemUse(Item item, Player player)
        {
            if (player.armor[0].type == ItemType<PoltergeistHead>())
            {
                var helm = player.armor[0].ModItem as PoltergeistHead;

                if (helm.minions.Any(n => (n.ModProjectile as PoltergeistMinion)?.Item?.type == item.type))
                    return false;

                if (item.DamageType.Type == DamageClass.Magic.Type)
                    helm.sleepTimer = 1200;
            }

            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Silk, 14);
            recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 7);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Silk, 14);
            recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 7);
            recipe.AddTile(TileID.Loom);
            recipe.Register();
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class PoltergeistChest : ModItem
    {
        public override string Texture => AssetDirectory.GravediggerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Haunting Breastplate");
            Tooltip.SetDefault("5% increased magic damage\n+15% Inoculation");
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
            Player.GetDamage(DamageClass.Magic) += 0.05f;
            Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.15f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Silk, 16);
            recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 8);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Silk, 16);
            recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 8);
            recipe.AddTile(TileID.Loom);
            recipe.Register();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class PoltergeistLegs : ModItem
    {
        public override string Texture => AssetDirectory.GravediggerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Haunting Robes");
            Tooltip.SetDefault("+40 maximum mana");
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
            Player.statManaMax2 += 40;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 6);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Silk, 12);
            recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 6);
            recipe.AddTile(TileID.Loom);
            recipe.Register();
        }
    }
}