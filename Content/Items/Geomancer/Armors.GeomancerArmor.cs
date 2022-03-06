using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Dyes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Geomancer
{
    [AutoloadEquip(EquipType.Head)]
    public class GeomancerHood : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        internal static Item dummyItem = new Item();

        public override bool Autoload(ref string name)
        {
            On.Terraria.Main.MouseText_DrawItemTooltip += SpoofMouseItem;
            return true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer hood");
            //Tooltip.SetDefault("15% increased ranged critical strike damage");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.value = 8000;
            item.defense = 5;
            item.rare = 3;
        }

        /*public override void UpdateEquip(Player player)
		{
            player.GetModPlayer<CritMultiPlayer>().RangedCritMult += 0.15f;
		}*/

        private void SpoofMouseItem(On.Terraria.Main.orig_MouseText_DrawItemTooltip orig, Main self, int rare, byte diff, int X, int Y)
        {
            var player = Main.LocalPlayer;

            if (dummyItem.IsAir)
                dummyItem.SetDefaults(ModContent.ItemType<GeomancerItemDummy>());

            if (IsGeomancerArmor(Main.HoverItem) && IsArmorSet(player) && player.controlUp)
            {
                Main.HoverItem = dummyItem.Clone();
                Main.hoverItemName = dummyItem.Name;
            }

            orig(self, rare, diff, X, Y);
        }

        public bool IsGeomancerArmor(Item item)
        {
            return item.type == ModContent.ItemType<GeomancerHood>() ||
                item.type == ModContent.ItemType<GeomancerRobe>() ||
                item.type == ModContent.ItemType<GeomancerPants>();
        }

        public bool IsArmorSet(Player player)
        {
            return player.armor[0].type == ModContent.ItemType<GeomancerHood>() && player.armor[1].type == ModContent.ItemType<GeomancerRobe>() && player.armor[2].type == ModContent.ItemType<GeomancerPants>();
        }

    }

    [AutoloadEquip(EquipType.Body)]
    public class GeomancerRobe : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer Robe");
            //Tooltip.SetDefault("10% increased ranged damage");
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 20;
            item.value = 6000;
            item.defense = 6;
            item.rare = 3;
        }

        /*public override void UpdateEquip(Player player)
        {
            player.rangedDamage += 0.1f;
        }*/

        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ModContent.ItemType<GeomancerHood>() && legs.type == ModContent.ItemType<GeomancerPants>();

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "egshels update this lol";

            player.GetModPlayer<GeomancerPlayer>().SetBonusActive = true;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GeomancerPants : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer Pants");
            //Tooltip.SetDefault("up to 20% ranged critical strike damage based on speed");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
            item.value = 4000;
            item.defense = 5;
            item.rare = 3;
        }

        /*public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<CritMultiPlayer>().RangedCritMult += Math.Min(0.2f, player.velocity.Length() / 16f * 0.2f);
        }*/
    }
    public class GeomancerItemDummy : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoDiamond";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Effects of different gems:");
            Tooltip.SetDefault(
            "Diamond does a funny \n" +
            "Topaz does a funny \n" + 
            "Emerald does a funny \n" +
            "Sapphire does a funny \n" + 
            "Amethyst does a funny \n" + 
            "Ruby does a funny");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
        }
    }
}