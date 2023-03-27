using Terraria.GameContent.ItemDropRules;

namespace StarlightRiver.Content.DropRules
{
	public static class CustomDropRules
	{
		public static IItemDropRule onlyInNormalMode(int ItemID, int chanceDenominator = 2, int amountDroppedMin = 1, int amountDroppedMax = 1, int chanceNumerator = 1)
		{
			return ItemDropRule.ByCondition(new Conditions.NotExpert(), ItemID, chanceDenominator, amountDroppedMin, amountDroppedMax, chanceNumerator);
		}

		//the way to use this is really weird but there are examples of using it for boss bags and a boss in SquidBoss.cs
		public static IItemDropRule ConditionalOneFromOptions(this LeadingConditionRule condition, int[] options)
		{
			return condition.OnSuccess(ItemDropRule.OneFromOptions(1, options));
		}

		public static IItemDropRule ConditionalFewFromOptions(this LeadingConditionRule condition, int[] options, int amount)
		{
			return condition.OnSuccess(ItemDropRule.FewFromOptions(amount, 1, options));
		}
	}
}
