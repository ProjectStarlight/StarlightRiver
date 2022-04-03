using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

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
            Main.tileMerge[Type][Mod.GetTile("Trellis").Type] = true;
            drop = Mod.ItemType("Soil");
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
            Main.tileMerge[Type][Mod.GetTile("Soil").Type] = true;
            drop = Mod.ItemType("Soil");

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Rich Soil");
            AddMapEntry(new Color(56, 33, 33), name);
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Herbology/Post").Value, new Vector2((i + 12) * 16, (j + 9) * 16) - Main.screenPosition, Lighting.GetColor(i, j));
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
            Item.NewItem(i * 16, j * 16, 16, 32, Mod.ItemType("Planter"));
        }

        public override void PlaceInWorld(int i, int j, Item Item)
        {
            Main.tile[i, j].frameX = 0;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j + 1].active() == false)
                switch (Main.tile[i, j].frameX / 18)
                {
                    case 0: break;
                    case 1: WorldGen.PlaceTile(i, j + 1, Mod.TileType("ForestIvy"), true); break;
                }
        }

        public override bool NewRightClick(int i, int j)
        {
            Player Player = Main.LocalPlayer;
            if (Player.HeldItem.type == Mod.ItemType("IvySeeds") && Main.tile[i, j].frameX == 0) //plants ivy
                Main.tile[i, j].frameX = 18;
            return true;
        }
    }

    public class GardenPot : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => this.QuickSetFurniture(6, 2, DustID.t_LivingWood, SoundID.Dig, false, new Color(151, 107, 75), true, false, "Garden Pot");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<GardenPotItem>());
    }

    public class GardenPotItem : QuickTileItem
    {
        public GardenPotItem() : base("Garden Pot", "Beeg planter", TileType<GardenPot>(), 0, AssetDirectory.HerbologyTile) { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.Wood, 20);
            recipe.AddIngredient(RecipeGroupID.IronBar, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}