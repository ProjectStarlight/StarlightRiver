using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.CrashTech
{
    class CrashPod : LootChest
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/CrashTech/CrashPod";
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 2, 4, DustID.Fire, SoundID.Tink, false, new Color(255, 200, 40), false, false, "Crashed Pod");
        }

        public override bool CanOpen(Player player) => Helper.HasItem(player, ItemID.ShadowKey, 1);

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.showItemIcon2 = ItemID.ShadowKey;
            player.noThrow = 2;
            player.showItemIcon = true;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tile = Framing.GetTileSafely(i, j);

            var tex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/CrashTech/CrashPodGlow");
            var pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;
            var frame = new Rectangle(tile.frameX, tile.frameY, 16, 16);
            var color = Color.White * (0.4f + (float)Math.Sin(Main.GameUpdateCount / 40f) * 0.25f);

            spriteBatch.Draw(tex, pos, frame, color);
        }

        public override bool NewRightClick(int i, int j)
        {
            var tile = (Tile)Framing.GetTileSafely(i, j).Clone();

            if (CanOpen(Main.LocalPlayer) && tile.frameX < 32)
            {
                for(int x = 0; x < 2; x++)
                    for(int y = 0; y < 4; y++)
                    {
                        int realX = x + i - tile.frameX / 18;
                        int realY = y + j - tile.frameY / 18;

                        Framing.GetTileSafely(realX, realY).frameX += 36;
                    }

                Loot[] smallLoot = new Loot[5];

                List<Loot> types = Helper.RandomizeList(SmallLootPool);
                for (int k = 0; k < 5; k++) smallLoot[k] = types[k];

                UILoader.GetUIState<LootUI>().SetItems(GoldLootPool[Main.rand.Next(GoldLootPool.Count)], smallLoot);
                UILoader.GetUIState<LootUI>().Visible = true;
                return true;
            }
            return false;
        }

        internal override List<Loot> GoldLootPool =>
            new List<Loot>
            {
                new Loot(ItemID.DirtBlock, 1)
            };

        internal override List<Loot> SmallLootPool =>
            new List<Loot>
            {
                new Loot(ItemID.DirtBlock, 10, 20),
                new Loot(ItemID.StoneBlock, 10, 20),
                new Loot(ItemID.Wood, 10, 20),
                new Loot(ItemID.Gel, 10, 20),
                new Loot(ItemID.IronBar, 10, 20)

            };

    }
}
