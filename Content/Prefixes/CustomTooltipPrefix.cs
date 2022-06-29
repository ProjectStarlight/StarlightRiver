using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Prefixes
{
	public abstract class CustomTooltipPrefix : ModPrefix
    {
        public readonly string _tooltip;

        protected CustomTooltipPrefix(string tooltip) => _tooltip = tooltip;

        public virtual void Update(Item Item, Player Player) { }

        public virtual void SafeApply(Item Item) { }

        public override void Apply(Item Item)
        {
            Item.GetGlobalItem<StarlightItem>().prefixLine = _tooltip;
            SafeApply(Item);
        }
    }
}
