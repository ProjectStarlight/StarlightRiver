using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.Underground;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Underground
{
	internal class Glorch : ModTile
	{
		public static List<Point16> darkPoints = [];
		public static int savedX;
		public static int savedY;

		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/Glorch";

		public override void Load()
		{
			On_LightingEngine.ProcessBlur += Gloomify;
		}

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileWaterDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.Torch[Type] = true;

			DustType = DustID.Dirt;
			AdjTiles = new int[] { TileID.Torches };

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.StyleTorch);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
			TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
			TileObjectData.addAlternate(1);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, TileObjectData.newTile.Height, 0);
			TileObjectData.newAlternate.AnchorAlternateTiles = new[] { 124 };
			TileObjectData.addAlternate(2);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.StyleTorch);
			TileObjectData.newAlternate.AnchorWall = true;
			TileObjectData.addAlternate(0);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(20, 20, 20));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			darkPoints.Add(new Point16(i, j));
			Main.GetAreaToLight(out int x, out int _, out int y, out int _);

			savedX = x;
			savedY = y;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			if (Main.rand.NextBool(5))
				Dust.NewDustPerfect(new Vector2(i, j) * 16 + new Vector2(8, 4), DustID.t_Slime, Vector2.UnitY * 0.1f, 180, new Color(95, 85, 80), Main.rand.NextFloat(0.5f, 1.0f));
		}

		private void Gloomify(On_LightingEngine.orig_ProcessBlur orig, LightingEngine self)
		{
			orig(self);

			object engine = typeof(Lighting).GetField("NewEngine", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			var map = typeof(LightingEngine).GetField("_workingLightMap", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(engine) as LightMap;

			foreach (Point16 point in darkPoints)
			{
				float limit = point.Y < Main.worldSurface ? 0.6f : 0f;

				for (int x2 = -7; x2 < 7; x2++)
				{
					for (int y2 = -7; y2 < 7; y2++)
					{
						float len = new Vector2(x2, y2).Length();

						if (len < 7f)
						{
							int thisx = point.X - savedX + 28 + x2;
							int thisy = point.Y - savedY + 28 + y2;

							if (thisx > 0 && thisx < map.Width && thisy > 0 && thisy < map.Height)
							{
								Vector3 current = map[thisx, thisy];

								if (current.Length() > limit)
									map[thisx, thisy] -= current * (1f - len / 7f) * 0.5f * (current.Length() - limit);
							}
						}
					}
				}
			}

			darkPoints.Clear();
		}
	}

	public class GlorchItem : QuickTileItem
	{
		public GlorchItem() : base("Glorch", "Does a lamp give off light... or suck up the dark?\nReduces light nearby", "Glorch", ItemRarityID.Blue, "StarlightRiver/Assets/Tiles/Underground/") { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(3);
			recipe.AddIngredient(ItemID.Wood, 1);
			recipe.AddIngredient(ModContent.ItemType<GloomGel>(), 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}