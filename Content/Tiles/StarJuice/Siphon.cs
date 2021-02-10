using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.StarJuice
{
    internal sealed class Siphon : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.StarjuiceTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<SiphonEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.addTile(Type);
            dustType = DustID.Stone;
            disableSmartCursor = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Astral Siphon");
            AddMapEntry(new Color(163, 163, 163), name);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j + 2].type == TileType<CrystalBlock>() && Main.tile[i, j].frameY == 0)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/StarJuice/SiphonGlow");
                spriteBatch.Draw(tex, (new Vector2(i, j) + Helper.TileAdj) * 16 + new Vector2(8, 12) - Main.screenPosition, tex.Frame(), Color.White * 0.8f, 0, tex.Frame().Size() / 2, 1.2f, 0, 0);
            }
        }
    }

    internal sealed class SiphonEntity : ModTileEntity
    {
        private TankEntity tank = null;
        private int timer = 0;
        private int variation = 0;

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.type == TileType<Siphon>() && tile.active() && tile.frameX == 0 && tile.frameY == 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i, j);
        }

        public override void Update()
        {
            if (!Main.tile[Position.X, Position.Y].active()) Kill(Position.X, Position.Y);

            if (tank == null)
                for (int i = -6; i <= 6; i++)
                    for (int j = -6; j <= 6; j++)
                    {
                        int index = GetInstance<TankEntity>().Find(Position.X + i, Position.Y + j);
                        if (index != -1) { tank = (TankEntity)ByID[index]; i = 7; j = 7; }
                    }
            else if (Main.tile[Position.X, Position.Y + 2].type == TileType<CrystalBlock>() && tank.charge < tank.maxCharge)
            {
                Vector2 pos = Position.ToVector2() * 16 + Vector2.One * 8;
                Vector2 tankpos = tank.Position.ToVector2() * 16 + new Vector2(24, -16);

                timer++;
                if (timer > 100 + variation)
                    Dust.NewDustPerfect(Vector2.Lerp(pos, tankpos, (timer - (100 + variation)) / 20f), DustType<Dusts.Starlight>());

                if (timer >= 120 + variation)
                {
                    timer = 0;
                    tank.charge += 10;
                    variation = Main.rand.Next(-30, 30);
                }
            }
        }
    }
}