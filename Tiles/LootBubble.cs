using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles
{
    abstract class LootBubble : LootChest
    {
        public virtual string BubbleTexture => "StarlightRiver/Tiles/Bubble";

        public override bool NewRightClick(int i, int j) => false;

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public virtual void PickupEffects(Vector2 origin)
        {
            //Main.PlaySound( , origin);

            for(int k = 0; k < 50; k++)
                Dust.NewDustPerfect(origin, DustType<Dusts.BlueStamina>(), Vector2.One.RotatedByRandom(3.14f) * Main.rand.NextFloat(10), 0, default, 0.5f);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            for(int k = 0; k < Main.maxPlayers; k++)
            {
                Player player = Main.player[k];
                if (CanOpen(player) && player.Hitbox.Intersects(new Rectangle(i * 16, j * 16, 16, 16)))
                {
                    Loot loot = GoldLootPool[Main.rand.Next(GoldLootPool.Count)];
                    Item.NewItem(new Vector2(i, j) * 16, loot.Type, loot.GetCount());
                    WorldGen.KillTile(i, j);

                    PickupEffects(new Vector2(i, j) * 16);
                }
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if (tile.frameX != 0 || tile.frameY != 0) return false;

            Texture2D tex = GetTexture(BubbleTexture);
            Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;

            int sin = (int)(Math.Sin(StarlightWorld.rottime) * 10);
            Rectangle bubbleTarget = new Rectangle((int)pos.X - (sin / 2), (int)pos.Y - (sin / 2), 32 + sin, 32 - sin);
            spriteBatch.Draw(tex, bubbleTarget, Color.White);

            int n = Main.rand.Next(GoldLootPool.Count);
            Texture2D tex2 = Helper.GetItemTexture(GoldLootPool[n].Type);
            Rectangle itemTarget = new Rectangle((int)pos.X + 3, (int)pos.Y + 3, 24, 24);
            spriteBatch.Draw(tex2, itemTarget, Color.White);

            return false;
        }
    }
}
