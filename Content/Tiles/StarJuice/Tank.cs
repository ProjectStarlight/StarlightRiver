using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.StarJuice;

namespace StarlightRiver.Content.Tiles.StarJuice
{
    internal sealed class Tank : ModTile
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
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(GetInstance<TankEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.addTile(Type);
            dustType = DustID.Stone;
            disableSmartCursor = true;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Astral Altar");
            AddMapEntry(new Color(163, 163, 163), name);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            //Item.NewItem(i * 16, j * 16, 32, 32, ItemType<OvenItem>());
        }

        public override bool NewRightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            int index = GetInstance<TankEntity>().Find(i - tile.frameX / 18 * 16, j - tile.frameY / 18 * 16);
            if (index == -1) return true;
            TankEntity entity = (TankEntity)TileEntity.ByID[index];

            if (player.HeldItem.modItem is StarjuiceStoringItem)
                (player.HeldItem.modItem as StarjuiceStoringItem).Refuel(entity);

            return true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            int index = GetInstance<TankEntity>().Find(i, j);
            if (index == -1) return true;
            TankEntity entity = (TankEntity)TileEntity.ByID[index];

            if (tile.frameX == 0 && tile.frameY == 0)
            {
                Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 + new Vector2(8, -28) - Main.screenPosition;
                int charge = (int)(entity.charge / 5000f * 32f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);

                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/RiftCrafting/Glow0"), pos + Vector2.One * -16, new Color(80, 150, 200) * (entity.charge / 5000f * 0.7f));

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/StarJuice/OrbIn"), new Rectangle((int)pos.X, (int)pos.Y + (32 - charge), 32, charge),
                    new Rectangle(0, 0, 32, charge), Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically, 0);

                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/StarJuice/OrbOut"), pos, Lighting.GetColor(i + 1, j - 2));

                if (new Rectangle(i * 16, (j - 2) * 16, 48, 64).Contains(Main.MouseWorld.ToPoint()))
                {
                    string counter = entity.charge + "/" + entity.maxCharge;
                    float scale = 0.7f;
                    spriteBatch.DrawString(Main.fontMouseText, counter, pos + new Vector2(16 - Main.fontMouseText.MeasureString(counter).X * scale / 2, -24),
                        Main.mouseTextColorReal * 0.75f, 0, Vector2.Zero, scale, 0, 0);
                }
            }
            return true;
        }
    }

    internal sealed class TankEntity : ModTileEntity
    {
        public int charge = 0;
        public int maxCharge = 5000;

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.type == TileType<Tank>() && tile.active() && tile.frameX == 0 && tile.frameY == 0;
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

            Vector2 pos = Position.ToVector2() * 16 + new Vector2(24, -12);
            Lighting.AddLight(pos, new Vector3(1.2f, 1.6f, 2) * (charge / (float)maxCharge) * 0.5f);

            if (!Main.dayTime && charge < maxCharge)
            {
                float rot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(pos + Vector2.One.RotatedBy(rot) * 20, DustType<Dusts.Starlight>(), Vector2.One.RotatedBy(rot) * -10, 0, default, 0.5f);

                if (Main.time % 10 == 0 && !Main.fastForwardTime) charge++;
            }

            if (charge == maxCharge)
                for (int k = 0; k < 4; k++)
                    Dust.NewDustPerfect(pos, DustType<Dusts.Starlight>(), Vector2.One.RotatedBy(StarlightWorld.rottime + 1.58f * k) * 5, 0, default, 0.7f);

            if (charge > maxCharge) charge = maxCharge;
        }

        public override TagCompound Save()
        {
            return new TagCompound
            {
                ["Charge"] = charge
            };
        }

        public override void Load(TagCompound tag)
        {
            charge = tag.GetInt("Charge");
        }
    }
}