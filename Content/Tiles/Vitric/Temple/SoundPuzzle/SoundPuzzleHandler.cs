using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.CameraSystem;
using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.SoundPuzzle
{
	class SoundPuzzleHandler : ModSystem
	{
		public static bool solved;

		public static int solveTimer;

		public static List<int> lastTries = new();
		public static int[] correct = new int[] { 0, 3, 1, 2 };

		public override void PreUpdateEntities()
		{
			correct = new int[] { 0, 3, 1, 2 };

			if (!solved)
			{
				for (int k = 0; k < lastTries.Count; k++)
				{
					if (lastTries[k] != correct[k])
					{
						lastTries.Clear();
						break;
					}

					if (k == 3)
					{
						lastTries.Clear();
						solved = true;
					}
				}
			}

			for (int k = 0; k < 4; k++)
			{
				if (Main.rand.NextBool(2))
				{
					Vector2 pos = StarlightWorld.VitricBossArena.BottomLeft() * 16 + new Vector2(-446 + k * 12 * 16, 1524);
					Color color = k < lastTries.Count || solved ? Color.Orange : new Color(40, 80, 90);

					color.A = 0;

					Dust.NewDustPerfect(pos + Vector2.UnitX * Main.rand.NextFloat(-4, 0), ModContent.DustType<Dusts.PixelatedEmber>(), Vector2.UnitY * -Main.rand.NextFloat(), 0, color, Main.rand.NextFloat(0.4f));
				}
			}

			if (StarlightRiver.debugMode && Main.LocalPlayer.controlHook)
				solveTimer = 0;

			if (solved && solveTimer == 1 && Main.LocalPlayer.InModBiome<VitricTempleBiome>())
			{
				CameraSystem.DoPanAnimation(240, StarlightWorld.VitricBossArena.BottomLeft() * 16 + new Vector2(2320, 1524));
				ZoomHandler.SetZoomAnimation(2f, 60);
			}

			if (solved && solveTimer == 179)
			{
				ZoomHandler.ReturnZoom();
			}

			if (solved && solveTimer < 180)
				solveTimer++;
		}

		public override void PostDrawTiles()
		{
			var one = Assets.Tiles.Vitric.OldCeirosOrnament0.Value;
			var two = Assets.Tiles.Vitric.OldCeirosOrnament3.Value;
			var three = Assets.Tiles.Vitric.OldCeirosOrnament1.Value;
			var four = Assets.Tiles.Vitric.OldCeirosOrnament2.Value;
			var blank = Assets.Bosses.VitricBoss.VitricBossCrystal.Value;

			Vector2 pos = StarlightWorld.VitricBossArena.BottomLeft() * 16 + new Vector2(-762, 1424) - Main.screenPosition;

			Main.spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(one, pos, Color.White * 0.25f);
			pos.X += 24 * 16;
			Main.spriteBatch.Draw(two, pos, Color.White * 0.25f);
			pos.X += 24 * 16 - 4;
			Main.spriteBatch.Draw(three, pos, Color.White * 0.25f);
			pos.X += 24 * 16 + 6;
			Main.spriteBatch.Draw(four, pos, Color.White * 0.25f);

			pos = StarlightWorld.VitricBossArena.BottomLeft() * 16 + new Vector2(-753, 1412) - Main.screenPosition;
			pos.X += 12 * 16;

			Main.spriteBatch.Draw(blank, pos, new Rectangle(0, 0, blank.Width, blank.Height / 4), Color.White * 0.25f);
			pos.X += 24 * 16;
			Main.spriteBatch.Draw(blank, pos, new Rectangle(0, 0, blank.Width, blank.Height / 4), Color.White * 0.25f);
			pos.X += 24 * 16;
			Main.spriteBatch.Draw(blank, pos, new Rectangle(0, 0, blank.Width, blank.Height / 4), Color.White * 0.25f);

			Main.spriteBatch.End();
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