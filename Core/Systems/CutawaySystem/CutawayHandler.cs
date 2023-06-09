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

		public static void CreateCutaways()
		{
			// Auroracle temple overlay
			cathedralOverlay = new Cutaway(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/SquidBoss/CathedralOver", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, StarlightWorld.squidBossArena.TopLeft() * 16)
			{
				Inside = CheckForSquidArena
			};
			CutawayHook.NewCutaway(cathedralOverlay);

			// Glassweaver forge overlay
			forgeOverlay = new Cutaway(ModContent.Request<Texture2D>("StarlightRiver/Assets/Overlay/ForgeOverlay", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, StarlightWorld.GlassweaverArena.TopLeft() + new Vector2(-2, 2) * 16)
			{
				Inside = (n) => StarlightWorld.GlassweaverArena.Intersects(n.Hitbox)
			};
			CutawayHook.NewCutaway(forgeOverlay);

			// Vitric temple overlay
			Point16 dimensions = Point16.Zero;
			StructureHelper.Generator.GetDimensions("Structures/VitricTempleNew", StarlightRiver.Instance, ref dimensions);
			Vector2 templePos = new Vector2(StarlightWorld.vitricBiome.Center.X - dimensions.X / 2, StarlightWorld.vitricBiome.Center.Y - 1) * 16;
			templePos.Y -= 9;
			templeOverlay = new Cutaway(ModContent.Request<Texture2D>("StarlightRiver/Assets/Overlay/TempleOverlay", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, templePos)
			{
				Inside = (n) => n.InModBiome<Content.Biomes.VitricTempleBiome>()
			};
			CutawayHook.NewCutaway(templeOverlay);
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

		public override void OnWorldUnload()
		{
			created = false;
		}
	}
}
