using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	class VitricBossBag : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override int BossBagNPC => NPCType<Bosses.VitricBoss.VitricBoss>();

        public override void SetStaticDefaults() => DisplayName.SetDefault("Treasure Bag");

        public override void SetDefaults()
        {
            item.consumable = true;
            item.rare = ItemRarityID.Expert;
            item.expert = true;
            item.maxStack = 999;
        }

        public override bool CanRightClick() => true;

        public override void OpenBossBag(Player player)
        {
            int weapon = Main.rand.Next(4);

            for (int k = 0; k < 2; k++) //PORT: k < Main.MasterMode ? 3 : 2
            {
                switch (weapon % 4)
                {
                    case 0: Item.NewItem(player.Center, ItemType<BossSpear>()); break;
                    case 1: Item.NewItem(player.Center, ItemType<VitricBossBow>()); break;
                    case 2: Item.NewItem(player.Center, ItemType<Needler>()); break;
                    case 3: Item.NewItem(player.Center, ItemType<RefractiveBlade>()); break;
                }
                weapon++;
            }

            Item.NewItem(player.Center, ItemType<VitricOre>(), Main.rand.Next(45, 85));
            Item.NewItem(player.Center, ItemType<MagmaCore>(), Main.rand.Next(2, 3));
            Item.NewItem(player.Center, ItemType<Misc.StaminaUp>());
            Item.NewItem(player.Center, ItemType<CeirosExpert>());

            if (Main.rand.Next(8) == 0)
                Item.NewItem(player.Center, ItemType<BarrierDye.VitricBossBarrierDye>());
        }
    }
}
