using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.WorldGeneration;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace StarlightRiver.Content.Items.Misc
{
	public class SpikedMail : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public SpikedMail() : base("Spiked Mail", "Barrier damage is applied to attackers as thorns \n+20 barrier") { }

        public override void SafeSetDefaults()
        {
            Item.value = Item.sellPrice(0, 0, 25, 0);
            Item.rare = ItemRarityID.Green;
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.GetModPlayer<BarrierPlayer>().MaxBarrier += 20;
            player.GetModPlayer<SpikedMailPlayer>().active = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronChainmail, 1);
            recipe.AddIngredient(ItemID.Spike, 15);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LeadChainmail, 1);
            recipe.AddIngredient(ItemID.Spike, 15);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    public class SpikedMailPlayer : ModPlayer
    {
        public bool active = false;

        private int oldBarrier = 0;

        public override void ResetEffects()
        {
            active = false;
            if (oldBarrier != Player.GetModPlayer<BarrierPlayer>().Barrier)
                oldBarrier = Player.GetModPlayer<BarrierPlayer>().Barrier;
        }

        public override void OnHitByNPC(NPC npc, int damage, bool crit)
        {
            BarrierPlayer barrierplayer = Player.GetModPlayer<BarrierPlayer>();
            if (oldBarrier > barrierplayer.Barrier)
            {
                int damageToDeal = oldBarrier - barrierplayer.Barrier;
                npc.StrikeNPC((int)Math.Max(1, damageToDeal - (npc.defense / 2)), 1, Math.Sign(npc.Center.X - Player.Center.X)); 
            }
        }
    }
}