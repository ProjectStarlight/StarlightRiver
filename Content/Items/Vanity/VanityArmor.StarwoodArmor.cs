﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Starwood;
using StarlightRiver.Core;
using StarlightRiver.Items.Armor;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vanity
{
	[AutoloadEquip(EquipType.Head)]
    public class AncientStarwoodHat : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public AncientStarwoodHat() : base(Request<Texture2D>(AssetDirectory.VanityItem + "AncientStarwoodHat_Alt").Value) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Starwood Hat");
            //Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 26;
            Item.value = Item.sellPrice(0, 0, 0, 14);
            Item.vanity = true;
        }

        public override void UpdateEquip(Player Player) =>
            isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;

        public override void UpdateVanity(Player player) =>
            isEmpowered = player.GetModPlayer<StarlightPlayer>().empowered;

        public void DrawArmorLayer(PlayerDrawSet info)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().empowered)
                ArmorHelper.QuickDrawHeadFramed(info, AssetDirectory.VanityItem + "AncientStarwoodHat_Worn_Alt", 1, new Vector2(0, -17));
            else
                ArmorHelper.QuickDrawHeadFramed(info, AssetDirectory.VanityItem + "AncientStarwoodHat_Worn", 1, new Vector2(0, -17));
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class AncientStarwoodChest : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public AncientStarwoodChest() : base(Request<Texture2D>(AssetDirectory.VanityItem + "AncientStarwoodChest_Alt").Value) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Starwood Robes");
            //Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 30;
            Item.value = Item.sellPrice(0, 0, 0, 16);
            Item.vanity = true;
        }

        public override void UpdateEquip(Player Player) =>
            isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;
        public override void UpdateVanity(Player Player) => 
            isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;

        public void DrawArmorLayer(PlayerDrawSet info)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().empowered)
                ArmorHelper.QuickDrawBodyFramed(info, AssetDirectory.VanityItem + "AncientStarwoodChest_Body_Alt", 1, new Vector2(10, 18));
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class AncientStarwoodBoots : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public AncientStarwoodBoots() : base(Request<Texture2D>(AssetDirectory.VanityItem + "AncientStarwoodBoots_Alt").Value) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Starwood Leggings");
            //Tooltip.SetDefault(" ");
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 0, 0, 12);
            Item.vanity = true;
        }

        public override void UpdateEquip(Player Player) =>
            isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;

        public override void UpdateVanity(Player Player) => 
            isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;

        public void DrawArmorLayer(PlayerDrawSet info)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().empowered)
                ArmorHelper.QuickDrawLegsFramed(info, AssetDirectory.VanityItem + "AncientStarwoodBoots_Legs_Alt", 1, new Vector2(10, 18));
        }
    }
}