using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Prefixes.Accessory
{
    internal abstract class StaminaPrefix : CustomTooltipPrefix
    {
        private readonly int _power;
        private readonly string _name;

        internal StaminaPrefix(int power, string name, string tip) : base(tip)
        {
            _power = power;
            _name = name;
        }

        public override bool CanRoll(Item item) => item.accessory;

        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override void SetDefaults() => DisplayName.SetDefault(_name);

        public override void ModifyValue(ref float valueMult) => valueMult *= 1 + 0.05f * _power;

        public override void SafeApply(Item item)
        {
            item.GetGlobalItem<Core.StarlightItem>().staminaRegenUp += _power;
        }
    }

    internal class StaminaPrefix1 : StaminaPrefix
    {
        public StaminaPrefix1() : base(2, "Springy", "+2 stamina regeneration") { }
    }

    internal class StaminaPrefix2 : StaminaPrefix
    {
        public StaminaPrefix2() : base(4, "Energetic", "+4 stamina regeneration") { }
    }

    internal class StaminaPrefix3 : StaminaPrefix
    {
        public StaminaPrefix3() : base(6, "Empowered", "+6 stamina regeneration") { }
    }

    internal class StaminaPrefix4 : StaminaPrefix
    {
        public StaminaPrefix4() : base(8, "Overcharged", "+8 stamina regeneration") { }
    }
}
