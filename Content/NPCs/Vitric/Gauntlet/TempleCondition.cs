using StarlightRiver.Content.Biomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal class TempleCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			return info.player.InModBiome<VitricTempleBiome>();
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return "When killed inside the vitric forge";
		}
	}
}
