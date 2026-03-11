using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Tiles.Permafrost;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.NoBuildingSystem;

public class NoBuildSystem : ModSystem
{
	/// <summary>
	/// Regions which players shold not be able to modify in the world
	/// </summary>
	public static List<Rectangle> protectedRegions = [];

	/// <summary>
	/// Specific points in disallowed regions that the player should be given the ability to modify.
	/// This should usually only be used for very small exceptions.
	/// </summary>
	public static List<Point16> pointExceptions = [];

	/// <summary>
	/// Collection of IDs for wall types that should not be buildable.
	/// </summary>
	public static bool[] noBuildWalls = WallID.Sets.Factory.CreateBoolSet();

	private static readonly Dictionary<Point16, Ref<Rectangle>> RuntimeRegionsByPoint = [];
	public static readonly List<Ref<Rectangle>> RuntimeProtectedRegions = [];

	public override void PostDrawTiles()
	{
		if (!StarlightRiver.debugMode)
			return;

		Main.spriteBatch.Begin(default, default, SamplerState.PointWrap, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

		foreach (Rectangle rect in protectedRegions)
		{
			Texture2D tex = Assets.MagicPixel.Value;
			var target = new Rectangle(rect.X * 16 - (int)Main.screenPosition.X, rect.Y * 16 - (int)Main.screenPosition.Y, rect.Width * 16, rect.Height * 16);
			Main.spriteBatch.Draw(tex, target, Color.Red * 0.25f);
		}

		foreach (Ref<Rectangle> rectRef in RuntimeProtectedRegions)
		{
			Rectangle rect = rectRef.Value;
			Texture2D tex = Assets.MagicPixel.Value;
			var target = new Rectangle(rect.X * 16 - (int)Main.screenPosition.X, rect.Y * 16 - (int)Main.screenPosition.Y, rect.Width * 16, rect.Height * 16);
			Main.spriteBatch.Draw(tex, target, Color.Blue * 0.25f);
		}

		Main.spriteBatch.End();
	}

	/// <summary>
	/// If a tile should have build protection applied or not at the given point
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public static bool IsTileProtected(int x, int y)
	{
		return IsTileProtected(new Point16(x, y));
	}

	/// <summary>
	/// If a tile should have build protection applied or not at the given point
	/// </summary>
	/// <param name="point"></param>
	/// <returns></returns>
	public static bool IsTileProtected(Point16 point)
	{
		if (Main.gameMenu || Main.dedServ) //shouldnt trigger while generating the world from the menu
			return false;

		if (StarlightRiver.debugMode)
			return false;

		if (pointExceptions.Contains(point))
			return false;

		if (BossRushSystem.BossRushSystem.isBossRush)
			return true;

		Tile tile = Framing.GetTileSafely(point);

		if (tile.TileType == TileID.Tombstones)
			return false;

		foreach (Rectangle region in protectedRegions)
		{
			if (region.Contains(point.ToPoint()))
				return true;
		}

		foreach (Ref<Rectangle> region in RuntimeProtectedRegions)
		{
			if (region.Value.Contains(point.ToPoint()))
				return true;
		}

		if (noBuildWalls[tile.WallType])
			return true;

		return false;
	}

	public override void PreWorldGen()
	{
		protectedRegions.Clear();
		RuntimeProtectedRegions.Clear();
		RuntimeRegionsByPoint.Clear();
	}

	public override void PreUpdateEntities()
	{
		pointExceptions.Clear();
	}

	public override void LoadWorldData(TagCompound tag)
	{
		protectedRegions.Clear();
		RuntimeProtectedRegions.Clear();
		RuntimeRegionsByPoint.Clear();

		int length = tag.GetInt("RegionCount");

		for (int k = 0; k < length; k++)
		{
			protectedRegions.Add(new Rectangle
				(
				tag.GetInt("x" + k),
				tag.GetInt("y" + k),
				tag.GetInt("w" + k),
				tag.GetInt("h" + k)
				));
		}
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag["RegionCount"] = protectedRegions.Count;

		for (int k = 0; k < protectedRegions.Count; k++)
		{
			Rectangle region = protectedRegions[k];
			tag.Add("x" + k, region.X);
			tag.Add("y" + k, region.Y);
			tag.Add("w" + k, region.Width);
			tag.Add("h" + k, region.Height);
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(protectedRegions.Count);

		for (int i = 0; i < protectedRegions.Count; i++)
		{
			Rectangle region = protectedRegions[i];
			writer.Write(region.X);
			writer.Write(region.Y);
			writer.Write(region.Width);
			writer.Write(region.Height);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		protectedRegions.Clear();

		int numRegions = reader.ReadInt32();

		for (int i = 0; i < numRegions; i++)
		{
			protectedRegions.Add(new Rectangle
			{
				X = reader.ReadInt32(),
				Y = reader.ReadInt32(),
				Width = reader.ReadInt32(),
				Height = reader.ReadInt32()
			});
		}
	}

	public static void AddRegionBySource(Point16 source, Rectangle region)
	{
		if (!RuntimeRegionsByPoint.ContainsKey(source))
		{
			var refRect = new Ref<Rectangle>(region);
			RuntimeRegionsByPoint.Add(source, refRect);
			RuntimeProtectedRegions.Add(refRect);
		}
	}

	public static void RemoveRegionBySource(Point16 source)
	{
		if (RuntimeRegionsByPoint.TryGetValue(source, out Ref<Rectangle> refRect))
		{
			RuntimeProtectedRegions.Remove(refRect);
			RuntimeRegionsByPoint.Remove(source);
		}
	}
}