using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.JungleCorrupt
{
    public class JungleCorruptSoul : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Nightfae");
            Tooltip.SetDefault("A glowing fragment of darkness");
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(5, 9));
            ItemID.Sets.ItemNoGravity[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.alpha = 0;
            item.width = 14;
            item.height = 14;
            item.rare = ItemRarityID.LightRed;
            item.maxStack = 999;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(item.Center, .3f, .25f, 0.6f);
            item.position.Y += (float)Math.Sin(StarlightWorld.rottime) / 3;
        }
    }
}