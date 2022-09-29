﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Interactive
{
	internal class WispSwitch : ModTile
    {
        public override string Texture => AssetDirectory.InteractiveTile + Name;

        public override void SetStaticDefaults()
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
            DustType = DustType<Dusts.GoldWithMovement>();
            
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
            {
                Tile tile = Main.tile[i, j];
                int left = i - tile.TileFrameX / 18;
                int top = j - tile.TileFrameY / 18;
                int index = GetInstance<WispSwitchEntity>().Find(left, top);

                if (index == -1)
                    return true;
                WispSwitchEntity altarentity = (WispSwitchEntity)TileEntity.ByID[index];

                int timer = altarentity.timer;
                Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;
                Color color = Color.White * (0.2f + timer / 300f * 0.8f);

                spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/WispSwitchReal").Value, pos, Lighting.GetColor(i, j));
                spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow0").Value, pos - Vector2.One, Helper.IndicatorColorProximity(150, 300, new Vector2(i, j) * 16 + Vector2.One * 16));
                spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow1").Value, pos, color);
                if (timer > 0)
                    spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2").Value, pos + Vector2.One * 16, new Rectangle(0, 0, 96, 96), Color.LightYellow * (timer / 300f), 0, new Vector2(48, 48), timer * 0.002f, 0, 0);
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

        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.HasTile && tile.TileType == TileType<WispSwitch>() && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }

		public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
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
            if (Main.player.Any(Player => Vector2.Distance(Player.Center, Position.ToVector2() * 16) <= 100 && Player.ActiveAbility<Whip>()) && timer == 0)
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