using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.Enums;

namespace StarlightRiver.Content.Items.Permafrost
{
    public class SquidFins : ModItem
    {
        public override string Texture => AssetDirectory.Debug;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Squid Fins");
            Tooltip.SetDefault("Allows you to swim like a jellysquid");
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 2));
        }
        public override void UpdateEquip(Player player)
        {
            bool canSwim = player.grapCount <= 0 && player.mount == null && player.wet;
            player.GetModPlayer<SwimPlayer>().ShouldSwim = canSwim;
            player.GetModPlayer<SwimPlayer>().SwimSpeed = 1.5f;
        }
    }
}
