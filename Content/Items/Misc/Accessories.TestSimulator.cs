using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class TestSimulator : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + "StarGlaive";

        public override List<int> ChildTypes => new List<int>()
        {
            ModContent.ItemType<SwordBook>(),
            ModContent.ItemType<Guillotine>()
        };

		public TestSimulator() : base("Testificate Charm", "debug item\nCombines the effects of Mantis Technique and Golden Guillotine\nAdditionally, boosts life by 100") { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
        }

		public override void OnEquip(Player player, Item item)
		{
            Main.NewText("We just equipped!");
		}

		public override void SafeUpdateEquip(Player Player)
        {
            Player.statLifeMax2 += 100;
        }
    }

    public class TestSimulator3 : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + "StarGlaive";

        public override List<int> ChildTypes => new List<int>()
        {
            ModContent.ItemType<SwordBook>(),
            ModContent.ItemType<StaminaUp>()
        };

        public TestSimulator3() : base("Testificate Charm 3", "debug item\nCombines the effects of Mantis Technique and Stamina Vessel\nAdditionally, boosts life by 100") { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
        }

        public override void OnEquip(Player player, Item item)
        {
            Main.NewText("We just equipped!");
        }

        public override void SafeUpdateEquip(Player Player)
        {
            Player.statLifeMax2 += 100;
        }
    }

    public class TestSimulator2 : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + "StarGlaive";

        public override List<int> ChildTypes => new List<int>()
        {
            ModContent.ItemType<TestSimulator>(),
            ModContent.ItemType<PulseBoots>()
        };

        public TestSimulator2() : base("Testificate Charm 2", "debug item\nCombines the effects of Testificate Charm and Pulse Boots") { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
        }

        public override void OnEquip(Player player, Item item)
        {
            Main.NewText("The bigger accessory just equipped!");
        }

        public override void SafeUpdateEquip(Player Player)
        {
            Player.statLifeMax2 += 100;
        }
    }
}