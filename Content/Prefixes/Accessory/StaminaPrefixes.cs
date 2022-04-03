using StarlightRiver.Content.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Prefixes.Accessory
{
	internal abstract class StaminaPrefix : CustomTooltipPrefix
    {
        private readonly int power;
        private readonly string name;

        internal StaminaPrefix(int power, string name, string tip) : base(tip)
        {
            this.power = power;
            this.name = name;
        }

        public override bool CanRoll(Item Item) => Item.accessory;

        public override PrefixCategory Category => PrefixCategory.Accessory;

        public override void SetDefaults() => DisplayName.SetDefault(name);

        public override void ModifyValue(ref float valueMult) => valueMult *= 1 + 0.05f * power;

        public override void Update(Item Item, Player Player)
        {
            Player.GetHandler().StaminaRegenRate += power;
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
