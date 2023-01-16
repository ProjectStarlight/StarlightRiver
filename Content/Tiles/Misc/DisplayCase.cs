using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
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
        public override string Texture => "StarlightRiver/Assets/Tiles/Misc/DisplayCase";

        public override void SetStaticDefaults()
        {
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<DisplayCaseEntity>().Hook_AfterPlacement, -1, 0, false);

            QuickBlock.QuickSetFurniture(this, 2, 3, DustID.t_BorealWood, SoundID.Dig, false, new Color(255, 255, 150), false, false, "Relic Case");
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                var outlineTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Misc/DisplayCaseGlow").Value;
                var outlinePos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(1, 3);
                var outlineColor = Helpers.Helper.IndicatorColorProximity(150, 300, new Vector2(i, j) * 16 + Vector2.One * 16);

                spriteBatch.Draw(outlineTex, outlinePos, null, outlineColor);

                int index = ModContent.GetInstance<DisplayCaseEntity>().Find(i, j);

                if (index == -1)
                    return true;

                DisplayCaseEntity entity = (DisplayCaseEntity)TileEntity.ByID[index];

                if (entity.containedItem is null)
                    return true;

                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default);

                var tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
                var pos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition - Vector2.One * 16;
                spriteBatch.Draw(tex2, pos, new Color(255, 255, 200) * (0.9f + ((float)Math.Sin(Main.GameUpdateCount / 50f) * 0.1f)) );

                var texShine = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Shine").Value;
                pos += Vector2.One * 32;

                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 -GetProgress(0)), Main.GameUpdateCount / 250f, new Vector2(texShine.Width / 2, texShine.Height), 0.08f * GetProgress(0), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 - GetProgress(34)), Main.GameUpdateCount / 350f + 2.2f, new Vector2(texShine.Width / 2, texShine.Height), 0.09f * GetProgress(34), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 - GetProgress(70)), Main.GameUpdateCount / 320f + 5.4f, new Vector2(texShine.Width / 2, texShine.Height), 0.09f * GetProgress(70), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 - GetProgress(15)), Main.GameUpdateCount / 300f + 3.14f, new Vector2(texShine.Width / 2, texShine.Height), 0.08f * GetProgress(15), 0, 0);
                spriteBatch.Draw(texShine, pos, null, new Color(255, 255, 200) * (1 - GetProgress(98)), Main.GameUpdateCount / 360f + 4.0f, new Vector2(texShine.Width / 2, texShine.Height), 0.09f * GetProgress(98), 0, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);

                var tex = Terraria.GameContent.TextureAssets.Item[entity.containedItem.type].Value;
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

        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return (tile.TileType == ModContent.TileType<DisplayCase>() || tile.TileType == ModContent.TileType<DisplayCaseFriendly>())
                && tile.HasTile && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i - 1, j - 1, 3);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i - 1, j - 1, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i -1, j - 1);
        }

        public override void Update()
        {
            if (!IsTileValidForEntity(Position.X, Position.Y))
                Kill(Position.X, Position.Y);

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

            if (tile.TileType == ModContent.TileType<DisplayCase>())
            {
                for (int k = 0; k < Main.maxPlayers; k++)
                {
                    var Player = Main.player[k];
                    if (AbilityHelper.CheckDash(Player, new Rectangle(Position.X * 16, Position.Y * 16, 32, 48)))
                    {
                        WorldGen.KillTile(Position.X, Position.Y);
                        Helpers.Helper.NewItemSpecific(Player.Center, containedItem);
                        Kill(Position.X, Position.Y);

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Player.Center);

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

        public override void SaveData(TagCompound tag)
        {
            if(containedItem != null)
                tag["Item"] = containedItem;
        }

        public override void LoadData(TagCompound tag)
        {
            containedItem = tag.Get<Item>("Item");
        }
    }
}
