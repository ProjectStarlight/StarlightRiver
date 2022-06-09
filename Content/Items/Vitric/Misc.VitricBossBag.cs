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
            Item.consumable = true;
            Item.rare = ItemRarityID.Expert;
            Item.expert = true;
            
            Item.maxStack = 999;
        }

        public override bool CanRightClick() => true;

        public override void OpenBossBag(Player Player)
        {
            int weapon = Main.rand.Next(4);

            for (int k = 0; k < 2; k++) //PORT: k < Main.MasterMode ? 3 : 2
            {
                switch (weapon % 4)
                {
                    case 0: Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<FacetAndLattice>()); break;
                    case 1: Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<Coalescence>()); break;
                    case 2: Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<Needler>()); break;
                    case 3: Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<RefractiveBlade>()); break;
                }
                weapon++;
            }

            Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<VitricOre>(), Main.rand.Next(45, 85));
            Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<MagmaCore>(), Main.rand.Next(2, 3));
            Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<Misc.StaminaUp>());
            Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<ShatteredAegis>());

            if (Main.rand.Next(8) == 0)
                Player.QuickSpawnItem(Player.GetSource_OpenItem(Item.type), ItemType<BarrierDye.VitricBossBarrierDye>());
        }
    }
}
