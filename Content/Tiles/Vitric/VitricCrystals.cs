using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using Terraria.ObjectData;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Vitric
{
    public class LargeCrystalItem : QuickTileItem
    {
        public LargeCrystalItem() : base("Solid Crystal Item", "", "VitricLargeCrystal", ItemRarityID.Blue, AssetDirectory.VitricTile) { }

        public override void SafeSetDefaults() => item.placeStyle = 0;

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
                if (player.controlSmart)
                    item.placeStyle = 2;
                else
                    item.placeStyle = 3;
            else
                if (player.controlSmart)
                    item.placeStyle = 0;
                else
                    item.placeStyle = 1;

            return base.CanUseItem(player);
        }//a
    }

    public class VitricLargeCrystal : VitricCrystal
    {
        public VitricLargeCrystal() : base(13, 19, AssetDirectory.VitricTile, 4) {}
    }

        //internal class VitricLargeCrystal : ModTile
        //{

        //    public override bool Autoload(ref string name, ref string texture)
        //    {
        //        texture = AssetDirectory.VitricTile + name;
        //        return base.Autoload(ref name, ref texture);
        //    }


        //    public override void SetDefaults()
        //    {
        //        (this).QuickSet(int.MaxValue, DustType<Dusts.Air>(), SoundID.CoinPickup, new Color(115, 182, 158), -1);
        //        Main.tileBlockLight[Type] = false;
        //        Main.tileFrameImportant[Type] = true;
        //        TileID.Sets.DrawsWalls[Type] = true;

        //        TileObjectData.newTile.UsesCustomCanPlace = true;
        //        TileObjectData.newTile.HookPlaceOverride = new PlacementHook(PostPlace, -1, 0, true);
        //        TileObjectData.addTile(Type);

        //        Main.tileMerge[Type][TileType<VitricSpike>()] = true;
        //    }

        //    //public override bool Slope(int i, int j) => false;

        //    private int PostPlace(int x, int y, int type, int style, int dir)
        //    {
        //        StructureHelper.StructureHelper.GenerateStructure("Structures/Vitric/WalkableCrystals/Crystal_" + style, new Point16(x, y), StarlightRiver.Instance);
        //        return 0;
        //    }

        //    public override bool CanExplode(int i, int j) => false;

        //    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;


        //    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        //    {
        //        //DebugDraw(i, j, spriteBatch);

        //        Tile t = Main.tile[i, j];
        //        if (t.frameX > 0)
        //        {
        //            Texture2D tex = Main.tileTexture[Type];
        //            Rectangle frame = tex.Frame(4, 1, t.frameX - 1);
        //            spriteBatch.Draw(tex, ((new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition) + new Vector2(9, 18), frame, Color.White/*Color.LightGray*/, 0, new Vector2(frame.Width * 0.5f, frame.Height), 1, 0, 0);
        //            //Helper.DrawWithLighting(((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition, tex); //Subject to change


        //            //lava fade drawing
        //            Texture2D lavaFadeTex = GetTexture(AssetDirectory.VitricTile + "VitricLavaFade");
        //            for (int k = -6; k < 7; k++)
        //                for (int h = -18; h < 1; h++)
        //                {
        //                    if (Main.tile[i + k, j + h].type == Type)
        //                    {
        //                        int val = (int)(Math.Sin(Main.GameUpdateCount * 0.05f + (h + k)) * 20f + 235f);
        //                        Color col = new Color(val, val, val, 0);
        //                        //Main.NewText(val / 255f);
        //                        Tile sideTile = Main.tile[i + k - 1, j + h];
        //                        Tile sideUpTile = Main.tile[i + k - 1, j + h - 1];
        //                        if ((sideUpTile.liquidType() == Tile.Liquid_Lava && sideTile.type != Type) || (sideTile.liquidType() == Tile.Liquid_Lava && sideTile.liquid > 200))
        //                            spriteBatch.Draw(lavaFadeTex, ((new Vector2(i + k, j + h) + Helper.TileAdj) * 16 - Main.screenPosition), null, col, 0, default, new Vector2(val / 255f, 1), SpriteEffects.None, 0);
        //                        sideTile = Main.tile[i + k + 1, j + h];
        //                        sideUpTile = Main.tile[i + k + 1, j + h - 1];
        //                        if ((sideUpTile.liquidType() == Tile.Liquid_Lava && sideTile.type != Type) || (sideTile.liquidType() == Tile.Liquid_Lava && sideTile.liquid > 200))
        //                            spriteBatch.Draw(lavaFadeTex, ((new Vector2(i + k - 2, j + h) + Helper.TileAdj) * 16 - Main.screenPosition) + new Vector2(lavaFadeTex.Width, 0), null, col, 0, new Vector2(lavaFadeTex.Width, 0), new Vector2(val / 255f, 1), SpriteEffects.FlipHorizontally, 0);
        //                    }
        //                }
        //        }
        //    }



        //    //disabled for debug
        //    //public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => fail = true;
        //}

        internal class VitricSmallCrystal : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            (this).QuickSet(int.MaxValue, DustType<Content.Dusts.Air>(), SoundID.CoinPickup, new Color(115, 182, 158), -1);
            Main.tileBlockLight[Type] = false;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;

            Main.tileMerge[Type][TileType<VitricSpike>()] = true;
        }

        public override bool CanExplode(int i, int j) => false;

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => fail = true;

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile t = Main.tile[i, j];
            if (t.frameX > 0)
            {
                Texture2D tex = Main.tileTexture[Type];

                spriteBatch.Draw(tex, ((new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition) + new Vector2(0, 2), tex.Frame(2, 1, t.frameX - 1), Color.LightGray, 0, new Vector2(32, 48), 1, 0, 0);
                //Helper.DrawWithLighting(((new Vector2(i, j) + Helper.TileAdj) * 16) - Main.screenPosition, tex); //Subject to change

                Texture2D tex1 = GetTexture(AssetDirectory.VitricTile + "VitricLavaFade");
                for (int k = -2; k < 2; k++)
                    for (int h = -3; h < 3; h++)
                    {
                        if (Main.tile[i + k, j + h].type == Type)
                        {
                            int val = (int)(Math.Sin(Main.GameUpdateCount * 0.05f + (h + k)) * 20f + 235f);
                            Color col = new Color(val, val, val, 0);
                            //Main.NewText(val / 255f);

                            Tile t1 = Main.tile[i + k - 1, j + h];
                            Tile t0 = Main.tile[i + k - 1, j + h - 1];
                            if ((t0.liquidType() == Tile.Liquid_Lava && t1.type != Type) || (t1.liquidType() == Tile.Liquid_Lava && t1.liquid > 200))
                                spriteBatch.Draw(tex1, ((new Vector2(i + k, j + h) + Helper.TileAdj) * 16 - Main.screenPosition), null, col, 0, default, new Vector2(val / 355f, 1), SpriteEffects.None, 0);
                            t1 = Main.tile[i + k + 1, j + h];
                            t0 = Main.tile[i + k + 1, j + h - 1];
                            if ((t0.liquidType() == Tile.Liquid_Lava && t1.type != Type) || (t1.liquidType() == Tile.Liquid_Lava && t1.liquid > 200))
                                spriteBatch.Draw(tex1, ((new Vector2(i + k - 2, j + h) + Helper.TileAdj) * 16 - Main.screenPosition) + new Vector2(tex1.Width, 0), null, col, 0, new Vector2(tex1.Width, 0), new Vector2(val / 355f, 1), SpriteEffects.FlipHorizontally, 0);
                        }
                    }
            }
        }
    }
}