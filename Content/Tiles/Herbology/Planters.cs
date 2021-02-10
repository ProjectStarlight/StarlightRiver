using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Herbology
{
    internal class Soil : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileMerge[Type][mod.GetTile("Trellis").Type] = true;
            drop = mod.ItemType("Soil");
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Rich Soil");
            AddMapEntry(new Color(56, 33, 33), name);
        }
    }

    internal class Trellis : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileMerge[Type][mod.GetTile("Soil").Type] = true;
            drop = mod.ItemType("Soil");

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Rich Soil");
            AddMapEntry(new Color(56, 33, 33), name);
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Tiles/Herbology/Post"), new Vector2((i + 12) * 16, (j + 9) * 16) - Main.screenPosition, Lighting.GetColor(i, j));
        }
    }

    public class Planter : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Planter");
            AddMapEntry(new Color(103, 92, 73), name);
            disableSmartCursor = true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 16, 32, mod.ItemType("Planter"));
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            Main.tile[i, j].frameX = 0;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j + 1].active() == false)
                switch (Main.tile[i, j].frameX / 18)
                {
                    case 0: break;
                    case 1: WorldGen.PlaceTile(i, j + 1, mod.TileType("ForestIvy"), true); break;
                }
        }

        public override bool NewRightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            if (player.HeldItem.type == mod.ItemType("IvySeeds") && Main.tile[i, j].frameX == 0) //plants ivy
                Main.tile[i, j].frameX = 18;
            return true;
        }
    }

    internal class GreenhouseWall : ModWall
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.wallHouse[Type] = true;
            drop = mod.ItemType("Greenhouse");
        }
    }
}