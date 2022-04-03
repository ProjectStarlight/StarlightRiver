using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;


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
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 2;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.magicCrit += 5;
            Player.rangedCrit += 5;
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
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5;
        }

        public override void UpdateEquip(Player Player)
        {
            if (Player.velocity.Y != 0)
                Player.rangedDamageMult += 0.15f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<OvergrowHead>() && legs.type == ItemType<OvergrowLegs>();
        }

        public override void UpdateArmorSet(Player Player)
        {
            //Main.NewText(floatTime);
            Player.setBonus = "Hitting enemies with ranged attacks allows you to float\n20% increased ranged critical strike chance while airborne";

            if (floatTime > 0)
            {
                Player.fallStart = (int)Player.position.Y;
                Player.maxFallSpeed -= 8.5f;
                floatTime--;
            }

            if (Player.velocity.Y != 0)
                Player.rangedCrit += 20;
        }

        public override void AddRecipes()
        {
        }
    }

    public class OvergrowArmorProjectile : GlobalProjectile
    {
        public override void OnHitNPC(Projectile Projectile, NPC target, int damage, float knockback, bool crit)
        {
            foreach (Player Player in Main.player.Where(Player => Player.armor[1].type == ItemType<OvergrowChest>()))
                if (Projectile.owner == Player.whoAmI && Projectile.active && Projectile.ranged && Player.velocity.Y != 0 && Player.armor[1].ModItem is OvergrowChest)
                    (Player.armor[1].ModItem as OvergrowChest).floatTime = 40;

            foreach (Player Player in Main.player.Where(Player => Player.armor[1].type == ItemType<OvergrowRobe>()))
                if (Projectile.owner == Player.whoAmI && Projectile.active && Projectile.magic && Player.armor[1].ModItem is OvergrowRobe && (Player.armor[1].ModItem as OvergrowRobe).leaves < 10)
                    (Player.armor[1].ModItem as OvergrowRobe).leaves++;
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
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.magicDamageMult += 0.1f;
            Player.manaRegenBonus += 15;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<OvergrowHead>() && legs.type == ItemType<OvergrowLegs>();
        }

        public override void UpdateArmorSet(Player Player)
        {
            Player.setBonus = "Hitting enemies grants you damaging leaves\n getting hit releases them";

            for (int k = 0; k < leaves; k++)
            {
                Dust dus = Dust.NewDustPerfect(Player.Center + (new Vector2((float)Math.Cos(StarlightWorld.rottime) * 2, (float)Math.Sin(StarlightWorld.rottime)) * 20).RotatedBy(k / (float)leaves * 6.28f),
                DustType<Dusts.GenericFollow>(), Vector2.Zero, 0, default, leaves == 10 ? 1.2f : 0.8f);
                dus.customData = Player;
            }

            if (Player.GetModPlayer<StarlightPlayer>().JustHit)
            {
                for (int k = 0; k < leaves; k++)
                    //Projectile.NewProjectile(Player.Center, Vector2.One.RotatedByRandom(6.28f) * 3, ProjectileType<ArmorLeaf>(), 10, 0); TODO: Rework this
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
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 3;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.jumpSpeedBoost += 2;
        }

        public override void AddRecipes()
        {
        }
    }
}