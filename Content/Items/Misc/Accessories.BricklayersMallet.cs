using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria;
using System;
using Microsoft.Xna.Framework;
using StarlightRiver.Core.Systems.ChestLootSystem;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class BricklayersMallet : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public BricklayersMallet() : base("Bricklayer's Mallet", "Doubles block placement and tool range\nDecreases mining speed by 50% for blocks outside your original range") { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(silver: 25);
        }

        public override void SafeUpdateEquip(Player Player)
        {
            Player.blockRange *= 2;
            if (Main.myPlayer == Player.whoAmI)
            {
                bool outsideXRange = Main.MouseWorld.X < Player.Center.X ? Math.Abs(Player.Left.X - Main.MouseWorld.X) > 80f + (Player.tileRangeX * 16f) : Math.Abs(Player.Right.X - Main.MouseWorld.X) > 80f + (Player.tileRangeX * 16f);
                bool outsideYRange = Main.MouseWorld.Y > Player.Center.Y ? Math.Abs(Player.Bottom.Y - Main.MouseWorld.Y) > 48f + (Player.tileRangeY * 16f) : Math.Abs(Player.Top.Y - Main.MouseWorld.Y) > 48f + (Player.tileRangeY * 16f);
                if (outsideXRange || outsideYRange)
                    Player.pickSpeed *= 1.5f;

                Player.tileRangeX *= 2;
                Player.tileRangeY *= 2;
            }
        }
    }

    class BricklayersMalletPool : LootPool
    {
        public override void AddLoot() => AddItem(ModContent.ItemType<BricklayersMallet>(), ChestRegionFlags.Surface, 0.25f, 1, false, -1);
    }
}
