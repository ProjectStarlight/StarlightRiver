using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using StarlightRiver.Content.Tiles.JungleCorrupt;
using Terraria.Enums;

namespace StarlightRiver.Core
{
    public abstract class ModBanner : ModTile
    {
        private readonly string ItemName;
        private int ItemType;
        private readonly int NpcType;
        private readonly int Width;
        private readonly int Height;
        private readonly Color? MapColor;
        private readonly string TexturePath;
        public ModBanner(string drop, int npcType, string path = null, int width = 1, int height = 3, Color? mapColor = null)
        {
            ItemName = drop;
            NpcType = npcType;
            Width = width;
            Height = height;
            MapColor = mapColor;
            TexturePath = path;
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            if(!string.IsNullOrEmpty(TexturePath))
                texture = TexturePath + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, Height).ToArray();
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            //disableSmartCursor = true;
            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Banner");
            AddMapEntry(MapColor ?? new Color(13, 88, 130));
            ItemType = mod.ItemType(ItemName);
            dustType = -1;

            SafeSetDefaults();
        }

        public virtual void SafeSetDefaults() { }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
            {
                Player player = Main.LocalPlayer;
                player.NPCBannerBuff[NpcType] = true;
                player.hasBanner = true;
            }
        }

        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        {
            if (i % 2 == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => 
            Item.NewItem(i * 16, j * 16, 16 * Width, 16 * Height, ItemType);
    }


    public abstract class ModVine : ModTile
    {
        private readonly string[] AnchorableTiles;
        private int[] AnchorTileTypes;
        private readonly int DustType;
        private readonly int MaxVineLength;
        private readonly int GrowthChance;//lower is faster (one out of this amount)
        private readonly Color? MapColor;
        private readonly string ItemName;
        private readonly int DustAmount;
        private readonly int Sound;
        private readonly string TexturePath;

        public ModVine(string[] anchorableTiles, int dustType, Color? mapColor = null, int growthChance = 10, int maxVineLength = 9, string drop = null, int dustAmount = 1, int soundType = SoundID.Grass, string path = null)
        {
            AnchorableTiles = anchorableTiles;
            DustType = dustType;
            MapColor = mapColor;
            GrowthChance = growthChance;
            MaxVineLength = maxVineLength;
            ItemName = drop;
            DustAmount = dustAmount;
            Sound = soundType;
            TexturePath = path;
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            if (!string.IsNullOrEmpty(TexturePath))
                texture = TexturePath + name;
            return base.Autoload(ref name, ref texture);
        }

        public sealed override void SetDefaults()
        {
            AnchorTileTypes = new int[AnchorableTiles.Length + 1];
            for (int i = 0; i < AnchorableTiles.Length; i++)
                AnchorTileTypes[i] = mod.TileType(AnchorableTiles[i]);
            AnchorTileTypes[AnchorableTiles.Length] = Type;

            Main.tileSolid[Type] = false;
            Main.tileCut[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;

            //this TileObjectData stuff is *only* needed for placing with an item
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.AnchorAlternateTiles = AnchorTileTypes;
            TileObjectData.addTile(Type);

            if(MapColor != null)
                AddMapEntry(MapColor ?? Color.Transparent);
            if(ItemName != null)
                drop = mod.ItemType(ItemName);
            dustType = DustType;
            soundType = Sound;

            SafeSetDefaults();
        }

        public virtual void SafeSetDefaults() { }

        public override void NumDust(int i, int j, bool fail, ref int num) =>
            num = DustAmount;

        public sealed override void RandomUpdate(int i, int j)
        {
            Grow(i, j, GrowthChance);
            SafeRandomUpdate(i, j);
        }
        protected void Grow(int i, int j, int chance)
        {
            if (!Main.tile[i, j + 1].active() && Main.tile[i, j - MaxVineLength].type != Type && Main.rand.Next(chance) == 0)
                WorldGen.PlaceTile(i, j + 1, Type, true);
        }

        public virtual void SafeRandomUpdate(int i, int j) { }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            if (!Main.tile[i, j - 1].active() && !AnchorTileTypes.Contains(Main.tile[i, j - 1].type))
                WorldGen.KillTile(i, j);
                //WorldGen.SquareTileFrame(i, j, true);
            return true;
        }
    }
}