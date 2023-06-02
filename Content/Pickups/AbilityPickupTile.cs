using Terraria.ID;

namespace StarlightRiver.Content.Pickups
{
	public abstract class AbilityPickupTile : ModTile
	{
		public virtual int PickupType => 0;

		private int spawnAttemptTimer = 0;
		public override string Texture => AssetDirectory.Invisible;

		public override void PostSetDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, 0, SoundID.Dig, false, Color.White);
			//minPick = int.MaxValue;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (spawnAttemptTimer > 0)
			{
				spawnAttemptTimer--;
				return;
			}

			spawnAttemptTimer = 60;

			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC NPC = Main.npc[k];

				if (NPC.active && NPC.type == PickupType && Vector2.DistanceSquared(NPC.position, new Vector2(i, j) * 16) <= 128)
					return;
			}

			var abilitySpawnPacket = new Packets.SpawnNPC(Main.myPlayer, i * 16 + 8, j * 16 + 24, PickupType);
			abilitySpawnPacket.Send();
		}
	}
}