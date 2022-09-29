﻿using Microsoft.Xna.Framework;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Utility;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Items.Herbology.Materials;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Crafting
{
    internal class CookStation : ModTile
    {
        public override string Texture => AssetDirectory.CraftingTile + Name;

        public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;

        public override void SetStaticDefaults()
        {
            this.QuickSetFurniture(6, 4, DustID.t_LivingWood, SoundID.Dig, true, new Color(151, 107, 75), false, false, "Cooking Station");
            Main.tileLighted[Type] = true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => 
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<CookStationItem>());

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			var tile = Framing.GetTileSafely(i, j);

            if (tile.TileFrameX == 18 && tile.TileFrameY == 18 * 2)
            {
                var sin = 1.1f + (float)System.Math.Sin(Main.GameUpdateCount * 0.04f) * (float)System.Math.Cos(Main.GameUpdateCount * 0.065f) * 0.15f;
                (r, g, b) = (1f * sin, 0.65f * sin, 0.4f * sin);
            }
		}

		public override bool RightClick(int i, int j)
        {
            Main.playerInventory = true;

            if (!CookingUI.visible)
            {
                CookingUI.visible = true;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuOpen);

                var bag = Main.LocalPlayer.inventory.FirstOrDefault(n => n.type == ItemType<ChefBag>())?.ModItem as ChefBag;

                if (bag != null)
                {
                    ChefBagUI.visible = true;
                    ChefBagUI.openBag = bag;
                    ChefBagUI.Move(CookingUI.Basepos + new Vector2(-500, 0));                 
                }
            }
            else
            {
                CookingUI.visible = false;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuClose);
            }

            return true;
        }
    }

    public class CookStationItem : QuickTileItem
    {
        public CookStationItem() : base("Prep Station", "Right click to prepare meals", "CookStation", 0, AssetDirectory.CraftingTile) { }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.Wood, 20)
            .AddIngredient(RecipeGroupID.IronBar, 5)
            .AddTile(TileID.WorkBenches)
            .Register();
        }
    }
}