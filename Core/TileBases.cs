using System;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Core
{
	public abstract class QuickFountain : ModTile
	{
		protected readonly string itemName;
		protected int itemType;
		protected readonly int fameCount;
		protected readonly Color? mapColor;
		protected readonly int dustType;
		protected readonly int width;
		protected readonly int height;
		protected readonly string texturePath;

		protected QuickFountain(string drop, string path = null, int animFrameCount = 6, int dust = 1, Color? mapColor = null, int width = 2, int height = 4)
		{
			itemName = drop;
			texturePath = path;
			fameCount = animFrameCount;
			this.mapColor = mapColor;
			dustType = dust;
			this.height = height;
			this.width = width;
		}

		public override string Texture => texturePath + Name;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = width;
			TileObjectData.newTile.Height = height;
			TileObjectData.newTile.Origin = new Point16(width / 2, height / 2 + 1);
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, height).ToArray();
			//TileObjectData.newTile.DrawYOffset = 2; has no effect for some reason
			TileObjectData.addTile(Type);

			DustType = dustType;
			AnimationFrameHeight = height * 18;

			itemType = Mod.Find<ModItem>(itemName).Type;
			AddMapEntry(mapColor ?? new Color(75, 139, 166));

			AdjTiles = new int[] { TileID.WaterFountain };
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, itemType);
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			if (++frameCounter * 0.2f >= fameCount)
				frameCounter = 0;

			frame = (int)(frameCounter * 0.2f);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];

			Vector2 zero = Main.drawToScreen ?
				Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);

			Texture2D texture = TextureAssets.Tile[Type].Value;

			int animate = tile.TileFrameY >= AnimationFrameHeight ?
				Main.tileFrame[Type] * AnimationFrameHeight : 0;

			Main.spriteBatch.Draw(texture, new Vector2(i * 16, j * 16) - Main.screenPosition + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY + animate, 16, 16), Lighting.GetColor(i, j), 0f, default, 1f, SpriteEffects.None, 0f);
			return false;
		}

		public override bool RightClick(int i, int j)
		{
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
			HitWire(i, j);
			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
			Player.cursorItemIconID = itemType;
		}

		public override void HitWire(int i, int j)
		{
			int x = i - Main.tile[i, j].TileFrameX / 18 % width;
			int y = j - Main.tile[i, j].TileFrameY / 18 % height;

			for (int l = x; l < x + width; l++)
			{
				for (int m = y; m < y + height; m++)
				{
					if (Main.tile[l, m].HasTile && Main.tile[l, m].TileType == Type)
					{
						if (Main.tile[l, m].TileFrameY < (short)AnimationFrameHeight)
							Main.tile[l, m].TileFrameY += (short)AnimationFrameHeight;
						else
							Main.tile[l, m].TileFrameY -= (short)AnimationFrameHeight;
					}
				}
			}

			if (Wiring.running)
			{
				for (int g = 0; g < width; g++)
				{
					for (int h = 0; h < height; h++)
					{
						Wiring.SkipWire(x + g, y + h);
					}
				}
			}

			NetMessage.SendTileSquare(-1, x, y + 1, 6);
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (Main.tile[i, j].TileFrameY >= AnimationFrameHeight)
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

	public abstract class ModVine : ModTile
	{
		protected readonly string[] anchorableTiles;
		protected int[] anchorTileTypes;
		protected readonly int dustType;
		protected readonly int maxLength;
		protected readonly int growChance;//lower is faster (one out of this amount)
		protected readonly Color? mapColor;
		protected readonly string itemName;
		protected readonly int dustAmount;
		protected readonly SoundStyle? sound;
		protected readonly string texturePath;

		public override string Texture => texturePath + Name;

		public ModVine(string[] anchorableTiles, int dustType, Color? mapColor = null, int growthChance = 10, int maxVineLength = 9, string drop = null, int dustAmount = 1, SoundStyle? soundType = null, string path = null)
		{
			this.anchorableTiles = anchorableTiles;
			this.dustType = dustType;
			this.mapColor = mapColor;
			growChance = growthChance;
			maxLength = maxVineLength;
			itemName = drop;
			this.dustAmount = dustAmount;
			sound = soundType ?? SoundID.Grass;
			texturePath = path;
		}

		public sealed override void SetStaticDefaults()
		{
			anchorTileTypes = new int[anchorableTiles.Length + 1];

			for (int i = 0; i < anchorableTiles.Length; i++)
			{
				anchorTileTypes[i] = Mod.Find<ModTile>(anchorableTiles[i]).Type;
			}

			anchorTileTypes[anchorableTiles.Length] = Type;

			Main.tileSolid[Type] = false;
			Main.tileCut[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileBlockLight[Type] = false;

			//this TileObjectData stuff is *only* needed for placing with an Item
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.AnchorAlternateTiles = anchorTileTypes;
			TileObjectData.addTile(Type);

			if (mapColor != null)
				AddMapEntry(mapColor ?? Color.Transparent);

			if (itemName != null)
				ItemDrop = Mod.Find<ModItem>(itemName).Type;

			DustType = dustType;
			HitSound = sound;

			SafeSetDefaults();
		}

		public virtual void SafeSetDefaults() { }

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = dustAmount;
		}

		public sealed override void RandomUpdate(int i, int j)
		{
			Grow(i, j, growChance);
			SafeRandomUpdate(i, j);
		}
		protected void Grow(int i, int j, int chance)
		{
			if (!Main.tile[i, j + 1].HasTile && Main.tile[i, j - maxLength].TileType != Type && Main.rand.Next(chance) == 0)
				WorldGen.PlaceTile(i, j + 1, Type, true);
		}

		public virtual void SafeRandomUpdate(int i, int j) { }

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			if (!Main.tile[i, j - 1].HasTile && !anchorTileTypes.Contains(Main.tile[i, j - 1].TileType))
				WorldGen.KillTile(i, j);
			//WorldGen.SquareTileFrame(i, j, true);
			return true;
		}
	}
}