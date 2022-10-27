using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	class LostCollar : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public LostCollar() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "BloodlessAmuletGlow").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lost Collar");
			Tooltip.SetDefault("+40% Inoculation\nDebuffs you inflict are inflicted on you\n+5% movement and attack speed per debuff affecting you\nCursed : Lose all debuff immunities");
		}

		public override void Load() //TODO: Make cursedaccessory.Load not hide this
		{
			StatusTrackingNPC.buffCompareEffects += CollarEffects;
            StarlightNPC.OnKillEvent += AddLostCollarToZoologistLoot;
		}

        public override void Unload()
		{
			StatusTrackingNPC.buffCompareEffects -= CollarEffects;
			StarlightNPC.OnKillEvent -= AddLostCollarToZoologistLoot; 
		}

		private void CollarEffects(Player player, NPC NPC, int[] oldTypes, int[] newTypes, int[] oldTimes, int[] newTimes)
		{
			if (Equipped(player))
			{
				for (int k = 0; k < 5; k++)
				{
					if ((oldTypes[k] != newTypes[k] || newTimes[k] > oldTimes[k]) && Main.debuff[player.buffType[k]])
						player.AddBuff(newTypes[k], newTimes[k]);
				}
			}
		}

		private void AddLostCollarToZoologistLoot(NPC NPC)
		{
			if (NPC.type == NPCID.BestiaryGirl)
            {
				int playerIndex = NPC.lastInteraction;
				if (!Main.player[playerIndex].active || Main.player[playerIndex].dead)
				{
					playerIndex = NPC.FindClosestPlayer();
				}
				Player player = Main.player[playerIndex];

				if (!player.HasItem(Type))
					Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), Type, 1);
			}
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.4f;

			for (int k = 0; k < Player.buffImmune.Length; k++)
			{
				Player.buffImmune[k] = false;
			}

			for(int k = 0; k < Player.buffType.Length; k++)
				if(Player.buffType[k] > 0 && Main.debuff[Player.buffType[k]])
				{
					Player.maxRunSpeed += 0.05f;
					Player.GetModPlayer<StarlightPlayer>().ItemSpeed += 0.05f;
				}
		}
	}
}
