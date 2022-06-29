using StarlightRiver.Core;
using StarlightRiver.Prefixes;
using Terraria;
using Terraria.ModLoader;

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

        public override bool CanRoll(Item Item) => Item.accessory;

        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override void SetStaticDefaults() => DisplayName.SetDefault(name);

        public override void ModifyValue(ref float valueMult) => valueMult *= 1 + 0.05f * power;

        public override void Update(Item Item, Player Player)
        {
            Player.GetModPlayer<DoTResistancePlayer>().DoTResist += power;
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
