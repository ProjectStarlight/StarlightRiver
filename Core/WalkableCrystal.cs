using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Core
{
	internal abstract class WalkableCrystal : DummyTile
	{
		protected readonly string itemName;
		protected int itemType;
		protected readonly string structurePath;
		protected string fullStructurePath;
		public readonly int variantCount;
		protected readonly Color? mapColor;
		protected readonly int dustType;
		protected readonly SoundStyle? sound;
		public readonly int maxWidth;
		public readonly int maxHeight;
		protected readonly string texturePath;
		public readonly string dummyName;

		public override int DummyType => Mod.Find<ModProjectile>(dummyName).Type;

		public override string Texture => texturePath + Name;

		public override bool SpawnConditions(int i, int j)
		{
			return Main.tile[i, j].TileFrameX > 0;
		}

		protected WalkableCrystal(int maxWidth, int maxHeight, string dummyType, string path = null, string structurePath = null, int variantCount = 1, string drop = null, int dust = 0, Color? mapColor = null, SoundStyle? sound = null)
		{
			itemName = drop;
			texturePath = path;
			this.structurePath = structurePath;
			this.variantCount = variantCount;
			this.mapColor = mapColor;
			dustType = dust;
			this.maxHeight = maxHeight;
			this.maxWidth = maxWidth;
			this.sound = sound;
			dummyName = dummyType;
		}

		public override void Load()
		{
			string suffix = Name + (variantCount > 1 ? "_" : string.Empty);
			fullStructurePath = (string.IsNullOrEmpty(structurePath) ? AssetDirectory.StructureFolder : structurePath) + suffix;
		}

		public override void SetStaticDefaults()
		{
			this.QuickSet(int.MaxValue, dustType, sound, mapColor ?? Color.Transparent, -1, default, default, default);
			Main.tileBlockLight[Type] = false;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;

			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.HookPlaceOverride = new PlacementHook(PostPlace, -1, 0, true);
			TileObjectData.addTile(Type);

			if (!string.IsNullOrEmpty(itemName))
				itemType = Mod.Find<ModItem>(itemName).Type;

			SafeSetDefaults();
		}

		public virtual void SafeSetDefaults() { }

		private int PostPlace(int x, int y, int type, int style, int dir, int extra)
		{
			if (style < variantCount)
			{
				var offset = new Point16(maxWidth / 2 - 1, maxHeight - 1);

				if (variantCount > 1)//if statement because the ternary was acting weird
					StructureHelper.Generator.GenerateStructure(fullStructurePath + style, new Point16(x, y) - offset, StarlightRiver.Instance);
				else
					StructureHelper.Generator.GenerateStructure(fullStructurePath, new Point16(x, y) - offset, StarlightRiver.Instance);
			}

			return 0;
		}

		public override bool Drop(int i, int j)
		{
			if (Main.tile[i, j].TileFrameX > 0)
				Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * maxWidth, 16 * maxHeight, itemType);

			return false;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return false;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			fail = true;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override bool Slope(int i, int j)
		{
			return false;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			return false;
		}
	}

	internal abstract class WalkableCrystalDummy : Dummy //extend from this and override PostDraw to change stuff
	{
		public readonly int variantCount;
		public virtual Vector2 DrawOffset => new(8, 18);
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
				Texture2D tex = TextureAssets.Tile[(int)t.BlockType].Value;
				Rectangle frame = tex.Frame(variantCount, 1, t.TileFrameX - 1);
				Vector2 pos = Projectile.position - Main.screenPosition + DrawOffset - new Vector2(frame.Width * 0.5f, frame.Height);
				LightingBufferRenderer.DrawWithLighting(pos, tex, frame, DrawColor);
			}
		}
	}
}