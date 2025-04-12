using StarlightRiver.Core.DrawingRigs;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.FoliageLayerSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using System.IO;
using System.Text.Json;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace StarlightRiver.Content.Tiles.Forest
{
	internal class RiggedTree : DummyTile
	{
		public override string Texture => AssetDirectory.ForestTile + "TrunkBody";

		public override int DummyType => DummySystem.DummyType<RiggedTreeDummy>();

		public override void SetStaticDefaults()
		{
			LocalizedText name = CreateMapEntryName();
			name.SetDefault("Large Tree");

			TileID.Sets.IsATreeTrunk[Type] = true;
			Main.tileAxe[Type] = true;
			AddMapEntry(new Color(169, 125, 93), name);

			RegisterItemDrop(ItemID.Wood);
		}

		public override bool SpawnConditions(int i, int j)
		{
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<RiggedTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<RiggedTree>();

			return right && !up && down;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<RiggedTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<RiggedTree>();

			if (right && !up && down)
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(new Point(i, j));
		}

		private float Sway(Vector2 worldPos, float magnitude)
		{
			float windDir = Main.windSpeedCurrent > 0 ? 1 : -1;
			return (float)Math.Sin(Main.GameUpdateCount * 0.07f + worldPos.X * 0.01f * -windDir) * magnitude * Main.windSpeedCurrent;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<RiggedTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<RiggedTree>();

			if (right && !up && down)
			{
				Texture2D tex = Assets.Tiles.Forest.Branches.Value;

				Vector2 pos = new Vector2(i + 1, j) * 16;
				Vector2 origin = pos;

				float branchRot = Main.windSpeedCurrent * 0.05f + Sway(pos, 0.02f);

				pos += Vector2.One * Main.offScreenRange;

				Texture2D tex2 = Assets.Tiles.Forest.Godray.Value;
				Color godrayColor = Lighting.GetColor(i, j - 20) * 0.5f;
				float godrayRot;

				if (Main.dayTime)
				{
					godrayColor *= (float)Math.Pow(Math.Sin(Main.time / 54000f * 3.14f), 3);
					godrayRot = -0.5f * 1.57f + (float)Main.time / 54000f * 3.14f;
				}
				else
				{
					godrayColor *= (float)Math.Pow(Math.Sin(Main.time / 24000f * 3.14f), 3);
					godrayRot = -0.5f * 1.57f + (float)Main.time / 24000f * 3.14f;
				}

				godrayColor.A = 0;

				pos += new Vector2(-80, -400);

				int daySeed = i + (int)Main.GetMoonPhase();

				if (daySeed % 3 == 0)
					spriteBatch.Draw(tex2, pos.RotatedBy(branchRot, origin) - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, new Vector2(1.05f, 1.2f), 0, 0);

				pos += new Vector2(-60, 80);

				if (daySeed % 5 == 0)
					spriteBatch.Draw(tex2, pos.RotatedBy(branchRot, origin) - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, new Vector2(0.75f, 1.2f), 0, 0);

				pos += new Vector2(150, -60);

				if (daySeed % 7 == 0)
					spriteBatch.Draw(tex2, pos.RotatedBy(branchRot, origin) - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, new Vector2(1.35f, 1.4f), 0, 0);
			}
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<RiggedTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<RiggedTree>();

			if (Main.rand.NextBool(10) && right && !up && down)
			{
				Color godrayColor = Lighting.GetColor(i, j - 20);
				if (Main.dayTime && !Main.raining && Main.time > 10000 && Main.time < 44000 && godrayColor.ToVector3().Length() > 1f)
				{
					float godrayRot = (float)Main.time / 54000f * 3.14f;
					Dust.NewDustPerfect(new Vector2(i, j) * 16 - Vector2.UnitY * 400 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(200), ModContent.DustType<Dusts.GoldSlowFade>(), Vector2.UnitX.RotatedBy(godrayRot) * Main.rand.NextFloat(0.25f, 0.5f), 255, default, 0.75f);
				}
			}
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly)
				return;

			Framing.GetTileSafely(i, j).HasTile = false;

			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<RiggedTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<RiggedTree>() ||
				Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<RiggedTreeBase>();

			if (left)
				WorldGen.KillTile(i - 1, j);
			if (right)
				WorldGen.KillTile(i + 1, j);
			if (up)
				WorldGen.KillTile(i, j - 1);
			if (down)
				WorldGen.KillTile(i, j + 1);
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			short x = 0;
			short y = 0;

			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<RiggedTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<RiggedTree>();

			bool farUp = Framing.GetTileSafely(i, j - 4).TileType == ModContent.TileType<RiggedTree>();

			if (up || down)
			{
				if (right)
					x = 0;

				if (left)
					x = 18;

				y = (short)(j % 2 * 18);

				if (!farUp)
					y += 36;

				x += (short)(36 * Main.rand.Next(4));
			}

			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX = x;
			tile.TileFrameY = y;

			return false;
		}
	}

	class RiggedTreeDummy : Dummy
	{
		public static StaticRig treeRig;

		public RiggedTreeDummy() : base(ModContent.TileType<RiggedTree>(), 1, 1) { }

		public override void OnLoad(Mod mod)
		{
			Stream stream = StarlightRiver.Instance.GetFileStream("Assets/Tiles/Forest/TreeRig.json");
			treeRig = JsonSerializer.Deserialize<StaticRig>(stream);
			stream.Close();
		}

		public override void OnSpawn()
		{
			offscreenRadius = 600;
		}

		public override bool ValidTile(Tile tile)
		{
			int i = ParentX;
			int j = ParentY;

			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<RiggedTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<RiggedTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<RiggedTree>();

			return right && !up && down;
		}

		private float Sway(Vector2 worldPos, float magnitude)
		{
			float windDir = Main.windSpeedCurrent > 0 ? 1 : -1;
			return (float)Math.Sin(Main.GameUpdateCount * 0.07f + worldPos.X * 0.01f * -windDir) * magnitude * Main.windSpeedCurrent;
		}

		public override void DrawBehindTiles()
		{
			Texture2D branches = Assets.Tiles.Forest.Branches.Value;
			var branchOrigin = new Vector2(branches.Width / 2 - 16, branches.Height);
			float windDir = Main.windSpeedCurrent > 0 ? 1 : -1;
			float branchRot = Main.windSpeedCurrent * 0.05f + Sway(Center, 0.02f);

			LightingBufferRenderer.DrawWithLighting(branches, Center - Main.screenPosition + Vector2.UnitY * 2, null, Color.White, branchRot, branchOrigin, 1);

			if (StarlightRiver.debugMode)
			{
				Rectangle box = Hitbox;
				box.Inflate(18, 8);
				box.Offset((-Main.screenPosition).ToPoint());
				Main.spriteBatch.Draw(Assets.MagicPixel.Value, box, Color.Yellow);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D leaves = Assets.Tiles.Forest.Leaves.Value;

			Vector2 pos = Center - new Vector2(280, 630);
			float windDir = Main.windSpeedCurrent > 0 ? 1 : -1;

			float branchRot = Main.windSpeedCurrent * 0.05f + Sway(Center, 0.02f);
			var leafRand = new UnifiedRandom((int)Center.X ^ (int)Center.Y);

			foreach (StaticRigPoint point in treeRig.Points)
			{
				Vector2 pointPos = pos + new Vector2(point.X, point.Y) * 2;
				var source = new Rectangle(leafRand.NextBool() ? 82 : 0, point.Frame * 82, 82, 82);
				Vector2 origin = Vector2.One * 41;
				float weight = point.Frame % 2 == 0 ? 6 : 4;

				pointPos = pointPos.RotatedBy(branchRot, Center);
				float rot = Sway(pointPos, 0.1f);
				pointPos.X += Sway(pointPos, weight);

				if (point.Frame < 2)
					FoliageLayerSystem.overTilesData.Add(new(leaves, pointPos, source, Lighting.GetColor((pointPos / 16).ToPoint()), rot, origin, 1, 0, 0));
				else
					FoliageLayerSystem.underTilesData.Add(new(leaves, pointPos, source, Lighting.GetColor((pointPos / 16).ToPoint()), rot, origin, 1, 0, 0));
			}

			if (StarlightRiver.debugMode)
			{
				Rectangle box = Hitbox;
				box.Inflate(8, 8);
				box.Offset((-Main.screenPosition).ToPoint());
				Main.spriteBatch.Draw(Assets.MagicPixel.Value, box, Color.Red);
			}
		}
	}

	class RiggedTreeBase : ModTile
	{
		public override string Texture => AssetDirectory.ForestTile + "ThickTreeBase";

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 4, 0);
			Main.tileAxe[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

			this.QuickSetFurniture(4, 4, 0, SoundID.Dig, true, new Color(169, 125, 93));//a
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly)
				return;

			Framing.GetTileSafely(i, j).HasTile = false;

			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<RiggedTree>();

			if (up)
				WorldGen.KillTile(i, j - 1);
		}
	}
}