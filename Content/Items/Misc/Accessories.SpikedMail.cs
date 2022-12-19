using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class SpikedMail : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		private int oldBarrier;

		public SpikedMail() : base("Spiked Mail", "Barrier damage is applied to attackers as thorns \n+20 barrier") { }

		public override void Load()
		{
			StarlightPlayer.OnHitByNPCEvent += HitByNPC;
			StarlightPlayer.ResetEffectsEvent += ResetEffects;
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 0, 25, 0);
			Item.rare = ItemRarityID.Green;
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.GetModPlayer<BarrierPlayer>().maxBarrier += 20;
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

		private void ResetEffects(StarlightPlayer modPlayer)
		{
			Player player = modPlayer.Player;

			if (oldBarrier != player.GetModPlayer<BarrierPlayer>().barrier)
				oldBarrier = player.GetModPlayer<BarrierPlayer>().barrier;
		}

		private void HitByNPC(Player player, NPC NPC, int damage, bool crit)
		{
			if (!Equipped(player))
				return;

			BarrierPlayer barrierplayer = player.GetModPlayer<BarrierPlayer>();

			if (oldBarrier > barrierplayer.barrier)
			{
				int damageToDeal = oldBarrier - barrierplayer.barrier;
				NPC.StrikeNPC(Math.Max(1, damageToDeal - NPC.defense / 2), 1, Math.Sign(NPC.Center.X - player.Center.X));
			}
		}
	}
}