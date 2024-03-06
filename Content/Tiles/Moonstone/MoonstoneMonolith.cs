//TODO:
//Moonstone visuals
//Make it animate correctly
//Make it have a mouse over icon

using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Items.Moonstone;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
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
			TileObjectData.newTile.DrawYOffset = 2;//just 
			QuickBlock.QuickSetFurniture(this, 2, 3, ModContent.DustType<MoonstoneArrowDust>(), SoundID.Tink, false, new Color(106, 113, 124), false, false, "Moonstone Monolith");
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			AnimationFrameHeight = 54;
			AdjTiles = new int[] { TileID.LunarMonolith };
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			const int startframeOffset = 6;
            if ((frameCounter = ++frameCounter % 8) == 0)
                frame = (++frame - startframeOffset) % 12 + startframeOffset;
		}

		public override bool RightClick(int i, int j)
		{
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i, j) * 16);
			HitWire(i, j);
			return true;
		}

		public override void HitWire(int i, int j)
		{
			Tile interactTile = Main.tile[i, j];

			int offsetX = interactTile.TileFrameX / 18;
			int offsetY = interactTile.TileFrameY / 18 % 3;
			Tile targetTile = Main.tile[i - offsetX, j - offsetY];

			bool inactive = targetTile.TileFrameY == 0;
			for (int x = 0; x < 2; x++)
				for (int y = 0; y < 3; y++)
				{
					int coordX = i - offsetX + x;
					int coordY = j - offsetY + y;

					if (inactive)
						Main.tile[coordX, coordY].TileFrameY += 54;
					else
						Main.tile[coordX, coordY].TileFrameY = (short)(y * 18);

					if (Wiring.running)
						Wiring.SkipWire(coordX, coordY);
				}

			NetMessage.SendTileSquare(-1, i - offsetX, j - offsetY + 1, 3);
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (Main.tile[i, j].TileFrameY >= 54)
			{
				ModContent.GetInstance<MoonstoneBiomeSystem>().monolithActive = true;
			}
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.noThrow = 2;
			Player.cursorItemIconID = ModContent.ItemType<MoonstoneMonolithItem>();
			Player.cursorItemIconEnabled = true;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			Texture2D texture = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneTile + "MoonstoneMonolith2").Value;
			Texture2D glowTexture = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneTile + "MoonstoneMonolith_Glow").Value;
			//Texture2D highlightTexture = ModContent.Request<Texture2D>(HighlightTexture).Value;

			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

			const int height = 16;
			// if the bottom tile is a pixel taller
			//int height = tile.TileFrameY % AnimationFrameHeight == 36 ? 18 : 16;

			int frameYOffset = tile.TileFrameY >= 54 ? Main.tileFrame[Type] * AnimationFrameHeight : 0;

			Vector2 pos = new Vector2(
					i * 16 - (int)Main.screenPosition.X,
					j * 16 - (int)Main.screenPosition.Y + 2) + zero;

			Rectangle frame = new Rectangle(
					tile.TileFrameX,
					tile.TileFrameY + frameYOffset,
					16,
					height);

			spriteBatch.Draw(
				texture, pos, frame, 
				Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			spriteBatch.Draw(
				glowTexture, pos, frame,
				Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			//spriteBatch.Draw(
			//	highlightTexture, pos, frame,
			//	Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

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
