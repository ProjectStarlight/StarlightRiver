using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Vitric
{
    class VitricBossBag : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override int BossBagNPC => NPCType<Bosses.GlassBoss.VitricBoss>();

        public override void SetStaticDefaults() => DisplayName.SetDefault("Treasure Bag");

        public override void SetDefaults()
        {
            item.consumable = true;
            item.rare = ItemRarityID.Expert;
            item.expert = true;
        }

        public override bool CanRightClick() => true;

        public override void OpenBossBag(Player player)
        {
            int weapon = Main.rand.Next(5);

            for (int k = 0; k < 2; k++) //PORT: k < Main.MasterMode ? 3 : 2
            {
                switch (weapon % 5)
                {
                    case 0: Item.NewItem(player.Center, ItemType<Vitric.VitricPick>()); break;
                    case 1: Item.NewItem(player.Center, ItemType<Vitric.VitricHamaxe>()); break;
                    case 3: Item.NewItem(player.Center, ItemType<Vitric.VitricSword>()); break;
                    case 4: Item.NewItem(player.Center, ItemType<Vitric.VitricBow>()); break;
                }
                weapon++;
            }

            Item.NewItem(player.Center, ItemType<VitricOre>(), Main.rand.Next(40, 60));
            Item.NewItem(player.Center, ItemType<Misc.StaminaUp>());
            Item.NewItem(player.Center, ItemType<CeirosExpert>());
        }
    }
}
