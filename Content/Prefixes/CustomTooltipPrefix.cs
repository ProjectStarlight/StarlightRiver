using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Prefixes
{
    public abstract class CustomTooltipPrefix : ModPrefix
    {
        public readonly string _tooltip;

        protected CustomTooltipPrefix(string tooltip) => _tooltip = tooltip;

        public virtual void SafeApply(Item item) { }

        public override void Apply(Item item)
        {
            item.GetGlobalItem<StarlightItem>().prefixLine = _tooltip;
            SafeApply(item);
        }
    }
}
