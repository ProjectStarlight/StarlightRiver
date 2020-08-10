using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Fountains
{
    // Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleCorrupt
    public abstract class BiomeFountain : ModTile
    {
        private readonly int ItemType;

        protected BiomeFountain(int item)
        {
            ItemType = item;
        }

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(75, 139, 166));
            dustType = 1;
            animationFrameHeight = 72;
            disableSmartCursor = true;
            adjTiles = new int[] { TileID.WaterFountain };
        }

        public override bool HasSmartInteract()
        {
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 32, 48, ItemType);
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frame = Main.tileFrame[TileID.WaterFountain];
            frameCounter = Main.tileFrameCounter[TileID.WaterFountain];
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Texture2D texture;
            if (Main.canDrawColorTile(i, j))
            {
                texture = Main.tileAltTexture[Type, tile.color()];
            }
            else
            {
                texture = Main.tileTexture[Type];
            }
            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
            {
                zero = Vector2.Zero;
            }
            int animate = 0;
            if (tile.frameY >= 72)
            {
                animate = Main.tileFrame[Type] * animationFrameHeight;
            }
            Main.spriteBatch.Draw(texture, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.frameX, tile.frameY + animate, 16, 16), Lighting.GetColor(i, j), 0f, default, 1f, SpriteEffects.None, 0f);
            return false;
        }

        public override bool NewRightClick(int i, int j)
        {
            Main.PlaySound(SoundID.Mech, i * 16, j * 16, 0);
            HitWire(i, j);
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = ItemType;
        }

        public override void HitWire(int i, int j)
        {
            int x = i - (Main.tile[i, j].frameX / 18) % 2;
            int y = j - (Main.tile[i, j].frameY / 18) % 4;
            for (int l = x; l < x + 2; l++)
            {
                for (int m = y; m < y + 4; m++)
                {
                    if (Main.tile[l, m] == null)
                    {
                        Main.tile[l, m] = new Tile();
                    }
                    if (Main.tile[l, m].active() && Main.tile[l, m].type == Type)
                    {
                        if (Main.tile[l, m].frameY < 72)
                        {
                            Main.tile[l, m].frameY += 72;
                        }
                        else
                        {
                            Main.tile[l, m].frameY -= 72;
                        }
                    }
                }
            }
            if (Wiring.running)
            {
                Wiring.SkipWire(x, y);
                Wiring.SkipWire(x, y + 1);
                Wiring.SkipWire(x, y + 2);
                Wiring.SkipWire(x, y + 3);
                Wiring.SkipWire(x + 1, y);
                Wiring.SkipWire(x + 1, y + 1);
                Wiring.SkipWire(x + 1, y + 2);
                Wiring.SkipWire(x + 1, y + 3);
            }
            NetMessage.SendTileSquare(-1, x, y + 1, 3);
        }
    }

    public class JungleCorruptFountain : BiomeFountain
    {
        public JungleCorruptFountain() : base(ItemType<Items.Fountains.JungleCorruptFountainItem>())
        {
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].frameY >= 72)
            {
                Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleCorrupt = true;
            }
        }
    }

    public class JungleBloodyFountain : BiomeFountain
    {
        public JungleBloodyFountain() : base(ItemType<Items.Fountains.JungleBloodyFountainItem>())
        {
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].frameY >= 72)
            {
                Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleBloody = true;
            }
        }
    }

    public class JungleHolyFountain : BiomeFountain
    {
        public JungleHolyFountain() : base(ItemType<Items.Fountains.JungleHolyFountainItem>())
        {
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].frameY >= 72)
            {
                Main.LocalPlayer.GetModPlayer<BiomeHandler>().FountainJungleHoly = true;
            }
        }
    }
}