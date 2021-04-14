using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    internal class GrassOvergrow : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 210, DustType<Dusts.Leaf>(), SoundID.Tink, new Color(202, 157, 49), StarlightRiver.Instance.ItemType("BrickOvergrowItem"), true, true);
            Main.tileMerge[Type][mod.GetTile("BrickOvergrow").Type] = true;
            Main.tileMerge[Type][mod.GetTile("StoneOvergrow").Type] = true;
            Main.tileMerge[Type][mod.GetTile("LeafOvergrow").Type] = true;
            Main.tileMerge[Type][TileType<CrusherTile>()] = true;
            Main.tileMerge[Type][TileType<GlowBrickOvergrow>()] = true;
            TileID.Sets.Grass[Type] = true;
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameX >= 10 && tile.frameX < 70 && tile.frameY == 0)
            {
                Main.specX[nextSpecialDrawIndex] = i;
                Main.specY[nextSpecialDrawIndex] = j;
                nextSpecialDrawIndex++;
            }
        }
        public static void CustomDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Texture2D tex = GetTexture(AssetDirectory.OvergrowTile + "GrassOvergrowMoss");
            Rectangle source = new Rectangle(0 + i % 5 * 8, 0, 8, 16);
            Color color = Lighting.GetColor(i, j);

            Vector2 crunch = new Vector2(0, -5);
            foreach (Player player in Main.player.Where(n => n.active))
            {
                if (player.Hitbox.Intersects(new Rectangle(i * 16 - 8, j * 16 - 1, 16, 1))) crunch.Y += 1;
                if (player.Hitbox.Intersects(new Rectangle(i * 16, j * 16 - 1, 8, 1))) crunch.Y += 2;
            }

            if (tile.frameX >= 10 && tile.frameX < 70 && tile.frameY == 0)
            {
                spriteBatch.Draw(tex, new Vector2(i, j) * 16 + crunch - Main.screenPosition, source, color);
                spriteBatch.Draw(tex, new Vector2(i + 0.5f, j) * 16 + crunch * 0.5f - Main.screenPosition, source, color);
            }
        }
        public override void FloorVisuals(Player player)
        {
            Vector2 playerFeet = player.Center + new Vector2(-8, player.height / 2);
            if (player.velocity.X != 0)
            {
                if (Main.rand.Next(3) == 0) Dust.NewDust(playerFeet, 16, 1, DustType<Dusts.Stamina>(), 0, -2);
                if (Main.rand.Next(10) == 0) Dust.NewDust(playerFeet, 16, 1, DustType<Dusts.Leaf>(), 0, 0.6f);
            }

            if (player.GetModPlayer<AbilityHandler>().GetAbility<Dash>(out var dash) && dash.Cooldown == 90)
                for (int k = 0; k < 20; k++)
                    Dust.NewDust(playerFeet, 16, 1, DustType<Dusts.Leaf>(), 0, -2);
        }
        public override void RandomUpdate(int i, int j)
        {
            if (!Main.tile[i, j - 1].active())
                if (Main.rand.NextBool())
                    WorldGen.PlaceTile(i, j - 1, TileType<TallgrassOvergrow>(), true);

            if (Main.rand.Next(10) == 0)
                if (!Main.tile[i, j + 1].active() && (Main.tile[i, j].slope() == 0 || Main.tile[i, j].topSlope()))
                    WorldGen.PlaceTile(i, j + 1, TileType<VineOvergrow>(), true);
        }
    }
    internal class GrassOvergrowItem : QuickTileItem { public GrassOvergrowItem() : base("Overgrowth Grass", "They have a pulse...", ModContent.TileType<GrassOvergrow>(), 1, AssetDirectory.OvergrowTile) { } }
}