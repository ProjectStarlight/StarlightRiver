//TODO:
//Moonstone visuals
//Make it animate correctly
//Make it have a mouse over icon

using StarlightRiver.Content.Items.Moonstone;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Moonstone
{
	class MoonstoneMonolith : ModTile
	{
		public override string Texture => AssetDirectory.MoonstoneTile + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 3, 3, ModContent.DustType<MoonstoneArrowDust>(), SoundID.Tink, false, new Color(255, 255, 150), false, false, "Moonstone Monolith");
			AnimationFrameHeight = 54;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			if (frame > 0 && ++frameCounter >= 5)
			{
				frameCounter = 0;
				frame++;
				if (frame > 18)
					frame = 7;
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, 48, 48, ModContent.ItemType<MoonstoneMonolithItem>());
		}

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;

			while (tile.TileFrameX != 0)
			{
				left--;
				tile = Main.tile[left, j];
			}

			while (tile.TileFrameY % 54 != 0)
			{
				top--;
				tile = Main.tile[left, top];
			}

			if (tile.TileFrameY == 0)
			{
				for (int x = 0; x < 3; x++)
					for (int y = 0; y < 3; y++)
					{
						tile = Main.tile[left + x, top + y];
						tile.TileFrameY += 54;
					}
			}
			else
			{
				for (int x = 0; x < 3; x++)
					for (int y = 0; y < 3; y++)
					{
						tile = Main.tile[left + x, top + y];
						tile.TileFrameY = (short)(y * 18);
					}
			}

			return true;
		}
	}

	public class MoonstoneMonolithItem : QuickTileItem
	{
		public MoonstoneMonolithItem() : base("Moonstone monolith", "Dreamifies the skies", "MoonstoneMonolith", 2, AssetDirectory.MoonstoneTile, false, Item.sellPrice(0, 0, 50, 0)) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
