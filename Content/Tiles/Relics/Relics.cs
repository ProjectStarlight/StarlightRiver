using ReLogic.Content;
using StarlightRiver.Core.Loaders.TileLoading;
using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Relics
{
	public class RelicLoader : SimpleTileLoader
	{
		public override string AssetRoot => "StarlightRiver/Assets/Tiles/Relics/";

		public override void Load()
		{
			LoadRelic("Auroracle", "Auroracle Relic");
			LoadRelic("Ceiros", "Ceiros Relic");
		}

		private void LoadRelic(string name, string displayName)
		{
			Mod.AddContent(new RelicItem(name + "RelicItem", displayName, "", name + "Relic", ItemRarityID.Master, AssetRoot + name, true));
			Mod.AddContent(new BaseRelic(name));
		}
	}

	public class RelicItem : LoaderTileItem
	{
		public RelicItem(string internalName, string name, string tooltip, string placetype, int rare = 0, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
			: base(internalName, name, tooltip, placetype, rare, texturePath, pathHasName, ItemValue)
		{

		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.master = true;
		}
	}

	[Autoload(false)]
	public class BaseRelic : ModTile
	{
		public const int FrameWidth = 18 * 3;
		public const int FrameHeight = 18 * 4;

		public string bossName;

		public Asset<Texture2D> RelicTexture;

		// Every relic has its own extra floating part, should be 50x50. Optional: Expand this sheet if you want to add more, stacked vertically
		// If you do not use the Item.placeStyle approach, and you extend from this class, you can override this to point to a different texture
		public string RelicTextureName => $"StarlightRiver/Assets/Tiles/Relics/{bossName}";

		// All relics use the same pedestal texture, this one is copied from vanilla
		public override string Texture => "StarlightRiver/Assets/Tiles/Relics/Base";

		public override string Name => $"{bossName}Relic";

		public BaseRelic() { }

		public BaseRelic(string bossName)
		{
			this.bossName = bossName;
		}

		public override void Load()
		{
			if (!Main.dedServ)
				RelicTexture = ModContent.Request<Texture2D>(RelicTextureName);
		}

		public override void Unload()
		{
			RelicTexture = null;
		}

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true; // Any multitile requires this
			Main.tileLighted[Type] = true;
			TileID.Sets.InteractibleByNPCs[Type] = true; // Town NPCs will palm their hand at this tile

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4); // Relics are 3x4
			TileObjectData.newTile.LavaDeath = false; // Does not break when lava touches it
			TileObjectData.newTile.DrawYOffset = 2; // So the tile sinks into the ground
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft; // Player faces to the left
			TileObjectData.newTile.StyleHorizontal = false; // Based on how the alternate sprites are positioned on the sprite (by default, true)

			// This controls how styles are laid out in the texture file. This tile is special in that all styles will use the same texture section to draw the pedestal.
			TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.styleLineSkipVisualOverride = 0; // This forces the tile preview to draw as if drawing the 1st style.

			// Register an alternate tile data with flipped direction
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile); // Copy everything from above, saves us some code
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight; // Player faces to the right
			TileObjectData.addAlternate(1);

			// Register the tile data itself
			TileObjectData.addTile(Type);

			// Register map name and color
			// "MapObject.Relic" refers to the translation key for the vanilla "Relic" text
			AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.Relic"));
		}

		public override bool CreateDust(int i, int j, ref int type)
		{
			return false;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Framing.GetTileSafely(i, j + 1);

			if (tile.TileType != Type)
			{
				if (Main.rand.NextBool(30))
				{
					var d = Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + 1) * 16, ModContent.DustType<Dusts.PulsingSparkle>(), Vector2.UnitY * Main.rand.Next(-4, -2), 0, new Color(20, Main.rand.Next(100, 200), 255));
					d.customData = Main.rand.NextFloat(0.7f, 1.4f);
				}

				if (Main.rand.NextBool(6))
					Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + 1) * 16, ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * Main.rand.Next(-2, 0), 0, new Color(50, Main.rand.Next(100, 200), 255) * 0.5f, Main.rand.NextFloat(0.5f, 0.8f));
			}
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			(r, g, b) = (0.5f, 0.6f, 0.7f);
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			tileFrameX %= FrameWidth;
			tileFrameY %= FrameHeight * 2;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0)
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var offScreen = new Vector2(Main.offScreenRange);

			if (Main.drawToScreen)
				offScreen = Vector2.Zero;

			var p = new Point(i, j);
			Tile tile = Main.tile[p.X, p.Y];

			if (tile == null || !tile.HasTile)
				return;

			// Get the initial draw parameters
			Texture2D texture = RelicTexture.Value;

			int frameY = tile.TileFrameX / FrameWidth; // Picks the frame on the sheet based on the placeStyle of the item
			Rectangle frame = texture.Frame(1, 1, 0, frameY);

			Vector2 origin = frame.Size() / 2f;
			Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);

			Color color = Lighting.GetColor(p.X, p.Y);

			bool direction = tile.TileFrameY / FrameHeight != 0; // This is related to the alternate tile data we registered before
			SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			// Some math magic to make it smoothly move up and down over time
			const float TwoPi = (float)Math.PI * 2f;
			float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);
			Vector2 drawPos = worldPos + offScreen - Main.screenPosition + new Vector2(0f, -40f) + new Vector2(0f, offset * 4f);

			// background glow
			Texture2D tex = Assets.StarTexture.Value;
			float sin = (float)Math.Sin(Main.GameUpdateCount * 0.05f);
			float sin2 = (float)Math.Sin(Main.GameUpdateCount * 0.05f + 2f);

			spriteBatch.Draw(tex, drawPos, null, new Color(190, 255, 255, 0), 0, tex.Size() / 2f, 0.25f + sin * 0.1f, 0, 0);
			spriteBatch.Draw(tex, drawPos, null, new Color(190, 255, 255, 0), 1.57f / 2f, tex.Size() / 2f, 0.15f + sin2 * 0.1f, 0, 0);

			spriteBatch.Draw(tex, drawPos, null, new Color(0, 230, 255, 0), 0, tex.Size() / 2f, 0.45f + sin * 0.1f, 0, 0);
			spriteBatch.Draw(tex, drawPos, null, new Color(0, 160, 255, 0), 1.57f / 2f, tex.Size() / 2f, 0.35f + sin2 * 0.1f, 0, 0);

			spriteBatch.Draw(tex, drawPos, null, new Color(0, 10, 60, 0), 0, tex.Size() / 2f, 0.55f + sin * 0.1f, 0, 0);
			spriteBatch.Draw(tex, drawPos, null, new Color(0, 0, 60, 0), 1.57f / 2f, tex.Size() / 2f, 0.45f + sin2 * 0.1f, 0, 0);

			// Draw the main texture
			spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

			// Draw the periodic glow effect
			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 2f) * 0.3f + 0.7f;
			Color effectColor = color;
			effectColor.A = 0;
			effectColor = effectColor * 0.1f * scale;

			for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI))
			{
				spriteBatch.Draw(texture, drawPos + (TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame, effectColor, 0f, origin, 1f, effects, 0f);
			}
		}
	}
}