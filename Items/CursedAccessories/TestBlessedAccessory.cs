using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.CursedAccessories
{
    internal class TestBlessedAccessory : BlessedAccessory
    {
        public TestBlessedAccessory() : base(GetTexture("StarlightRiver/Items/CursedAccessories/TestBlessedAccessoryGlow"))
        {
        }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Blessed");
            DisplayName.SetDefault("ExampleBlessedAccessory");
        }

        public override void SafeUpdateEquip(Player player)
        {
        }

        public override bool TestCondition()
        {
            return Main.player[item.owner].statLife < 100;
        }
    }
}