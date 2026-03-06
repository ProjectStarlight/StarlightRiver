using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.InoculationSystem;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Relic
{
	internal class Hextouched : ModPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.GetGlobalItem<RelicItem>().isRelic;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hextouched");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 20f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.maxMinions += 1;
			player.maxTurrets += 1;
			player.GetDamage(DamageClass.Summon).Additive += 0.05f;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "HextouchedTip1", "Increases your max number of minions by 1")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "HextouchedTip2", "Increases your max number of sentries by 1")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "HextouchedTip3", "5% increased summon damage")
			{
				IsModifier = true
			};
		}
	}
}
