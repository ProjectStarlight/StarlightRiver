using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class Glassweaver : ModNPC
    {
        private static List<int> enemyTypes = new List<int>
        {
            //ModContent.NPCType<Glassweaver>(),
        };

        private void SpawnEnemy(Vector2 pos, int type, bool onFloor = true)
        {
            if (onFloor)
            {
                int x = (int)(pos.X / 16f);
                int y = (int)(pos.Y / 16f);
                for (int i = y - 2; i < y + 20; i++)
                {
                    if (WorldGen.ActiveAndWalkableTile(x, i))
                    {
                        pos = new Vector2(pos.X, i * 16f - 2);
                        break;
                    }
                }
            }
            NPC.NewNPC(Entity.GetSource_Misc("GlassTrial"), (int)pos.X, (int)pos.Y, type);
        }

        private void CheckWave()
        {
            bool allEnemiesDowned = true;

            foreach (NPC enemy in Main.npc)
            {
                if (!enemy.active || enemy.Distance(arenaPos) > 1000f)
                    continue;
                else
                    foreach (int t in enemyTypes)
                        if (enemy.type == t)
                            allEnemiesDowned = false;
            }

            if (allEnemiesDowned)
            {
                ResetAttack();
                AttackPhase++;
            }
        }

        private void GlassTrial_Wave1()
        {
            AttackTimer++;

            if (AttackTimer == 5)
            {
                SpawnEnemy(arenaPos + new Vector2(400, 0), enemyTypes[0]);
                SpawnEnemy(arenaPos + new Vector2(-400, 0), enemyTypes[0]);
            }

            if (AttackTimer > 120)
                CheckWave();
        }

        private void GlassTrial_Wave2()
        {

        }
    }
}
