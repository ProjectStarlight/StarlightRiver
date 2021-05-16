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
    public abstract class ModVine : ModTile
    {
        private readonly string[] AnchorableTiles;
        private int[] AnchorTileTypes;
        private readonly int DustType;
        private readonly int MaxVineLength;
        private readonly int GrowthChance;//lower is faster (one out of this amount)
        private readonly Color? MapColor;
        private readonly int ItemType;
        private readonly int DustAmount;
        private readonly int Sound;

        public ModVine(string[] anchorableTiles, int dustType, Color? mapColor = null, int growthChance = 10, int maxVineLength = 9, int drop = 0, int dustAmount = 1, int soundType = SoundID.Grass)
        {
            AnchorableTiles = anchorableTiles;
            DustType = dustType;
            MapColor = mapColor;
            GrowthChance = growthChance;
            MaxVineLength = maxVineLength;
            ItemType = drop;
            DustAmount = dustAmount;
            Sound = soundType;
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
            drop = ItemType;
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