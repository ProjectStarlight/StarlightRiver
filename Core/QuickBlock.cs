using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Core
{
	internal static class QuickBlock
    {
        public static void QuickSet(this ModTile tile, int minPick, int dustType, SoundStyle? hitSound, Color mapColor, int drop, bool dirtMerge = false, bool stone = false, string mapName = "")
        {
            tile.MinPick = minPick;
            tile.DustType = dustType;
            tile.HitSound = hitSound;
            tile.ItemDrop = drop;
            Main.tileMergeDirt[tile.Type] = dirtMerge;
            Main.tileStone[tile.Type] = stone;

            Main.tileSolid[tile.Type] = true;
            Main.tileLighted[tile.Type] = true;
            Main.tileBlockLight[tile.Type] = true;

            ModTranslation name = tile.CreateMapEntryName();
            name.SetDefault(mapName);
            tile.AddMapEntry(mapColor, name);
        }

        public static void QuickSetBar(this ModTile tile, int drop, int dustType, Color? mapColor = null, SoundStyle? hitSound = null)
        {
            Main.tileMergeDirt[tile.Type] = false;
            Main.tileFrameImportant[tile.Type] = true;

            Main.tileSolid[tile.Type] = true;
            Main.tileSolidTop[tile.Type] = true;
            Main.tileNoAttach[tile.Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(tile.Type);

            ModTranslation name = tile.CreateMapEntryName();
            name.SetDefault("Metal Bar"); //all bars are called metal bar in vanilla
            if (mapColor != null)
                tile.AddMapEntry(mapColor ?? Color.Transparent, name);

            if (hitSound.HasValue)
                tile.HitSound = hitSound;

            tile.DustType = dustType;
            tile.ItemDrop = drop;
        }

        public static void QuickSetWall(this ModWall wall, int dustType, SoundStyle hitSound, int drop, bool safe, Color mapColor)
        {
            wall.DustType = dustType;
            wall.HitSound = hitSound;
            wall.ItemDrop = drop;
            Main.wallHouse[wall.Type] = safe;
            wall.AddMapEntry(mapColor);
        }

        public static void QuickSetFurniture(this ModTile tile, int width, int height, int dustType, SoundStyle? hitSound, bool tallBottom, Color mapColor, bool solidTop = false, bool solid = false, string mapName = "", AnchorData bottomAnchor = default, AnchorData topAnchor = default, int[] anchorTiles = null, bool faceDirection = false, bool wallAnchor = false, Point16 Origin = default)
        {
            Main.tileLavaDeath[tile.Type] = false;
            Main.tileFrameImportant[tile.Type] = true;
            Main.tileSolidTop[tile.Type] = solidTop;
            Main.tileSolid[tile.Type] = solid;

            TileObjectData.newTile.Width = width;
            TileObjectData.newTile.Height = height;


            TileObjectData.newTile.CoordinateHeights = new int[height];

            for (int k = 0; k < height; k++)
                TileObjectData.newTile.CoordinateHeights[k] = 16;

            if (tallBottom) //this breaks for some tiles: the two leads are multitiles and tiles with random styles
                TileObjectData.newTile.CoordinateHeights[height - 1] = 18;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = Origin == default(Point16) ? new Point16(width / 2, height - 1) : Origin;

            if (bottomAnchor != default)
                TileObjectData.newTile.AnchorBottom = bottomAnchor;
            /*else
                TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);*/

            if (topAnchor != default)
                TileObjectData.newTile.AnchorTop = topAnchor;

            if (anchorTiles != null)
                TileObjectData.newTile.AnchorAlternateTiles = anchorTiles;

            if (wallAnchor)
                TileObjectData.newTile.AnchorWall = true;

            if (faceDirection)
            {
                TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
                TileObjectData.newTile.StyleHorizontal = true;
                TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
                TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
                TileObjectData.addAlternate(1);
            }


            TileObjectData.addTile(tile.Type);

            ModTranslation name = tile.CreateMapEntryName();
            name.SetDefault(mapName);
            tile.AddMapEntry(mapColor, name);
            tile.DustType = dustType;
            tile.HitSound = hitSound;
        }

        public static void QuickSetFurniture(this ModTile tile, int width, int height, int dustType, SoundStyle hitSound, Color mapColor, int bottomHeight = 16, bool solidTop = false, bool solid = false, string mapName = "", AnchorData bottomAnchor = default, AnchorData topAnchor = default, int[] anchorTiles = null)
        {
            Main.tileLavaDeath[tile.Type] = false;
            Main.tileFrameImportant[tile.Type] = true;
            Main.tileSolidTop[tile.Type] = solidTop;
            Main.tileSolid[tile.Type] = solid;

            TileObjectData.newTile.Width = width;
            TileObjectData.newTile.Height = height;


            TileObjectData.newTile.CoordinateHeights = new int[height];

            for (int k = 0; k < height; k++)
                TileObjectData.newTile.CoordinateHeights[k] = 16;

            TileObjectData.newTile.CoordinateHeights[height - 1] = bottomHeight;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(width / 2, height / 2);

            if (bottomAnchor != default)
                TileObjectData.newTile.AnchorBottom = bottomAnchor;

            if (topAnchor != default)
                TileObjectData.newTile.AnchorTop = topAnchor;

            if (anchorTiles != null)
                TileObjectData.newTile.AnchorAlternateTiles = anchorTiles;


            TileObjectData.addTile(tile.Type);

            ModTranslation name = tile.CreateMapEntryName();
            name.SetDefault(mapName);
            tile.AddMapEntry(mapColor, name);
            tile.DustType = dustType;
            tile.HitSound = hitSound;
        }

        public static void QuickSetPainting(this ModTile tile, int width, int height, int dustType, Color mapColor, string mapName = "Painting", SoundStyle? hitSound = null)
        {
            TileObjectData.newTile.AnchorWall = TileObjectData.Style3x3Wall.AnchorWall;
            tile.QuickSetFurniture(width, height, dustType, hitSound ?? SoundID.Dig, false, mapColor, false, false, mapName);
        }

        public static void QuickSetBreakableVase(this ModTile tile, int dustType, Color mapColor, int randomVariants, int width = 2, int height = 2, SoundStyle? hitSound = null, string mapName = "Pot", bool tileCut = true, bool tallBottom = false, int paddingX = 2, AnchorData bottomAnchor = default, AnchorData topAnchor = default, int[] anchorTiles = null)
        {
            Main.tileCut[tile.Type] = tileCut;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = randomVariants;
            TileObjectData.newTile.CoordinatePadding = paddingX;
            tile.QuickSetFurniture(width, height, dustType, hitSound ?? SoundID.Shatter, tallBottom, mapColor, false, false, mapName, bottomAnchor, topAnchor, anchorTiles);
        }
    }
}
