using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Core
{
    internal abstract class WalkableCrystal : DummyTile
    {
        protected readonly string ItemName;
        protected int ItemType;
        protected readonly string StructurePath;
        protected string FullStructPath;
        public readonly int VariantCount;
        protected readonly Color? MapColor;
        protected readonly int DustType;
        protected readonly int Sound;
        protected readonly int SoundStyle;
        public readonly int MaxWidth;
        public readonly int MaxHeight;
        protected readonly string TexturePath;
        public readonly string DummyName;


        public override int DummyType => mod.ProjectileType(DummyName);

        public override bool SpawnConditions(int i, int j) => Main.tile[i, j].frameX > 0;

        protected WalkableCrystal(int maxWidth, int maxHeight, string dummyType, string path = null, string structurePath = null, int variantCount = 1, string drop = null, int dust = 0, Color? mapColor = null, int sound = 1, int soundStyleVar = 1)
        {
            ItemName = drop;
            TexturePath = path;
            StructurePath = structurePath;
            VariantCount = variantCount;
            MapColor = mapColor;
            DustType = dust;
            MaxHeight = maxHeight;
            MaxWidth = maxWidth;
            Sound = sound;
            SoundStyle = soundStyleVar;
            DummyName = dummyType;
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            if (!string.IsNullOrEmpty(TexturePath))
                texture = TexturePath + name;

            string suffix = name + (VariantCount > 1 ? "_" : string.Empty);
            FullStructPath = (string.IsNullOrEmpty(StructurePath) ? AssetDirectory.StructureFolder : StructurePath) + suffix;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            (this).QuickSet(int.MaxValue, DustType, Sound, MapColor ?? Color.Transparent, -1, default, default, default, SoundStyle);
            Main.tileBlockLight[Type] = false;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.HookPlaceOverride = new PlacementHook(PostPlace, -1, 0, true);
            TileObjectData.addTile(Type);

            if (!string.IsNullOrEmpty(ItemName))
                ItemType = mod.ItemType(ItemName);

            SafeSetDefaults();
        }

        public virtual void SafeSetDefaults() { }

        private int PostPlace(int x, int y, int type, int style, int dir)
        {
            if (style < VariantCount)
            {
                Point16 offset = new Point16((MaxWidth / 2) - 1, MaxHeight - 1);
                if (VariantCount > 1)//if statement because the ternary was acting weird
                    StructureHelper.Generator.GenerateStructure(FullStructPath + style, new Point16(x, y) - offset, StarlightRiver.Instance);
                else
                    StructureHelper.Generator.GenerateStructure(FullStructPath, new Point16(x, y) - offset, StarlightRiver.Instance);
            }
            return 0;
        }

        public override bool Drop(int i, int j)
        {
            if (Main.tile[i, j].frameX > 0)
                Item.NewItem(i * 16, j * 16, 16 * MaxWidth, 16 * MaxHeight, ItemType);
            return false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => fail = true;
        public override bool CanExplode(int i, int j) => false;
        public override bool Slope(int i, int j) => false;
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
    }

    internal abstract class WalkableCrystalDummy : Dummy //extend from this and override PostDraw to change stuff
    {
        public readonly int variantCount;
        public virtual Vector2 DrawOffset => new Vector2(8, 18);
        public virtual Color DrawColor => Color.White;

        public WalkableCrystalDummy(int validType, int VariantCount = 1) : base(validType, 16, 16) { variantCount = VariantCount; }

        public override void SafeSetDefaults()
        {
            projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Tile t = Parent;

            if (t.frameX > 0 && variantCount > 0)
            {
                Texture2D tex = Main.tileTexture[t.type];
                Rectangle frame = tex.Frame(variantCount, 1, t.frameX - 1);
                Vector2 pos = ((projectile.position - Main.screenPosition) + DrawOffset) - new Vector2(frame.Width * 0.5f, frame.Height);
                LightingBufferRenderer.DrawWithLighting(pos, tex, frame, DrawColor);

            }
        }
    }
}