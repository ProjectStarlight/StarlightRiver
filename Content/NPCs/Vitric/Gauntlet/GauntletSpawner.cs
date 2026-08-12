using System;
using System.IO;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal class GauntletSpawner : ConstructSpawner
	{
		public Vector2 targetPos;
		public Vector2 startPos;

		public int moveTimer;
		private float rand;

		public override bool PreAI()
		{
			moveTimer++;

			gravity = false;
			Projectile.tileCollide = false;

			if (moveTimer == 1)
			{
				rand = Main.rand.NextFloat(6.28f);
				Projectile.netUpdate = true;
			}

			if (moveTimer < 60)
			{
				float offset = (1 - moveTimer / 60f) * ((float)Math.Cos(moveTimer / 8f * 3.14f + rand) * 30 + (float)Math.Sin(moveTimer / 14f * 3.14f + rand) * 20);
				Projectile.Center = Vector2.SmoothStep(startPos, targetPos, moveTimer / 60f) + new Vector2(0, 32 - (float)Math.Sin(moveTimer / 60f * 3.14f) * (180 + offset));
			}
			else if (moveTimer == 60)
			{
				Timer = 2;
			}

			return true;
		}

		public override void SpawnNPC()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int i = NPC.NewNPC(Entity.GetSource_Misc("SLR:GlassGauntlet"), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)NPCType);

				var mnpc = Main.npc[i].ModNPC as VitricConstructNPC;
				mnpc.partOfGauntlet = true;
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WriteVector2(startPos); //this may need to get changed into a safeSendExtraAI if the base class needs to send extra ai
			writer.WriteVector2(targetPos);
			writer.Write(rand); //these could be put into ai[] fields but the base class should hold dominion over those
			writer.Write(moveTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			startPos = reader.ReadVector2();
			targetPos = reader.ReadVector2();
			rand = reader.ReadSingle();
			moveTimer = reader.ReadInt32();
		}
	}
}