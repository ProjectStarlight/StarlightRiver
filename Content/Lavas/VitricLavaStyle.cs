using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Lavas
{
	class VitricLavaStyle : LavaStyle
    {
        public override int ChooseWaterfallStyle() => 0;

        public override int GetSplashDust() => 0;

        public override int GetDropletGore() => 0;

        public override bool ChooseLavaStyle()
        {
            BiomeHandler modPlayer = Main.LocalPlayer.GetModPlayer<BiomeHandler>();
            return modPlayer.ZoneGlass || modPlayer.FountainVitric;
        }

        public override bool SafeAutoload(ref string name, ref string texture, ref string blockTexture)
        {
            texture = "StarlightRiver/Assets/Waters/LavaVitric";
            blockTexture = texture + "_Block";
            return true;
        }

        public override bool DrawEffects(int x, int y)
        {
            var tile = Framing.GetTileSafely(x, y - 1);

            if (Main.rand.Next(45) == 0 && tile.liquid == 0 && tile.collisionType != 1)
                Dust.NewDustPerfect(new Vector2(x, y + 1) * 16, ModContent.DustType<Dusts.LavaSpark>(), -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(2, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.2f, 0.3f));

            if (tile.liquid > 0)
            {


                Main.spriteBatch.Draw(Main.magicPixel, new Rectangle((x + (int)Helpers.Helper.TileAdj.X) * 16 - (int)Main.screenPosition.X, (y + (int)Helpers.Helper.TileAdj.Y) * 16 - (int)Main.screenPosition.Y, 16, 16), new Color(255, 175, 0) * GetOpacity(x, y));
            }

            return true;
        }

        public override void DrawBlockEffects(int x, int y, Tile up, Tile left, Tile right, Tile down)
        {
            float opacity = 0;

            if (!left.active() && left.liquid > 0)
                opacity = GetOpacity(x - 1, y);

            else if (!right.active() && right.liquid > 0)
                opacity = GetOpacity(x + 1, y);

            else if (!up.active() && up.liquid > 0)
                opacity = GetOpacity(x, y - 1);

            Main.spriteBatch.Draw(Main.magicPixel, new Rectangle((x + (int)Helpers.Helper.TileAdj.X) * 16 - (int)Main.screenPosition.X, (y + (int)Helpers.Helper.TileAdj.Y) * 16 - (int)Main.screenPosition.Y, 16, 16), new Color(255, 175, 0) * opacity);
        }

        private float GetOpacity(int x, int y)
		{
            float opacity = 0;

            int up = 0;
            while (Framing.GetTileSafely(x, y - up).liquid > 0 && opacity <= 0.5f)
            {
                opacity += 0.075f;
                up++;
            }

            return opacity;
        }
	}
}
