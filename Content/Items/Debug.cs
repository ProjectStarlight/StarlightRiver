using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items
{
	class DebugStick : ModItem
    {
        public override string Texture => AssetDirectory.Assets+ "Items/DebugStick";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Debug Stick");
            Tooltip.SetDefault("Cooming and Cooming");
        }

        public override void SetDefaults()
        {
            item.damage = 10;
            item.melee = true;
            item.width = 38;
            item.height = 38;
            item.useTime = 18;
            item.useAnimation = 18;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 5f;
            item.value = 1000;
            item.rare = ItemRarityID.LightRed;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;
            item.accessory = true;

            //item.createTile = ModContent.TileType<Tiles.Vitric.ForgeActor>();
        }

        public override bool UseItem(Player player)
        {
            for (int x = 0; x < Main.maxTilesX; x++)
                for (int y = 0; y < Main.maxTilesY; y++)
                    Main.tile[x, y].ClearEverything();

            return true;

            Player dummy = new Player();
            dummy.active = true;

            var item = new Item();
            item.SetDefaults(ModContent.ItemType<UndergroundTemple.TempleRune>());

            dummy.armor[5] = item;
            dummy.Center = Main.LocalPlayer.Center;

            Main.player[1] = dummy;
            dummy.team = Main.LocalPlayer.team;
            dummy.name = "Johnathan Testicle";
            dummy.statLife = 400;
            dummy.statLifeMax = 500;

            return true;

            /*
            foreach (NPC npc in Main.npc)
                npc.active = false;

            NPC.NewNPC((StarlightWorld.VitricBiome.X) * 16, (StarlightWorld.VitricBiome.Center.Y + 10) * 16, ModContent.NPCType<Bosses.GlassMiniboss.GlassweaverWaiting>());
            player.Center = new Vector2((StarlightWorld.VitricBiome.X) * 16, (StarlightWorld.VitricBiome.Center.Y + 10) * 16);

            return true;*/
        }
    }
}
