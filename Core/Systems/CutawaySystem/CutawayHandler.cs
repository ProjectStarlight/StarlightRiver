using StarlightRiver.Content.Tiles.Permafrost;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.CutawaySystem
{
	internal class CutawayHandler : ModSystem
	{
		public static bool created = false;

		public static Cutaway cathedralOverlay;
		public static Cutaway forgeOverlay;

		public static void CreateCutaways()
		{
			//TODO: Create new overlay for this when the structure is done
			/*var templeCutaway = new Cutaway(Request<Texture2D>("StarlightRiver/Assets/Backgrounds/TempleCutaway").Value, new Vector2(VitricBiome.Center.X - 47, VitricBiome.Center.Y + 5) * 16);
            templeCutaway.inside = n => n.InModBiome(ModContent.GetInstance<VitricTempleBiome>());
            CutawayHandler.NewCutaway(templeCutaway);*/

			cathedralOverlay = new Cutaway(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/SquidBoss/CathedralOver", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, StarlightWorld.squidBossArena.TopLeft() * 16)
			{
				Inside = CheckForSquidArena
			};
			CutawayHook.NewCutaway(cathedralOverlay);

			forgeOverlay = new Cutaway(ModContent.Request<Texture2D>("StarlightRiver/Assets/Overlay/ForgeOverlay", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value, StarlightWorld.GlassweaverArena.TopLeft() + new Vector2(-2, 2) * 16)
			{
				Inside = (n) => StarlightWorld.GlassweaverArena.Intersects(n.Hitbox)
			};
			CutawayHook.NewCutaway(forgeOverlay);
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

		public override void PostUpdateWorld()
		{
			if (!created)
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
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
