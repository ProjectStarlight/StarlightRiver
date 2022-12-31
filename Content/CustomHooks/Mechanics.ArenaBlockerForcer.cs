using StarlightRiver.Content.Bosses.SquidBoss;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
	class ArenaBlockerForcer : HookGroup
	{
		public override void Load()
		{
			On.Terraria.Player.Update_NPCCollision += ForceColliding;
		}

		// for the most part vanilla
		private void ForceColliding(On.Terraria.Player.orig_Update_NPCCollision orig, Player self)
		{
			var rectangle = new Rectangle((int)self.position.X, (int)self.position.Y, self.width, self.height);

			for (int i = 0; i < 200; i++)
			{
				if (!Main.npc[i].active || Main.npc[i].friendly || Main.npc[i].damage <= 0)
					continue;

				NPC NPC = Main.npc[i];
				int type = NPC.type;
				var NPCRect = new Rectangle((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
				int specialHit = -1;
				float damageMultipler = 1;

				NPC.GetMeleeCollisionData(rectangle, i, ref specialHit, ref damageMultipler, ref NPCRect);

				if (rectangle.Intersects(NPCRect) && type == NPCType<ArenaBlocker>() && NPCLoader.CanHitPlayer(NPC, self, ref specialHit))
				{
					NPCLoader.OnHitPlayer(NPC, self, NPC.damage, false);

					int dam = NPC.damage;
					bool crit = false;
					NPCLoader.ModifyHitPlayer(NPC, self, ref dam, ref crit);

					return;
				}
			}

			orig(self);
		}
	}
}
