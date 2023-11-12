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
		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public WarriorKingsCrown() : base(ModContent.Request<Texture2D>(AssetDirectory.ArtifactItem + "WarriorKingsCrown").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Warrior King's Crown");
			Tooltip.SetDefault("Cursed : All of your summon slots are converted into +2% summoning critical strike chance per slot" +
				" and all of your sentry slots are converted into +20% summoning damage per slot\n" +
				"Summoning damage increased by 200%\nCombat mount cooldowns reduced by 30%");
		}

		public override void SafeUpdateAccessory(Player Player, bool hideVisual)
		{
			Player.GetCritChance(DamageClass.Summon) += Player.maxMinions * 2;
			Player.GetDamage(DamageClass.Summon) += Player.maxTurrets * 0.2f;
			Player.GetDamage(DamageClass.Summon) += 2;
			Player.GetModPlayer<CombatMountPlayer>().combatMountCooldownMultiplier += 0.3f;

			Player.maxMinions = 0;
			Player.maxTurrets = 0;
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 5);
		}
	}
}
