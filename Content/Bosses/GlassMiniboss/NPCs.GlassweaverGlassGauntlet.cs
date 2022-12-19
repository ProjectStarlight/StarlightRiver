using StarlightRiver.Content.NPCs.Vitric.Gauntlet;
using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class Glassweaver : ModNPC
	{
		private int waitTime;

		private int Grunt => NPCType<GruntConstruct>();
		private int Pelter => NPCType<PelterConstruct>();
		private int FlyingGrunt => NPCType<FlyingGruntConstruct>();
		private int FlyingPelter => NPCType<FlyingPelterConstruct>();
		private int Shielder => NPCType<ShieldConstruct>();
		private int Supporter => NPCType<SupporterConstruct>();
		private int Juggernaut => NPCType<JuggernautConstruct>();

		private int PlayerDirection => Math.Sign(Target.Center.X - NPC.Center.X);

		private void SpawnEnemy(Vector2 pos, int type, bool onFloor = true)
		{
			Projectile.NewProjectile(Entity.GetSource_Misc("SLR:GlassGauntlet"), pos, Vector2.Zero, ProjectileType<GauntletSpawner>(), 0, 0, Main.myPlayer, type, onFloor ? 0 : 2);
		}

		private void CheckGauntletWave()
		{
			bool allEnemiesDowned = true;

			foreach (NPC enemy in Main.npc)
			{
				if (enemy.active && enemy.ModNPC is VitricConstructNPC)
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

		private void GauntletWave0()
		{
			if (AttackTimer == 1)
				Main.NewText("Begin", Color.OrangeRed);

			if (AttackTimer == 150) //Ground melee
				SpawnEnemy(arenaPos + new Vector2(-300 * PlayerDirection, 0), Grunt, true);

			if (AttackTimer == 160) //Ground ranged and air melee
			{
				SpawnEnemy(arenaPos + new Vector2(-400 * PlayerDirection, 0), Pelter, true);
				SpawnEnemy(arenaPos + new Vector2(-450 * PlayerDirection, 0), Pelter, true);
			}

			if (AttackTimer > 300)
				CheckGauntletWave();
		}

		private void GauntletWave1()
		{
			if (AttackTimer == 1)
				Main.NewText("Wave 2", Color.OrangeRed);

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(-300 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-400 * PlayerDirection, 0), Grunt, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{
				SpawnEnemy(arenaPos + new Vector2(-450 * PlayerDirection, 0), Pelter, true);
				SpawnEnemy(arenaPos + new Vector2(-360 * PlayerDirection, -100), FlyingGrunt, false);
			}

			if (AttackTimer == 40) //Air ranged and support
				SpawnEnemy(arenaPos + new Vector2(-440 * PlayerDirection, -150), FlyingPelter, false);

			if (AttackTimer > 160)
				CheckGauntletWave();
		}

		private void GauntletWave2()
		{
			if (AttackTimer == 1)
				Main.NewText("Wave 3", Color.OrangeRed);

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(-300, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-400, 0), Grunt, true);

				SpawnEnemy(arenaPos + new Vector2(200, 0), Shielder, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{
				SpawnEnemy(arenaPos + new Vector2(400, 0), Pelter, true);
				SpawnEnemy(arenaPos + new Vector2(460, 0), Pelter, true);
			}

			if (AttackTimer == 40) //Air ranged and support
			{
				SpawnEnemy(arenaPos + new Vector2(300, 0), Supporter, true);
			}

			if (AttackTimer > 160)
				CheckGauntletWave();
		}
		private void GauntletWave3()
		{
			if (AttackTimer == 1)
				Main.NewText("Wave 4", Color.OrangeRed);

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(-120 * PlayerDirection, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(-160 * PlayerDirection, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(-200 * PlayerDirection, 0), Shielder, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{

			}

			if (AttackTimer == 40) //Air ranged and support
			{
				SpawnEnemy(arenaPos + new Vector2(-240 * PlayerDirection, -100), FlyingPelter, false);
				SpawnEnemy(arenaPos + new Vector2(-280 * PlayerDirection, -120), FlyingPelter, false);
				SpawnEnemy(arenaPos + new Vector2(-320 * PlayerDirection, -100), FlyingPelter, false);

				SpawnEnemy(arenaPos + new Vector2(-240 * PlayerDirection, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(-280 * PlayerDirection, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(-320 * PlayerDirection, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(-360 * PlayerDirection, 0), Supporter, true);
			}

			if (AttackTimer > 160)
				CheckGauntletWave();
		}
		private void GauntletWave4()
		{
			if (AttackTimer == 1)
				Main.NewText("Wave 5", Color.OrangeRed);

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(-120 * PlayerDirection, 0), Juggernaut, true);
				SpawnEnemy(arenaPos + new Vector2(-200 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-280 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-360 * PlayerDirection, 0), Grunt, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{

			}

			if (AttackTimer == 40) //Air ranged and support
			{

			}

			if (AttackTimer > 160)
				CheckGauntletWave();
		}
		private void GauntletWave5()
		{
			if (AttackTimer == 1)
				Main.NewText("Wave 6", Color.OrangeRed);

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(200, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(260, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(320, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(380, 0), Grunt, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{
				SpawnEnemy(arenaPos + new Vector2(-200, 0), Pelter, true);
				SpawnEnemy(arenaPos + new Vector2(-260, 0), Pelter, true);

				SpawnEnemy(arenaPos + new Vector2(200, -50), FlyingGrunt, false);
				//SpawnEnemy(arenaPos + new Vector2(260, -80), FlyingGrunt, false);
				SpawnEnemy(arenaPos + new Vector2(320, -50), FlyingGrunt, false);
			}

			if (AttackTimer == 40) //Air ranged and support
			{
				SpawnEnemy(arenaPos + new Vector2(-320, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(-380, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(300, 0), Supporter, true);

				SpawnEnemy(arenaPos + new Vector2(-320, -70), FlyingPelter, false);
				// SpawnEnemy(arenaPos + new Vector2(-380, -90), FlyingPelter, false);
			}

			if (AttackTimer > 160)
				CheckGauntletWave();
		}
		private void GauntletWave6()
		{
			if (AttackTimer == 1)
				Main.NewText("Wave 7", Color.OrangeRed);

			if (AttackTimer == 20) //Ground melee
			{
				//SpawnEnemy(arenaPos + new Vector2(40 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-40 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-120 * PlayerDirection, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(-220 * PlayerDirection, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(-300 * PlayerDirection, 0), Juggernaut, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{
				//SpawnEnemy(arenaPos + new Vector2(-400 * PlayerDirection, 0), Pelter, true);
				SpawnEnemy(arenaPos + new Vector2(-460 * PlayerDirection, 0), Pelter, true);

				SpawnEnemy(arenaPos + new Vector2(-180 * PlayerDirection, -100), FlyingGrunt, false);
				SpawnEnemy(arenaPos + new Vector2(-220 * PlayerDirection, -125), FlyingGrunt, false);
			}

			if (AttackTimer == 40) //Air ranged and support
			{
				SpawnEnemy(arenaPos + new Vector2(-400 * PlayerDirection, -130), FlyingPelter, false);
				//SpawnEnemy(arenaPos + new Vector2(-460 * PlayerDirection, -150), FlyingPelter, false);

				SpawnEnemy(arenaPos + new Vector2(-340 * PlayerDirection, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(-370 * PlayerDirection, 0), Supporter, true);
			}

			if (AttackTimer > 160)
				CheckGauntletWave();
		}

		private void EndGauntlet()
		{
			Phase = (int)Phases.ReturnToForeground;
			ResetAttack();
		}
	}
}