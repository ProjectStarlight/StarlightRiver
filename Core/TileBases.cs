using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Core
{
	public abstract class ModFountain : ModTile
    {
        protected readonly string ItemName;
        protected int ItemType;
        protected readonly int FrameCount;
        protected readonly Color? MapColor;
        protected readonly int DustType;
        protected readonly int Width;
        protected readonly int Height;
        protected readonly string TexturePath;

        protected ModFountain(string drop, string path = null, int animFrameCount = 6, int dust = 1, Color? mapColor = null, int width = 2, int height = 4)
        {
            ItemName = drop;
            TexturePath = path;
            FrameCount = animFrameCount;
            MapColor = mapColor;
            DustType = dust;
            Height = height;
            Width = width;
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            if (!string.IsNullOrEmpty(TexturePath))
                texture = TexturePath + name;

            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Width = Width;
            TileObjectData.newTile.Height = Height;
            TileObjectData.newTile.Origin = new Point16(Width / 2, Height / 2 + 1);
            TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, Height).ToArray();
            TileObjectData.addTile(Type);

            dustType = DustType;
            animationFrameHeight = Height * 18;
            disableSmartCursor = true;
            ItemType = mod.ItemType(ItemName);
            AddMapEntry(MapColor ?? new Color(75, 139, 166));

            adjTiles = new int[] { TileID.WaterFountain };
        }

        public override bool HasSmartInteract() => true;

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => 
            Item.NewItem(i * 16, j * 16, 32, 48, ItemType);

        public override void NumDust(int i, int j, bool fail, ref int num) => 
            num = 1;

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++frameCounter * 0.2f >= FrameCount)
                frameCounter = 0;
            frame = (int)(frameCounter * 0.2f);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];

            Vector2 zero = Main.drawToScreen ?
                Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);

            Texture2D texture = Main.canDrawColorTile(i, j) ?
                Main.tileAltTexture[Type, tile.color()] : Main.tileTexture[Type];

            int animate = tile.frameY >= animationFrameHeight ?
                Main.tileFrame[Type] * animationFrameHeight : 0;

            Main.spriteBatch.Draw(texture, new Vector2(i * 16, j * 16) - Main.screenPosition + zero, new Rectangle(tile.frameX, tile.frameY + animate, 16, 16), Lighting.GetColor(i, j), 0f, default, 1f, SpriteEffects.None, 0f);
            return false;
        }

        public override bool NewRightClick(int i, int j)
        {
            Main.PlaySound(SoundID.Mech, i * 16, j * 16, 0);
            HitWire(i, j);
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.showItemIcon = true;
            player.showItemIcon2 = ItemType;
        }

        public override void HitWire(int i, int j)
        {
            int x = i - (Main.tile[i, j].frameX / 18) % Width;
            int y = j - (Main.tile[i, j].frameY / 18) % Height;

            for (int l = x; l < x + Width; l++)
                for (int m = y; m < y + Height; m++)
                {
                    if (Main.tile[l, m] == null)
                        Main.tile[l, m] = new Tile();
                    if (Main.tile[l, m].active() && Main.tile[l, m].type == Type)
                        if (Main.tile[l, m].frameY < (short)animationFrameHeight)
                            Main.tile[l, m].frameY += (short)animationFrameHeight;
                        else
                            Main.tile[l, m].frameY -= (short)animationFrameHeight;
                }

            if (Wiring.running)
                for(int g = 0; g < Width; g++)
                    for (int h = 0; h < Height; h++)
                        Wiring.SkipWire(x + g, y + h);

            NetMessage.SendTileSquare(-1, x, y + 1, 6);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].frameY >= animationFrameHeight)
                FountainActive(i, j, closer);
        }

        /// <summary>
        /// Acts just like nearby effects but takes if the fountain is active into account
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="closer"></param>
        public virtual void FountainActive(int i, int j, bool closer) { }
    }

    public abstract class ModBanner : ModTile
    {
        protected readonly string ItemName;
        protected int ItemType;
        protected readonly int NpcType;
        protected readonly int Width;
        protected readonly int Height;
        protected readonly Color? MapColor;
        protected readonly string TexturePath;

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
        protected readonly string[] AnchorableTiles;
        protected int[] AnchorTileTypes;
        protected readonly int DustType;
        protected readonly int MaxVineLength;
        protected readonly int GrowthChance;//lower is faster (one out of this amount)
        protected readonly Color? MapColor;
        protected readonly string ItemName;
        protected readonly int DustAmount;
        protected readonly int Sound;
        protected readonly string TexturePath;

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