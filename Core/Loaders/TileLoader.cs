using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Items;
using Terraria.DataStructures;

namespace StarlightRiver.Core.Loaders
{
    public abstract class TileLoader : ILoadable, IPostLoadable
    {
        public Mod mod => StarlightRiver.Instance;

        public virtual string AssetRoot => "StarlightRiver/Assets/Unknown/";

        public void LoadTile(string internalName, string displayName, TileLoadData data)
        {
            mod.AddItem(internalName + "Item", new QuickTileItem(displayName, "", internalName, 0, AssetRoot + internalName + "Item", true));
            mod.AddTile(internalName, new LoaderTile(data, data.dropType == -1 ? mod.ItemType(internalName + "Item") : data.dropType), AssetRoot + internalName);
        }

        public void LoadFurniture(string internalName, string displayName, FurnitureLoadData data)
        {
            mod.AddItem(internalName + "Item", new QuickTileItem(displayName, "", internalName, 0, AssetRoot + internalName + "Item", true));
            mod.AddTile(internalName, new LoaderFurniture(data, mod.ItemType(internalName + "Item")), AssetRoot + internalName);
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
                Main.tileMerge[mod.TileType(type1)][mod.TileType(type2)] = true;
                Main.tileMerge[mod.TileType(type2)][mod.TileType(type1)] = true;
            }
        }

        public void AddMerge(string type1, int[] type2arr)
        {
            foreach (int type2 in type2arr)
            {
                Main.tileMerge[mod.TileType(type1)][type2] = true;
                Main.tileMerge[type2][mod.TileType(type1)] = true;
            }
        }



        public void AddMerge(int type1, int type2)
        {
            Main.tileMerge[type1][type2] = true;
            Main.tileMerge[type2][type1] = true;
        }

        public void AddMerge(string type1, string type2)
        {
            Main.tileMerge[mod.TileType(type1)][mod.TileType(type2)] = true;
            Main.tileMerge[mod.TileType(type2)][mod.TileType(type1)] = true;
        }

        public void AddMerge(string type1, int type2)
        {
            Main.tileMerge[mod.TileType(type1)][type2] = true;
            Main.tileMerge[type2][mod.TileType(type1)] = true;
        }



        public float Priority { get => 1f; }

        public virtual void Load() { }

        public virtual void Unload() { }

        public virtual void PostLoad() { }

        public void PostLoadUnload() { }
    }

    public class LoaderTile : ModTile
    {
        TileLoadData data;
        readonly int dropID;

        public LoaderTile(TileLoadData data, int dropID)
        {
            this.data = data;
            this.dropID = dropID;
        }

        public override void SetDefaults()
        {
            this.QuickSet
                (
                data.minPick,
                data.dustType,
                data.soundType,
                data.mapColor,
                dropID,
                data.dirtMerge,
                data.stone,
                data.mapName
                );
        }
    }

    public class LoaderFurniture : ModTile
    {
        FurnitureLoadData data;
        readonly int dropID;

        public LoaderFurniture(FurnitureLoadData data, int drop)
        {
            this.data = data;
            this.dropID = drop;
        }

        public override void SetDefaults()
        {
            this.QuickSetFurniture
                (
                    data.width,
                    data.height,
                    data.dustType,
                    data.soundType,
                    data.tallBottom,
                    data.mapColor,
                    data.solidTop,
                    data.solid,
                    data.mapName,
                    data.bottomAnchor,
                    data.topAnchor,
                    data.anchorTiles
                );
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new Vector2(i, j) * 16, dropID);
        }
    }

    public struct TileLoadData
    {
        public int minPick;
        public int dustType;
        public int soundType;
        public Color mapColor;
        public bool dirtMerge;
        public bool stone;
        public string mapName;
        public int dropType;

        public TileLoadData(int minPick, int dustType, int soundType, Color mapColor, bool dirtMerge = false, bool stone = false, string mapName = "", int dropType = -1)
        {
            this.minPick = minPick;
            this.dustType = dustType;
            this.soundType = soundType;
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
        public int soundType;
        public bool tallBottom;
        public Color mapColor;
        public bool solidTop;
        public bool solid;
        public string mapName;
        public AnchorData bottomAnchor;
        public AnchorData topAnchor;
        public int[] anchorTiles;

        public FurnitureLoadData(int width, int height, int dustType, int soundType, bool tallBottom, Color mapColor, bool solidTop = false, bool solid = false, string mapName = "", AnchorData bottomAnchor = default, AnchorData topAnchor = default, int[] anchorTiles = null)
        {
            this.width = width;
            this.height = height;
            this.dustType = dustType;
            this.soundType = soundType;
            this.tallBottom = tallBottom;
            this.mapColor = mapColor;
            this.solidTop = solidTop;
            this.solid = solid;
            this.mapName = mapName;
            this.bottomAnchor = bottomAnchor;
            this.topAnchor = topAnchor;
            this.anchorTiles = anchorTiles;
        }
    }
}
