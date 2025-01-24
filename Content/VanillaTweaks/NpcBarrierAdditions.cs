using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.VanillaTweaks
{
	internal class NpcBarrierAdditions : GlobalNPC
	{
		public void AddBarrier(NPC entity, int type, int normal, int expert, int master)
		{
			if (entity.type == type)
			{
				BarrierNPC barrierGlobal = entity.GetGlobalNPC<BarrierNPC>();
				barrierGlobal.maxBarrier = Main.masterMode ? master : Main.expertMode ? expert : normal;
				barrierGlobal.barrier = barrierGlobal.maxBarrier;

				// Automatic adjustments to lower other defenses to compensate
				entity.defense /= 2;
				entity.lifeMax = (int)(entity.lifeMax * 0.8f);
			}
		}

		public override void SetDefaults(NPC entity)
		{
			AddBarrier(entity, NPCID.DarkCaster, 30, 60, 90);
			AddBarrier(entity, NPCID.Demon, 50, 100, 150);
			AddBarrier(entity, NPCID.VoodooDemon, 50, 100, 150);
			AddBarrier(entity, NPCID.Tim, 50, 100, 150);
			AddBarrier(entity, NPCID.GraniteFlyer, 40, 80, 120);
			AddBarrier(entity, NPCID.GoblinSorcerer, 20, 40, 60);

			AddBarrier(entity, NPCID.CrimsonAxe, 100, 200, 300);
			AddBarrier(entity, NPCID.CursedHammer, 100, 200, 300);
			AddBarrier(entity, NPCID.EnchantedSword, 100, 200, 300);
			AddBarrier(entity, NPCID.IceElemental, 100, 200, 300);
			AddBarrier(entity, NPCID.Necromancer, 200, 400, 600);
			AddBarrier(entity, NPCID.DiabolistRed, 150, 300, 450);
			AddBarrier(entity, NPCID.DiabolistWhite, 150, 300, 450);
			AddBarrier(entity, NPCID.RedDevil, 200, 400, 600);
			AddBarrier(entity, NPCID.RuneWizard, 300, 600, 900);
			AddBarrier(entity, NPCID.Pixie, 50, 100, 150);
			AddBarrier(entity, NPCID.GoblinSummoner, 200, 400, 600);
			AddBarrier(entity, NPCID.Reaper, 150, 300, 450);
			AddBarrier(entity, NPCID.NebulaBeast, 200, 400, 600);
			AddBarrier(entity, NPCID.NebulaBrain, 200, 400, 600);
			AddBarrier(entity, NPCID.NebulaHeadcrab, 100, 200, 300);
			AddBarrier(entity, NPCID.NebulaSoldier, 200, 400, 600);
			AddBarrier(entity, NPCID.DD2DarkMageT1, 100, 200, 300);
			AddBarrier(entity, NPCID.DD2DarkMageT3, 200, 400, 600);
			AddBarrier(entity, NPCID.LunarTowerNebula, 2000, 4000, 6000);
			AddBarrier(entity, NPCID.CultistBoss, 500, 1000, 1500);
		}
	}
}