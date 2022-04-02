using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	class GearPuzzleHandler : ModWorld
	{
		public static int engagedObjectives;
		public static bool solved;

		private static Vector2 puzzleOriginLocation;

		public static Point16 PuzzleOriginLocation
		{
			get => puzzleOriginLocation.ToPoint16();
			set => puzzleOriginLocation = new Vector2(value.X, value.Y);
		}

		public static GearTileEntity PuzzleOriginEntity
		{
			get => TileEntity.ByPosition.ContainsKey(PuzzleOriginLocation) ? TileEntity.ByPosition[PuzzleOriginLocation] as GearTileEntity : null;
		}

		public override TagCompound Save()
		{
			return new TagCompound()
			{
				["puzzleOriginLocation"] = puzzleOriginLocation,
				["solved"] = solved
			};
		}

		public override void Load(TagCompound tag)
		{
			puzzleOriginLocation = tag.Get<Vector2>("puzzleOriginLocation");
			solved = tag.GetBool("solved");
		}
	}
}
