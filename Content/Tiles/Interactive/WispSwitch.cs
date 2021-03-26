using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faeflame;

namespace StarlightRiver.Content.Tiles.Interactive
{
    internal class WispSwitch : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.InteractiveTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<WispSwitchEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("");//Map name
            AddMapEntry(new Color(0, 0, 0), name);
            dustType = DustType<Dusts.GoldWithMovement>();
            disableSmartCursor = true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].frameX == 0 && Main.tile[i, j].frameY == 0)
            {
                Tile tile = Main.tile[i, j];
                int left = i - tile.frameX / 18;
                int top = j - tile.frameY / 18;
                int index = GetInstance<WispSwitchEntity>().Find(left, top);

                if (index == -1)
                    return true;
                WispSwitchEntity altarentity = (WispSwitchEntity)TileEntity.ByID[index];

                int timer = altarentity.timer;
                Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;
                Color color = Color.White * (0.2f + timer / 300f * 0.8f);

                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchReal"), pos, Lighting.GetColor(i, j));
                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow0"), pos - Vector2.One, Helper.IndicatorColor);
                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow1"), pos, color);
                if (timer > 0)
                    spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2"), pos + Vector2.One * 16, new Rectangle(0, 0, 96, 96), Color.LightYellow * (timer / 300f), 0, new Vector2(48, 48), timer * 0.002f, 0, 0);
            }

            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            //TODO scalie impl
            //Item.NewItem(new Vector2(i * 16, j * 16), 32, 48, ItemType<Items.Debug.DebugPotion>());
        }
    }

    public class WispSwitchEntity : ModTileEntity
    {
        public int timer = 0;

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == TileType<WispSwitch>() && tile.frameX == 0 && tile.frameY == 0;
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
            if (Main.player.Any(player => Vector2.Distance(player.Center, Position.ToVector2() * 16) <= 100 && player.ActiveAbility<Wisp>()) && timer == 0)
                timer = 300;
            if (timer > 0)
            {
                timer--;
                if (timer == 299 || timer == 1)
                    Wiring.TripWire(Position.X, Position.Y, 2, 2);

                Dust.NewDust(Position.ToVector2() * 16 + new Vector2(10, 10), 2, 2, DustType<Dusts.GoldWithMovement>(), 0, 0, 0, default, timer / 300f);
                Lighting.AddLight(Position.ToVector2() * 16 + new Vector2(10, 10), new Vector3(10, 8, 2) * timer / 300f * 0.06f);
            }
        }
    }
}