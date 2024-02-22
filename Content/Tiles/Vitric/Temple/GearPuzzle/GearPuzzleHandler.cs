using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	class GearPuzzleHandler : ModSystem
	{
		public static int engagedObjectives;
		public static int solveTimer;

		public static bool Solved => engagedObjectives >= 15;

		private static Vector2 puzzleOriginLocation;

		public static Point16 PuzzleOriginLocation
		{
			get => puzzleOriginLocation.ToPoint16();
			set => puzzleOriginLocation = new Vector2(value.X, value.Y);
		}

		public static GearTileEntity PuzzleOriginEntity => TileEntity.ByPosition.ContainsKey(PuzzleOriginLocation) ? TileEntity.ByPosition[PuzzleOriginLocation] as GearTileEntity : null;

		public override void PreUpdateEntities()
		{
			if (StarlightRiver.debugMode && Main.LocalPlayer.controlHook)
				solveTimer = 0;

			if (Solved && solveTimer == 1 && Main.LocalPlayer.InModBiome<VitricTempleBiome>())
			{
				CameraSystem.DoPanAnimation(240, StarlightWorld.VitricBossArena.BottomLeft() * 16 + new Vector2(220, 980));
				ZoomHandler.SetZoomAnimation(2f, 60);
			}

			if (Solved && solveTimer == 179) //don't check biome here incase they tp out or die and need a reset at the end
				ZoomHandler.SetZoomAnimation(1f, 60);

			if (Solved && solveTimer < 180)
				solveTimer++;
		}

		public override void ClearWorld()
		{
			engagedObjectives = 0;
			solveTimer = 0;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["puzzleOriginLocation"] = puzzleOriginLocation;
			tag["solved"] = Solved;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			puzzleOriginLocation = tag.Get<Vector2>("puzzleOriginLocation");

			bool isSolved = tag.GetBool("solved");
			LoadSolved(isSolved);
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.WriteVector2(puzzleOriginLocation);
			writer.Write(Solved);
		}

		public override void NetReceive(BinaryReader reader)
		{
			puzzleOriginLocation = reader.ReadVector2(); //doesn't use packedVector2 since it needs the extra precision for some reason

			bool isSolved = reader.ReadBoolean();
			LoadSolved(isSolved);
		}

		private void LoadSolved(bool isSolved)
		{
			if (isSolved)
			{
				engagedObjectives = 15;
				solveTimer = 180;
			}
		}
	}
}