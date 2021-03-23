using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.Starwood;
using StarlightRiver.Core;
using StarlightRiver.Items.Armor;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class AncientStarwoodHat : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public AncientStarwoodHat() : base(GetTexture(AssetDirectory.VanityItem + "AncientStarwoodHat_Alt")) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Starwood Hat");
            //Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            item.width = 42;
            item.height = 26;
            item.value = Item.sellPrice(0, 0, 0, 14);
            item.vanity = true;
        }

        public override void UpdateEquip(Player player) =>
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public override void UpdateVanity(Player player, EquipType type) =>
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public void DrawArmorLayer(PlayerDrawInfo info)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().Empowered)
                ArmorHelper.QuickDrawHeadFramed(info, AssetDirectory.VanityItem + "AncientStarwoodHat_Worn_Alt", 1, new Vector2(10, 4));
            else
                ArmorHelper.QuickDrawHeadFramed(info, AssetDirectory.VanityItem + "AncientStarwoodHat_Worn", 1, new Vector2(10, 4));
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class AncientStarwoodChest : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public AncientStarwoodChest() : base(GetTexture(AssetDirectory.VanityItem + "AncientStarwoodChest_Alt")) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Starwood Robes");
            //Tooltip.SetDefault("");
        }

        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 30;
            item.value = Item.sellPrice(0, 0, 0, 16);
            item.vanity = true;
        }

        public override void UpdateEquip(Player player) =>
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public override void UpdateVanity(Player player, EquipType type) => 
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public void DrawArmorLayer(PlayerDrawInfo info)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().Empowered)
                ArmorHelper.QuickDrawBodyFramed(info, AssetDirectory.VanityItem + "AncientStarwoodChest_Body_Alt", 1, new Vector2(10, 18));
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class AncientStarwoodBoots : StarwoodItem, IArmorLayerDrawable
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public AncientStarwoodBoots() : base(GetTexture(AssetDirectory.VanityItem + "AncientStarwoodBoots_Alt")) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ancient Starwood Leggings");
            //Tooltip.SetDefault(" ");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 18;
            item.value = Item.sellPrice(0, 0, 0, 12);
            item.vanity = true;
        }

        public override void UpdateEquip(Player player) =>
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public override void UpdateVanity(Player player, EquipType type) => 
            isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;

        public void DrawArmorLayer(PlayerDrawInfo info)
        {
            if (info.drawPlayer.GetModPlayer<StarlightPlayer>().Empowered)
                ArmorHelper.QuickDrawLegsFramed(info, AssetDirectory.VanityItem + "AncientStarwoodBoots_Legs_Alt", 1, new Vector2(10, 18));
        }
    }
}