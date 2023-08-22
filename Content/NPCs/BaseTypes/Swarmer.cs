using System;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
	internal abstract class Swarmer : ModNPC
	{
		/// <summary>
		/// The distance for which the swarm attraction/spread should aim for between each NPC
		/// </summary>
		public virtual float IdealDistance => 32;

		/// <summary>
		/// How much velocity swarmers should push each other away with
		/// </summary>
		public virtual float PushPower => 2f;

		/// <summary>
		/// How much velocity swarmers should attract other nearby swarmers with
		/// </summary>
		public virtual float PullPower => 0.05f;

		/// <summary>
		/// Causes this NPC to shove others and itself away to get within an ideal distance, prevents clumping
		/// Should be called by the AI when ideal to do so
		/// </summary>
		public virtual void ShoveSwarm()
		{
			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC other = Main.npc[k];

				if (other is null || !other.active || other.type != NPC.type || other == NPC)
					continue;

				bool tooClose = Vector2.DistanceSquared(other.Center, NPC.Center) <= Math.Pow(IdealDistance, 2);

				// Push away if too close
				if (tooClose)
				{
					other.velocity += other.DirectionFrom(NPC.Center) * PushPower;
					NPC.velocity += NPC.DirectionFrom(other.Center) * PushPower;
				}

				bool tooFar = Vector2.DistanceSquared(other.Center, NPC.Center) >= Math.Pow(IdealDistance * 2, 2);
				tooFar &= Vector2.DistanceSquared(other.Center, NPC.Center) <= Math.Pow(IdealDistance * 4, 2);

				// Attract if too far
				if (tooFar)
				{
					other.velocity += other.DirectionTo(NPC.Center) * PullPower;
					NPC.velocity += NPC.DirectionTo(other.Center) * PullPower;
				}
			}
		}

		/// <summary>
		/// We override this in order to make a large quantity of our swarmers spawn at once easily
		/// </summary>
		/// <param name="tileX"></param>
		/// <param name="tileY"></param>
		/// <returns></returns>
		public override int SpawnNPC(int tileX, int tileY)
		{
			// Pick an appropriate amount to spawn
			int amount = Main.masterMode ? 10 : Main.expertMode ? 8 : 6;
			amount += Main.rand.Next(3);

			// Spawn the extras
			for (int k = 0; k < amount; k++)
			{
				NPC.NewNPC(NPC.GetSource_FromThis(), tileX * 16 + k, tileY * 16, NPC.type);
			}

			// Spawn the original
			return NPC.NewNPC(NPC.GetSource_FromThis(), tileX * 16, tileY * 16, NPC.type);
		}
	}
}