using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Prefixes;

namespace StarlightRiver.Content.Prefixes.Accessory
{
    internal abstract class DoTResistPrefix : CustomTooltipPrefix
    {
        private readonly float power;
        private readonly string name;

        internal DoTResistPrefix(float power, string name, string tip) : base(tip)
        {
            this.power = power;
            this.name = name;
        }

        public override bool CanRoll(Item item) => item.accessory;

        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override void SetDefaults() => DisplayName.SetDefault(name);

        public override void ModifyValue(ref float valueMult) => valueMult *= 1 + 0.05f * power;

        public override void Update(Item item, Player player)
        {
            player.GetModPlayer<DoTResistancePlayer>().DoTResist += power;
        }
    }

    internal class DoTResistPrefix1 : DoTResistPrefix
    {
        public DoTResistPrefix1() : base(0.02f, "Healthy", "+2% DoT Resistance") { }
    }

    internal class DoTResistPrefix2 : DoTResistPrefix 
    {
        public DoTResistPrefix2() : base(0.04f, "Protected", "+4% DoT Resistance") { }
    }

    internal class DoTResistPrefix3 : DoTResistPrefix
    {
        public DoTResistPrefix3() : base(0.05f, "Blessed", "+5% DoT Resistance") { }
    }
}
