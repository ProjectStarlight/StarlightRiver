using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Misc
{
    class DisplayCase : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Misc/DisplayCase";
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<DisplayCaseEntity>().Hook_AfterPlacement, -1, 0, false);

            QuickBlock.QuickSetFurniture(this, 2, 3, DustID.t_BorealWood, SoundID.Dig, false, new Color(255, 255, 150), false, false, "Relic Case");
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            if (tile.frameX == 0 && tile.frameY == 0)
            {
                int index = ModContent.GetInstance<DisplayCaseEntity>().Find(i, j);

                if (index == -1)
                    return true;

                DisplayCaseEntity entity = (DisplayCaseEntity)TileEntity.ByID[index];

                if (entity.containedItem is null)
                    return true;

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive);

                var tex2 = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
                var pos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition - Vector2.One * 16;
                spriteBatch.Draw(tex2, pos, new Color(255, 255, 200) * (0.9f + ((float)Math.Sin(Main.GameUpdateCount / 50f) * 0.1f)) );

                var texShine = ModContent.GetTexture("StarlightRiver/Assets/Keys/Shine");
                pos += Vector2.One * 32;

                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 -GetProgress(0)), Main.GameUpdateCount / 250f, new Vector2(texShine.Width / 2, texShine.Height), 0.08f * GetProgress(0), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 - GetProgress(34)), Main.GameUpdateCount / 350f + 2.2f, new Vector2(texShine.Width / 2, texShine.Height), 0.09f * GetProgress(34), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 - GetProgress(70)), Main.GameUpdateCount / 320f + 5.4f, new Vector2(texShine.Width / 2, texShine.Height), 0.09f * GetProgress(70), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 - GetProgress(15)), Main.GameUpdateCount / 300f + 3.14f, new Vector2(texShine.Width / 2, texShine.Height), 0.08f * GetProgress(15), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 - GetProgress(98)), Main.GameUpdateCount / 360f + 4.0f, new Vector2(texShine.Width / 2, texShine.Height), 0.09f * GetProgress(98), 0, 0);

                spriteBatch.End();
                spriteBatch.Begin();

                var tex = Main.itemTexture[entity.containedItem.type];
                var target = new Rectangle((i + (int)Helpers.Helper.TileAdj.X) * 16 - (int)Main.screenPosition.X + 4, (j + (int)Helpers.Helper.TileAdj.Y) * 16 - (int)Main.screenPosition.Y + 6, 20, 20);

                spriteBatch.Draw(tex, target, null, Color.White);
            }

            return true;
        }

        private float GetProgress(float off)
        {
            return (Main.GameUpdateCount + off * 3) % 300 / 300f;
        }
    }

    internal sealed class DisplayCaseEntity : ModTileEntity
    {
        public Item containedItem;

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return (tile.type == ModContent.TileType<DisplayCase>() || tile.type == ModContent.TileType<DisplayCaseFriendly>())
                && tile.active() && tile.frameX == 0 && tile.frameY == 0;
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
            if (containedItem != null)
            {
                Lighting.AddLight(Position.ToVector2() * 16, new Vector3(1, 1, 0.7f) * 0.8f * (0.9f + ((float)Math.Sin(Main.GameUpdateCount / 50f) * 0.1f)));

                if (Main.rand.Next(6) == 0)
                {
                    int i = Dust.NewDust(Position.ToVector2() * 16, 32, 32, 228);
                    Main.dust[i].noGravity = true;
                }
            }

            Tile tile = Framing.GetTileSafely(Position.X, Position.Y);

            if (tile.type == ModContent.TileType<DisplayCase>())
            {
                for (int k = 0; k < Main.maxPlayers; k++)
                {
                    var player = Main.player[k];
                    if (AbilityHelper.CheckDash(player, new Rectangle(Position.X * 16, Position.Y * 16, 32, 48)))
                    {
                        WorldGen.KillTile(Position.X, Position.Y);
                        Helpers.Helper.NewItemSpecific(player.Center, containedItem);
                        Kill(Position.X, Position.Y);

                        Main.PlaySound(SoundID.Shatter, player.Center);

                        for (int n = 0; n < 30; n++)
                        {
                            int i = Dust.NewDust(Position.ToVector2() * 16, 32, 32, 228);
                            Main.dust[i].noGravity = true;
                        }

                        break;
                    }
                }
            }
        }

        public override TagCompound Save()
        {
            return new TagCompound
            {
                ["Item"] = containedItem
            };
        }

        public override void Load(TagCompound tag)
        {
            containedItem = tag.Get<Item>("Item");
        }
    }
}
