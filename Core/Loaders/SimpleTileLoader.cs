using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Loaders
{
	public abstract class SimpleTileLoader : IOrderedLoadable, IPostLoadable
    {
        public Mod Mod => StarlightRiver.Instance;

        public virtual string AssetRoot => "StarlightRiver/Assets/Unknown/";

        public void LoadTile(string internalName, string displayName, TileLoadData data)
        {
            Mod.AddContent(new LoaderTileItem(internalName + "Item", displayName, "", internalName, 0, AssetRoot + internalName + "Item", true));
            Mod.AddContent(new LoaderTile(internalName, data, data.dropType == -1 ? Mod.Find<ModItem>(internalName + "Item").Type : data.dropType, AssetRoot + internalName));
        }

        public void LoadFurniture(string internalName, string displayName, FurnitureLoadData data)
        {
            Mod.AddContent(new LoaderTileItem(internalName + "Item", displayName, "", internalName, 0, AssetRoot + internalName + "Item", true));
            Mod.AddContent(new LoaderFurniture(internalName, data, Mod.Find<ModItem>(internalName + "Item").Type, AssetRoot + internalName));
        }

        public void AddMerge(int type1, int[] type2arr)
        {
            foreach(int type2 in type2arr)
            {
                Main.tileMerge[type1][type2] = true;
                Main.tileMerge[type2][type1] = true;
            }
        }

        public void AddMerge(string type1, string[] type2arr)
        {
            foreach (string type2 in type2arr)
            {
                Main.tileMerge[Mod.Find<ModTile>(type1).Type][Mod.Find<ModTile>(type2).Type] = true;
                Main.tileMerge[Mod.Find<ModTile>(type2).Type][Mod.Find<ModTile>(type1).Type] = true;
            }
        }

        public void AddMerge(string type1, int[] type2arr)
        {
            foreach (int type2 in type2arr)
            {
                Main.tileMerge[Mod.Find<ModTile>(type1).Type][type2] = true;
                Main.tileMerge[type2][Mod.Find<ModTile>(type1).Type] = true;
            }
        }

        public void AddMerge(int type1, int type2)
        {
            Main.tileMerge[type1][type2] = true;
            Main.tileMerge[type2][type1] = true;
        }

        public void AddMerge(string type1, string type2)
        {
            Main.tileMerge[Mod.Find<ModTile>(type1).Type][Mod.Find<ModTile>(type2).Type] = true;
            Main.tileMerge[Mod.Find<ModTile>(type2).Type][Mod.Find<ModTile>(type1).Type] = true;
        }

        public void AddSand(string type)
        {
            TileID.Sets.Conversion.Sand[Mod.Find<ModTile>(type).Type] = true; // Allows Clentaminator solutions to convert this tile to their respective Sand tiles.
            TileID.Sets.ForAdvancedCollision.ForSandshark[Mod.Find<ModTile>(type).Type] = true;
        }

        public void AddSandstone(string type)
        {
            TileID.Sets.Conversion.Sandstone[Mod.Find<ModTile>(type).Type] = true; // Allows Clentaminator solutions to convert this tile to their respective Sand tiles.
        }

        public void AddHardenedSand(string type)
        {
            TileID.Sets.Conversion.HardenedSand[Mod.Find<ModTile>(type).Type] = true; // Allows Clentaminator solutions to convert this tile to their respective Sand tiles.
        }

        public void AddMerge(string type1, int type2)
        {
            Main.tileMerge[Mod.Find<ModTile>(type1).Type][type2] = true;
            Main.tileMerge[type2][Mod.Find<ModTile>(type1).Type] = true;
        }

        public virtual float Priority { get => 2f; }

        public virtual void Load() { }

        public virtual void Unload() { }

        public virtual void PostLoad() { }

        public void PostLoadUnload() { }
    }

    [Autoload(false)]
    public class LoaderTileItem : QuickTileItem
	{
        public LoaderTileItem() { }
        protected override bool CloneNewInstances => true;
        public LoaderTileItem(string internalName, string name, string tooltip, string placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0) : 
            base(internalName, name, tooltip, placetype, rare, texturePath, pathHasName, ItemValue) { }
	}

    [Autoload(false)]
    public class LoaderTile : ModTile
    {
        public string InternalName;
        public string TileTexture;
        public TileLoadData Data;
        public readonly int DropID;

        public LoaderTile(string internalName, TileLoadData data, int dropID, string texture)
        {
            InternalName = internalName;
            Data = data;
            DropID = dropID;
            TileTexture = texture;
        }

		public override string Name => InternalName;

		public override string Texture => TileTexture;

		public override void SetStaticDefaults()
        {
            this.QuickSet
                (
                Data.minPick,
                Data.dustType,
                Data.hitSound,
                Data.mapColor,
                DropID,
                Data.dirtMerge,
                Data.stone,
                Data.mapName
                );
        }

		public override bool CanExplode(int i, int j)
		{
            return MinPick < 100;
		}
	}

    [Autoload(false)]
    public class LoaderFurniture : ModTile
    {
        public string InternalName;
        public string TileTexture;
        public FurnitureLoadData Data;
        public readonly int DropID;

        public LoaderFurniture(string internalName, FurnitureLoadData data, int drop, string texture)
        {
            InternalName = internalName;
            Data = data;
            DropID = drop;
            TileTexture = texture;
        }

        public override string Name => InternalName;

        public override string Texture => TileTexture;

		public override void SetStaticDefaults()
        {
            this.QuickSetFurniture
                (
                    Data.width,
                    Data.height,
                    Data.dustType,
                    Data.hitSound,
                    Data.tallBottom,
                    Data.mapColor,
                    Data.solidTop,
                    Data.solid,
                    Data.mapName,
                    Data.bottomAnchor,
                    Data.topAnchor,
                    Data.anchorTiles,
                    Data.faceDirection,
                    Data.wallAnchor,
                    Data.origin
                );
        }

        public override bool CanExplode(int i, int j)
        {
            return MinPick < 100;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, DropID);
        }
    }

    public struct TileLoadData
    {
        public int minPick;
        public int dustType;
        public SoundStyle hitSound;
        public Color mapColor;
        public bool dirtMerge;
        public bool stone;
        public string mapName;
        public int dropType;

        public TileLoadData(int minPick, int dustType, SoundStyle hitSound, Color mapColor, bool dirtMerge = false, bool stone = false, string mapName = "", int dropType = -1)
        {
            this.minPick = minPick;
            this.dustType = dustType;
            this.hitSound = hitSound;
            this.mapColor = mapColor;
            this.dirtMerge = dirtMerge;
            this.stone = stone;
            this.mapName = mapName;
            this.dropType = dropType;
        }
    }

    public struct FurnitureLoadData
    {
        public int width;
        public int height;
        public int dustType;
        public SoundStyle hitSound;
        public bool tallBottom;
        public Color mapColor;
        public bool solidTop;
        public bool solid;
        public string mapName;
        public AnchorData bottomAnchor;
        public AnchorData topAnchor;
        public int[] anchorTiles;
        public bool faceDirection;
        public bool wallAnchor;
        public Point16 origin;

        public FurnitureLoadData(int width, int height, int dustType, SoundStyle hitSound, bool tallBottom, Color mapColor, bool solidTop = false, bool solid = false, string mapName = "", AnchorData bottomAnchor = default, AnchorData topAnchor = default, int[] anchorTiles = null, bool faceDirection = false, bool wallAnchor = false, Point16 origin = default)
        {
            this.width = width;
            this.height = height;
            this.dustType = dustType;
            this.hitSound = hitSound;
            this.tallBottom = tallBottom;
            this.mapColor = mapColor;
            this.solidTop = solidTop;
            this.solid = solid;
            this.mapName = mapName;
            this.bottomAnchor = bottomAnchor;
            this.topAnchor = topAnchor;
            this.anchorTiles = anchorTiles;
            this.faceDirection = faceDirection;
            this.wallAnchor = wallAnchor;
            this.origin = origin;
        }
    }
}
