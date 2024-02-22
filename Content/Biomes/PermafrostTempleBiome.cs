using StarlightRiver.Content.Tiles.Permafrost;

namespace StarlightRiver.Content.Biomes
{
	public class PermafrostTempleBiome : ModBiome
	{
		/// <summary>
		/// Stores the old state of smart cursor so its can be restored when the player leaves the biome
		/// </summary>
		public bool oldSmartCursor = false;

		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/SquidArena");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Auroracle Temple");
		}

		public override bool IsBiomeActive(Player player)
		{
			return Main.tile[(int)player.Center.X / 16, (int)player.Center.Y / 16].WallType == ModContent.WallType<AuroraBrickWall>();
		}

		public override void OnInBiome(Player player)
		{
			if (player == Main.LocalPlayer && Main.SmartCursorWanted)
			{
				Main.SmartCursorWanted_Mouse = false;
				Main.SmartCursorWanted_GamePad = false; // Is this ever even used?
			}
		}

		public override void OnEnter(Player player)
		{
			if (player == Main.LocalPlayer)
				oldSmartCursor = Main.SmartCursorWanted;
		}

		public override void OnLeave(Player player)
		{
			if (player == Main.LocalPlayer)
			{
				Main.SmartCursorWanted_Mouse = oldSmartCursor;
				Main.SmartCursorWanted_GamePad = oldSmartCursor; // Is this ever even used?
			}
		}
	}
}