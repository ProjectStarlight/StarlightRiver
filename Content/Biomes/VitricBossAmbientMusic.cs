using StarlightRiver.Content.Waters;
using StarlightRiver.Core.Systems.LightingSystem;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Biomes
{
	public class VitricBossAmbientMusic : ModSceneEffect
	{
		public override int Music => MusicLoader.GetMusicSlot("StarlightRiver/Sounds/Music/VitricBossAmbient");

		public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

		public override bool IsSceneEffectActive(Player player)
		{
			return StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && StarlightWorld.VitricBossArena.Contains((player.Center / 16).ToPoint());
		}
	}
}