using StarlightRiver.Core;
using StarlightRiver.Prefixes;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Prefixes.Accessory
{
    internal abstract class BarrierPrefix : CustomTooltipPrefix
    {
        private readonly int barrier;
        private readonly string name;
        private readonly string tip;

        internal BarrierPrefix(int barrier, string name, string tip)
        {
            this.barrier = barrier;
            this.name = name;
            this.tip = tip;
        }

        public override bool CanRoll(Item Item) => Item.accessory;

        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override void SetStaticDefaults() => DisplayName.SetDefault(name);

        public override void ModifyValue(ref float valueMult) => valueMult *= 1 + (barrier / 100);

        public override void Update(Item Item, Player Player)
        {
            Player.GetModPlayer<BarrierPlayer>().Barrier += barrier;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            TooltipLine newline = new TooltipLine(StarlightRiver.Instance, "BarrierTip", tip);
            newline.IsModifier = true;

            tooltips.Add(newline);
        }
    }

    internal class LayeredPrefix : BarrierPrefix
    {
        public LayeredPrefix() : base(4, "Layered", "+4 Barrier") { }
    }

    internal class DefensivePrefix : BarrierPrefix
    {
        public DefensivePrefix() : base(8, "Defensive", "+8 Barrier") { }
    }

    internal class PlatedPrefix : BarrierPrefix
    {
        public PlatedPrefix() : base(12, "Plated", "+12 Barrier") { }
    }

    internal class ReinforcedPrefix : BarrierPrefix
    {
        public ReinforcedPrefix() : base(16, "Reinforced", "+16 Barrier") { }
    }
}
