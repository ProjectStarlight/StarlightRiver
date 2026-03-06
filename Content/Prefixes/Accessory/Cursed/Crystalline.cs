using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;

namespace StarlightRiver.Content.Prefixes.Accessory.Cursed
{
	internal class Crystalline : ModPrefix
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
			DisplayName.SetDefault("Crystalline");
		}

		public override void ModifyValue(ref float valueMult)
		{
			valueMult *= 2f;
		}

		public override void ApplyAccessoryEffects(Player player)
		{
			player.endurance += 0.03f;
			player.statDefense += 4;
			player.moveSpeed -= 0.1f;
		}

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(StarlightRiver.Instance, "CrystallineTip1", "+4 defense")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "CrystallineTip2", "+3% damage resistance")
			{
				IsModifier = true
			};

			yield return new TooltipLine(StarlightRiver.Instance, "CrystallineTip3", "-10% movement speed")
			{
				IsModifier = true,
				IsModifierBad = true
			};
		}
	}
}