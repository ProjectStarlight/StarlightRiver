using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Prefixes
{
	public abstract class CustomTooltipPrefix : ModPrefix
	{
		public virtual void Update(Item Item, Player Player) { }

		public virtual void SafeApply(Item Item) { }

		public virtual void ModifyTooltips(Item item, List<TooltipLine> tooltips) { }

		public override void Apply(Item Item)
		{
			SafeApply(Item);
		}
	}

	public class CustomTooltipItem : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			ModPrefix prefix = PrefixLoader.GetPrefix(item.prefix);

			if (prefix is CustomTooltipPrefix)
				(prefix as CustomTooltipPrefix).ModifyTooltips(item, tooltips);
		}
	}
}
