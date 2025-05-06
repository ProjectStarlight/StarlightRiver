using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Tiles.Permafrost;
using Terraria.DataStructures;

namespace StarlightRiver.Core.Systems.CutawaySystem
{
	internal class CutawayHandler : ModSystem
	{
		public static bool created = false;

		public static Cutaway cathedralOverlay;
		public static Cutaway forgeOverlay;
		public static Cutaway templeOverlay;
		public static Cutaway observatoryOverlay;

		public static void CreateCutaways()
		{
			CutawayHook.cutaways.Clear();

			// Dont create in subworlds
			if (CutawayHook.InSubworld)
				return;

			// Auroracle temple overlay
			cathedralOverlay = new Cutaway(Assets.Bosses.SquidBoss.CathedralOver, StarlightWorld.squidBossArena.TopLeft() * 16)
			{
				Inside = CheckForSquidArena
			};
			CutawayHook.NewCutaway(cathedralOverlay);

			// Glassweaver forge overlay
			forgeOverlay = new Cutaway(Assets.Overlay.ForgeOverlay, StarlightWorld.GlassweaverArena.TopLeft() + new Vector2(-2, 2) * 16)
			{
				Inside = (n) =>
				{
					Rectangle arena = StarlightWorld.GlassweaverArena;
					arena.Y += 4 * 16;
					arena.Height -= 4 * 16;
					return arena.Intersects(n.Hitbox);
				}
			};
			CutawayHook.NewCutaway(forgeOverlay);

			// Vitric temple overlay
			Point16 dimensions = StructureHelper.API.Generator.GetStructureDimensions("Structures/VitricTempleNew", StarlightRiver.Instance);
			Vector2 templePos = new Vector2(StarlightWorld.vitricBiome.Center.X - dimensions.X / 2, StarlightWorld.vitricBiome.Center.Y - 1) * 16;
			templePos.Y -= 9;
			templeOverlay = new Cutaway(Assets.Overlay.TempleOverlay, templePos)
			{
				Inside = (n) => n.InModBiome<VitricTempleBiome>()
			};
			CutawayHook.NewCutaway(templeOverlay);

			// Observatory overlay
			observatoryOverlay = new Cutaway(Assets.Overlay.ObservatoryOverlay, ObservatorySystem.ObservatoryRoomWorld.TopLeft() + new Vector2(-9, 8) * 16)
			{
				Inside = (n) => ObservatorySystem.IsInMainStructure(n)
			};
			CutawayHook.NewCutaway(observatoryOverlay);
		}

		/// <summary>
		/// Condition for the auroracle overlay to be seen
		/// </summary>
		/// <param name="Player"></param>
		/// <returns>If the auroracle arena overlay should be active</returns>
		private static bool CheckForSquidArena(Player Player)
		{
			if (WorldGen.InWorld((int)Main.LocalPlayer.Center.X / 16, (int)Main.LocalPlayer.Center.Y / 16))
			{
				Tile tile = Framing.GetTileSafely((int)Main.LocalPlayer.Center.X / 16, (int)Main.LocalPlayer.Center.Y / 16);

				if (tile != null)
				{
					return
						tile.WallType == ModContent.WallType<AuroraBrickWall>() &&
						!Main.LocalPlayer.GetModPlayer<StarlightPlayer>().trueInvisible;
				}
			}

			return false;
		}

		public override void PostUpdateEverything()
		{
			if (!Main.dedServ && !created)
			{
				CreateCutaways();
				created = true;
			}
		}

		public override void OnWorldLoad()
		{
			created = false;
		}

		public override void OnWorldUnload()
		{
			created = false;
		}
	}
}