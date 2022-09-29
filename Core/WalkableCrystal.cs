using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
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
        protected readonly int CrystalDustType;
        protected readonly SoundStyle? CrystalSound;
        public readonly int MaxWidth;
        public readonly int MaxHeight;
        protected readonly string TexturePath;
        public readonly string DummyName;


        public override int DummyType => Mod.Find<ModProjectile>(DummyName).Type;

        public override bool SpawnConditions(int i, int j) => Main.tile[i, j].TileFrameX > 0;

        protected WalkableCrystal(int maxWidth, int maxHeight, string dummyType, string path = null, string structurePath = null, int variantCount = 1, string drop = null, int dust = 0, Color? mapColor = null, SoundStyle? sound = null)
        {
            ItemName = drop;
            TexturePath = path;
            StructurePath = structurePath;
            VariantCount = variantCount;
            MapColor = mapColor;
            CrystalDustType = dust;
            MaxHeight = maxHeight;
            MaxWidth = maxWidth;
            CrystalSound = sound;
            DummyName = dummyType;
        }

        public override string Texture => TexturePath + Name;

        public override void Load()
        {
            string suffix = Name + (VariantCount > 1 ? "_" : string.Empty);
            FullStructPath = (string.IsNullOrEmpty(StructurePath) ? AssetDirectory.StructureFolder : StructurePath) + suffix;
        }

        public override void SetStaticDefaults()
        {
            (this).QuickSet(int.MaxValue, CrystalDustType, CrystalSound, MapColor ?? Color.Transparent, -1, default, default, default);
            Main.tileBlockLight[Type] = false;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;

            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.HookPlaceOverride = new PlacementHook(PostPlace, -1, 0, true);
            TileObjectData.addTile(Type);

            if (!string.IsNullOrEmpty(ItemName))
                ItemType = Mod.Find<ModItem>(ItemName).Type;

            SafeSetDefaults();
        }

		public virtual void SafeSetDefaults() { }

        private int PostPlace(int x, int y, int type, int style, int dir, int extra)
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
            if (Main.tile[i, j].TileFrameX > 0)
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * MaxWidth, 16 * MaxHeight, ItemType);
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
            Projectile.hide = true;
        }

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
            behindNPCsAndTiles.Add(index);
		}

        public override void PostDraw(Color lightColor)
        {
            Tile t = Parent;
            
            if (t != null && t.TileFrameX > 0 && variantCount > 0)
            {
                Texture2D tex = TextureAssets.Tile[((int)t.BlockType)].Value;
                Rectangle frame = tex.Frame(variantCount, 1, t.TileFrameX - 1);
                Vector2 pos = ((Projectile.position - Main.screenPosition) + DrawOffset) - new Vector2(frame.Width * 0.5f, frame.Height);
                LightingBufferRenderer.DrawWithLighting(pos, tex, frame, DrawColor);
            }
        }
    }
}