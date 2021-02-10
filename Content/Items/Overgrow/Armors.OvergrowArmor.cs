using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;


namespace StarlightRiver.Content.Items.Overgrow
{
    [AutoloadEquip(EquipType.Head)]
    public class OvergrowHead : ModItem
    {
        public override string Texture => AssetDirectory.OvergrowItem + "OvergrowHead";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Faeleaf Cowl");
            Tooltip.SetDefault("5% increased magic and ranged critial strike change");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
            player.magicCrit += 5;
            player.rangedCrit += 5;
        }

        public override void AddRecipes()
        {
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class OvergrowChest : ModItem
    {
        public override string Texture => AssetDirectory.OvergrowItem + "OvergrowChest";

        public int floatTime = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Faeleaf Cloak");
            Tooltip.SetDefault("15 % increased ranged damage while airborne");
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
            if (player.velocity.Y != 0)
                player.rangedDamageMult += 0.15f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<OvergrowHead>() && legs.type == ItemType<OvergrowLegs>();
        }

        public override void UpdateArmorSet(Player player)
        {
            Main.NewText(floatTime);
            player.setBonus = "Hitting enemies with ranged attacks allows you to float\n20% increased ranged critical strike chance while airborne";

            if (floatTime > 0)
            {
                player.fallStart = (int)player.position.Y;
                player.maxFallSpeed -= 8.5f;
                floatTime--;
            }

            if (player.velocity.Y != 0)
                player.rangedCrit += 20;
        }

        public override void AddRecipes()
        {
        }
    }

    public class OvergrowArmorProjectile : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            foreach (Player player in Main.player.Where(player => player.armor[1].type == ItemType<OvergrowChest>()))
                if (projectile.owner == player.whoAmI && projectile.active && projectile.ranged && player.velocity.Y != 0 && player.armor[1].modItem is OvergrowChest)
                    (player.armor[1].modItem as OvergrowChest).floatTime = 40;

            foreach (Player player in Main.player.Where(player => player.armor[1].type == ItemType<OvergrowRobe>()))
                if (projectile.owner == player.whoAmI && projectile.active && projectile.magic && player.armor[1].modItem is OvergrowRobe && (player.armor[1].modItem as OvergrowRobe).leaves < 10)
                    (player.armor[1].modItem as OvergrowRobe).leaves++;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class OvergrowRobe : ModItem
    {
        public override string Texture => AssetDirectory.OvergrowItem + "OvergrowRobe";

        public int leaves = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Faeleaf Robes");
            Tooltip.SetDefault("10% increased magic damage\nincreased mana regeneration");
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
            player.magicDamageMult += 0.1f;
            player.manaRegenBonus += 15;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<OvergrowHead>() && legs.type == ItemType<OvergrowLegs>();
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Hitting enemies grants you damaging leaves\n getting hit releases them";

            for (int k = 0; k < leaves; k++)
            {
                Dust dus = Dust.NewDustPerfect(player.Center + (new Vector2((float)Math.Cos(StarlightWorld.rottime) * 2, (float)Math.Sin(StarlightWorld.rottime)) * 20).RotatedBy(k / (float)leaves * 6.28f),
                DustType<Dusts.GenericFollow>(), Vector2.Zero, 0, default, leaves == 10 ? 1.2f : 0.8f);
                dus.customData = player;
            }

            if (player.GetModPlayer<StarlightPlayer>().JustHit)
            {
                for (int k = 0; k < leaves; k++)
                    //Projectile.NewProjectile(player.Center, Vector2.One.RotatedByRandom(6.28f) * 3, ProjectileType<ArmorLeaf>(), 10, 0); TODO: Rework this
                leaves = 0;
            }
        }

        public override void AddRecipes()
        {
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class OvergrowLegs : ModItem
    {
        public override string Texture => AssetDirectory.OvergrowItem + "OvergrowLegs";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Faeleaf Boots");
            Tooltip.SetDefault("Increased jump height");
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
            player.jumpSpeedBoost += 2;
        }

        public override void AddRecipes()
        {
        }
    }
}