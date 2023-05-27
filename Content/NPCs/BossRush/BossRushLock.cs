using StarlightRiver.Core.Systems.ScreenTargetSystem;
using Terraria.Audio;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.NPCs.BossRush
{
	internal class BossRushLock : ModNPC, ILoadable
	{
		public override string Texture => "StarlightRiver/Assets/NPCs/BossRush/BossRushLock";

		public override void SetDefaults()
		{
			NPC.lifeMax = 400;
			NPC.width = 42;
			NPC.height = 42;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.knockBackResist = 0f;

			NPC.HitSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/Clink");
		}

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, new Vector3(1, 1, 0.4f));
		}
	}
}