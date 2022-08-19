using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;

namespace StarlightRiver.Content.Items.Misc
{
	public class WardedMail : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public WardedMail() : base("Warded Mail", "Barrier damage is applied to attackers as thorns \n+Barrier negates 10% more damage \n+40 barrier") { }

        public override List<int> ChildTypes => new List<int>()
        {
            ModContent.ItemType<SpikedMail>(),
        };

        public override void SafeSetDefaults()
        {
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.Green;
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.GetModPlayer<BarrierPlayer>().MaxBarrier += 20;
            player.GetModPlayer<BarrierPlayer>().BarrierDamageReduction += 0.1f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<SpikedMail>(), 1);
            recipe.AddIngredient(ModContent.ItemType<Dungeon.AquaSapphire>(), 1);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();

        }
    }
}