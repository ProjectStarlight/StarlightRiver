using Microsoft.Xna.Framework;
using StarlightRiver.Content.NPCs.Vitric.Gauntlet;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    public partial class Glassweaver : ModNPC
    {
        private int Grunt => NPCType<GruntConstruct>();
        private int Pelter => NPCType<PelterConstruct>();
        private int FlyingGrunt => NPCType<FlyingGruntConstruct>();
        private int FlyingPelter => NPCType<FlyingPelterConstruct>();

        private void SpawnEnemy(Vector2 pos, int type, bool onFloor = true)
        {
            if (onFloor)
            {
                int x = (int)(pos.X / 16f);
                int y = (int)(pos.Y / 16f);
                for (int i = 2; i < 20; i++)
                {
                    if (WorldGen.ActiveAndWalkableTile(x, y + i))
                    {
                        pos = new Vector2(pos.X, (y + i) * 16f - 2);
                        break;
                    }
                }
            }
            NPC.NewNPC(Entity.GetSource_Misc("GlassTrial"), (int)pos.X, (int)pos.Y, type);
        }

        private int waitTime;

        private void CheckGauntletWave()
        {
            bool allEnemiesDowned = true;

            foreach (NPC enemy in Main.npc)
            {
                if (enemy.active && enemy.ModNPC is IGauntletNPC)
                {
                    allEnemiesDowned = false;
                    continue;
                }
            }

            if (allEnemiesDowned)
                waitTime++;

            if (waitTime > 50)
            {
                waitTime = 0;
                ResetAttack();
                AttackPhase++;
            }
        }

        private void GlassGauntlet_Wave0()
        {
            if (AttackTimer == 1)
                Main.NewText("Begin", Color.OrangeRed);

            if (AttackTimer == 15)
                SpawnEnemy(arenaPos + new Vector2(0, -20), Grunt);


            if (AttackTimer > 40)
                CheckGauntletWave();
        }
        
        private void GlassGauntlet_Wave1()
        {
            if (AttackTimer == 1)
                Main.NewText("Wave 1", Color.OrangeRed);

            if (AttackTimer == 5)
            {
                SpawnEnemy(arenaPos + new Vector2(200, -20), Grunt);
                SpawnEnemy(arenaPos + new Vector2(-200, -20), Grunt);
            }

            if (AttackTimer == 40)
            {
                SpawnEnemy(arenaPos + new Vector2(400, -20), Pelter);
                SpawnEnemy(arenaPos + new Vector2(-400, -20), Pelter);
            }

            if (AttackTimer > 240)
                CheckGauntletWave();
        }

        private void GlassGauntlet_Wave2()
        {
            if (AttackTimer == 1)
                Main.NewText("Wave 2", Color.OrangeRed);

            if (AttackTimer == 5)
            {
                SpawnEnemy(arenaPos + new Vector2(250, -80), FlyingGrunt, true);
                SpawnEnemy(arenaPos + new Vector2(-250, -80), FlyingGrunt, true);
            }

            if (AttackTimer == 20)
            {
                SpawnEnemy(arenaPos + new Vector2(420, -120), Pelter, true);
                SpawnEnemy(arenaPos + new Vector2(-420, -120), Pelter, true);
            }

            if (AttackTimer > 100)
                CheckGauntletWave();
        }

        private void GlassGauntlet_End()
        {
            Phase = (int)PhaseEnum.ReturnToForeground;
            ResetAttack(); 
        }
    }
}
