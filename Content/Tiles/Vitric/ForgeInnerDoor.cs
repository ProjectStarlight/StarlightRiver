using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class ForgeInnerDoor : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            QuickBlock.QuickSetFurniture(this, 2, 5, DustType<Dusts.Stamina>(), SoundID.Tink, false, new Color(200, 150, 80), false, true, "Forge Door");
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Vector2 pos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition;
            var tex = Request<Texture2D>(AssetDirectory.VitricTile + "ForgeInnerDoorGlow").Value;
            var tile = Framing.GetTileSafely(i, j);
            var source = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

            spriteBatch.Draw(tex, pos, source, Color.White * (0.5f + (float)Math.Sin(StarlightWorld.rottime) * 0.25f));
        }
    }

    class ForgeInnerDoorItem : QuickTileItem
    {
        public ForgeInnerDoorItem() : base("Forge Inner Door", "Debug Item", TileType<ForgeInnerDoor>(), 1, AssetDirectory.Debug, true) { }
    }
}
