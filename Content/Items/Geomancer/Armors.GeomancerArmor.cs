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

        public override void Load() //PORTTODO: replace this with something else
        {
            On.Terraria.Main.DrawPendingMouseText += SpoofMouseItem;
        }

		public override void Unload()
		{
            On.Terraria.Main.DrawPendingMouseText -= SpoofMouseItem;
        }

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer's Hood");
            //Tooltip.SetDefault("15% increased ranged critical strike damage");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = 8000;
            Item.defense = 5;
            Item.rare = 3;
        }

        public override void UpdateEquip(Player Player)
		{
            if (Player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.Topaz || Player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All)
                Player.GetModPlayer<BarrierPlayer>().MaxBarrier += 100;
        }

        private void SpoofMouseItem(On.Terraria.Main.orig_DrawPendingMouseText orig)
        {
            var Player = Main.LocalPlayer;

            if (dummyItem.IsAir && !Main.gameMenu)
                dummyItem.SetDefaults(ModContent.ItemType<GeomancerItemDummy>());

            if (IsGeomancerArmor(Main.HoverItem) && IsArmorSet(Player) && Player.controlUp)
            {
                Main.HoverItem = dummyItem.Clone();
                Main.hoverItemName = dummyItem.Name;
            }

            orig();
        }

        public bool IsGeomancerArmor(Item Item)
        {
            return Item.type == ModContent.ItemType<GeomancerHood>() ||
                Item.type == ModContent.ItemType<GeomancerRobe>() ||
                Item.type == ModContent.ItemType<GeomancerPants>();
        }

        public bool IsArmorSet(Player Player)
        {
            return Player.armor[0].type == ModContent.ItemType<GeomancerHood>() && Player.armor[1].type == ModContent.ItemType<GeomancerRobe>() && Player.armor[2].type == ModContent.ItemType<GeomancerPants>();
        }

    }

    [AutoloadEquip(EquipType.Body)]
    public class GeomancerRobe : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer's Cowl");
            //Tooltip.SetDefault("10% increased ranged damage");
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 20;
            Item.value = 6000;
            Item.defense = 6;
            Item.rare = 3;
        }

        /*public override void UpdateEquip(Player Player)
        {
            Player.rangedDamage += 0.1f;
        }*/

        public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == ModContent.ItemType<GeomancerHood>() && legs.type == ModContent.ItemType<GeomancerPants>();

        public override void UpdateArmorSet(Player Player)
        {
            Player.setBonus = "Kills ands critical strikes have a chance to drop magic gems\n" +
            "Each gem activates a different effect when picked up\n" +
            "Obtaining another gem stores the previous effect\n" +
            "Collecting all breifly activates every effect at once";

            Player.GetModPlayer<GeomancerPlayer>().SetBonusActive = true;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class GeomancerPants : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Geomancer's Greaves");
            //Tooltip.SetDefault("up to 20% ranged critical strike damage based on speed");
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
            Item.value = 4000;
            Item.defense = 5;
            Item.rare = 3;
        }

        /*public override void UpdateEquip(Player Player)
        {
            Player.GetModPlayer<CritMultiPlayer>().RangedCritMult += Math.Min(0.2f, Player.velocity.Length() / 16f * 0.2f);
        }*/
    }
    public class GeomancerItemDummy : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoDiamond";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Effects of different gems:");
            Tooltip.SetDefault(
            "Diamond: Critical strikes partially ignore armor, with increased chance for missing enemy HP \n" +
            "Topaz: +100 barrier. Gain a shield that points to your cursor and blocks attacks, consuming barrier \n" +
            "Emerald: Immediately heal 20 hp. Hits have a chance to create a 5 HP life heart \n" +
            "Sapphire: Immediately refill mana. Hits have a chance to create 1 to 3 mana stars \n" + 
            "Amethyst: All strikes inflict Toxic Amethyst, a stacking poison debuff \n" + 
            "Ruby: Hits have a chance to summon a Ruby Dagger to seek the struck enemy for 20% of the hit's damage");
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
        }
    }
}