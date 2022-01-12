using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System.Linq;
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
            item.height = 40;
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

            item.createTile = ModContent.TileType<Tiles.Vitric.VitricDecor2x1>();
        }

        public override bool UseItem(Player player)
        {
            player.GetModPlayer<Abilities.AbilityHandler>().InfusionLimit = 0;
            return true;

            InfusionMaker.visible = !InfusionMaker.visible;
            return true;

            StarlightWorld.FlipFlag(WorldFlags.VitricBossDowned);
            return true;

            player.GetModPlayer<Abilities.AbilityHandler>().Lock<Abilities.ForbiddenWinds.Dash>();
            return true;

            if (ZoomHandler.ExtraZoomTarget == 0.8f)
            {
                ZoomHandler.SetZoomAnimation(0.65f);
                Main.NewText("Zoom: 65%");
            }

            else if (ZoomHandler.ExtraZoomTarget == 0.65f)
            {
                ZoomHandler.SetZoomAnimation(0.5f);
                Main.NewText("Zoom: 50%");
            }

            else if (ZoomHandler.ExtraZoomTarget == 0.5f)
            {
                ZoomHandler.SetZoomAnimation(1);
                Main.NewText("Zoom: Default");
            }

            else
            {
                ZoomHandler.SetZoomAnimation(0.8f);
                Main.NewText("Zoom: 80%");
            }

            return true;

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

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
            return;

			var target = new Rectangle(50, 160, Main.screenWidth / 10, Main.screenHeight / 10);
            var target2 = new Rectangle(50, 160 + Main.screenHeight / 10 * 1 + 20, Main.screenWidth / 10, Main.screenHeight / 10);
            var target3 = new Rectangle(50, 160 + Main.screenHeight / 10 * 2 + 40, Main.screenWidth / 10, Main.screenHeight / 10);

            var targetO = new Rectangle(48, 158, Main.screenWidth / 10 + 4, Main.screenHeight / 10 + 4);
            var targetO2 = new Rectangle(48, 158 + Main.screenHeight / 10 * 1 + 20, Main.screenWidth / 10 + 4, Main.screenHeight / 10 + 4);
            var targetO3 = new Rectangle(48, 158 + Main.screenHeight / 10 * 2 + 40, Main.screenWidth / 10 + 4, Main.screenHeight / 10 + 4);

            spriteBatch.Draw(Main.magicPixel, targetO, Color.Black);
            spriteBatch.Draw(Main.screenTarget, target, Color.White);

            spriteBatch.Draw(Main.magicPixel, targetO2, Color.Black);
            spriteBatch.Draw(StarlightRiver.LightingBufferInstance.ScreenLightingTexture, target2, Color.White);

            spriteBatch.Draw(Main.magicPixel, targetO3, Color.Black);
            spriteBatch.Draw(StarlightRiver.LightingBufferInstance.TileLightingTexture, target3, Color.White);

        }
	}

    class Eraser : ModItem
	{
        public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Eraser");
            Tooltip.SetDefault("Death");
        }

        public override void SetDefaults()
        {
            item.damage = 10;
            item.melee = true;
            item.width = 38;
            item.height = 38;
            item.useTime = 2;
            item.useAnimation = 2;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 5f;
            item.value = 1000;
            item.rare = ItemRarityID.LightRed;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;
            item.accessory = true;
        }

        public override bool UseItem(Player player)
        {
            foreach (NPC npc in Main.npc.Where(n => Vector2.Distance(n.Center, Main.MouseWorld) < 100))
                npc.StrikeNPC(99999, 0, 0, false, false, false);
            return true;
        }
    }
}
