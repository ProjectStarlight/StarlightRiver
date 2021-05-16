using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
    class TempleRune : SmartAccessory
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        public TempleRune() : base("Rune of Warding", "+20 maximum barrier") { }

        public override void SafeSetDefaults()
        {
            item.rare = ItemRarityID.Blue;
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.GetModPlayer<ShieldPlayer>().MaxShield += 20;
        }
    }
}
