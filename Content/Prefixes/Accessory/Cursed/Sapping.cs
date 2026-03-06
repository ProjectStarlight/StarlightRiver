using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal class Sapping : ModPrefix
	{
		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override bool CanRoll(Item item)
		{
			return item.accessory;
		}

		public override float RollChance(Item item)
		{
			return item.ModItem is CursedAccessory ? 1f : 0f;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sapping");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.lifeRegen += 10;
			player.manaRegenBonus += 10;

			player.GetDamage(DamageClass.Generic) -= 0.02f;
			player.GetCritChance(DamageClass.Generic) -= 2;
			player.statDefense -= 2;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "SappingTip1", "+10 life regeneration")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "SappingTip2", "+10 mana regeneration")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "SappingTip3", "-2% damage")
			{
				IsModifier = true,
				IsModifierBad = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "SappingTip4", "-2% critical strike chance")
			{
				IsModifier = true,
				IsModifierBad = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "SappingTip5", "-2 defense")
			{
				IsModifier = true,
				IsModifierBad = true
			};
		}
	}
}