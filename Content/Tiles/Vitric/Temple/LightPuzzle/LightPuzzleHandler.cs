using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.CameraSystem;
using System.IO;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	class LightPuzzleHandler : ModSystem
	{
		public static bool solved;

		public static int solveTimer;

		public override void PreUpdateEntities()
		{
			if (StarlightRiver.debugMode && Main.LocalPlayer.controlHook)
				solveTimer = 0;

			if (solved && solveTimer == 1 && Main.LocalPlayer.InModBiome<VitricTempleBiome>())
			{
				CameraSystem.DoPanAnimation(240, StarlightWorld.VitricBossArena.BottomLeft() * 16 + new Vector2(220, 1180));
				ZoomHandler.SetZoomAnimation(2f, 60);
			}

			if (solved && solveTimer == 179)
			{
				ZoomHandler.SetZoomAnimation(1f, 60);
			}

			if (solved && solveTimer < 180)
				solveTimer++;
		}

		public override void ClearWorld()
		{
			solved = false;
			solveTimer = 0;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["solved"] = solved;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			solved = tag.GetBool("solved");

			if (solved)
				solveTimer = 180;
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(solved);
		}

		public override void NetReceive(BinaryReader reader)
		{
			solved = reader.ReadBoolean();

			if (solved)
				solveTimer = 180;
		}
	}
}