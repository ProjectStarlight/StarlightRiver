using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
	public class WarriorKingsCrown : CursedAccessory
	{
		public int lastMinions;
		public int lastTurrets;

		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void Load()
		{
			StarlightPlayer.PostUpdateEquipsEvent += RecordStats;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Warrior King's Crown");
			Tooltip.SetDefault("All summon slots convert to +2% summoning critical strike chance per slot\n" +
				"All sentry slots convert to +30% summoning damage per slot\n" +
				"Summoning damage increased by 40%\nCombat mount cooldowns reduced by 30%");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 5);
		}

		private void RecordStats(StarlightPlayer player)
		{
			if (Equipped(player.Player))
			{
				(GetEquippedInstance(player.Player) as WarriorKingsCrown).lastMinions = player.Player.maxMinions;
				(GetEquippedInstance(player.Player) as WarriorKingsCrown).lastTurrets = player.Player.maxTurrets;
			}
		}

		public override void SafeUpdateAccessory(Player player, bool hideVisual)
		{
			player.GetCritChance(DamageClass.Summon) += lastMinions * 2;
			player.GetDamage(DamageClass.Summon) += lastTurrets * 0.3f;
			player.GetDamage(DamageClass.Summon) += 0.4f;
			player.GetModPlayer<CombatMountPlayer>().combatMountCooldownMultiplier += 0.3f;

			player.maxMinions = 0;
			player.maxTurrets = 0;
		}
	}
}