using Terraria.Audio;

namespace StarlightRiver.Content.NPCs.BossRush
{
	internal class BossRushLock : ModNPC
	{
		public override string Texture => "StarlightRiver/Assets/NPCs/BossRush/BossRushLock";

		public override void SetDefaults()
		{
			NPC.lifeMax = 1000;
			NPC.width = 42;
			NPC.height = 42;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.knockBackResist = 1;

			NPC.HitSound = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Impacts/Clink");
		}
	}
}
