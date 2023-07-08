﻿using StarlightRiver.Content.NPCs.Vitric.Gauntlet;
using System;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
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

		private int ActiveGauntletCount => Main.npc.Count(enemy => enemy.active && enemy.ModNPC is VitricConstructNPC && (enemy.ModNPC as VitricConstructNPC).partOfGauntlet);

		private void SpawnEnemy(Vector2 pos, int type, bool onFloor = true)
		{
			if (summonAnimTime > 0 && summonAnimTime < 30)
				summonAnimTime = 30;

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //only server or single player should be creating these spawners

			int i = Projectile.NewProjectile(Entity.GetSource_Misc("SLR:GlassGauntlet"), NPC.Center + new Vector2(32, -128), Vector2.Zero, ProjectileType<GauntletSpawner>(), 0, 0, Owner: -1, type, onFloor ? 0 : 0);

			if (Main.projectile[i].ModProjectile is GauntletSpawner)
			{
				var gs = Main.projectile[i].ModProjectile as GauntletSpawner;
				gs.startPos = NPC.Center + new Vector2(32, -128);
				gs.targetPos = pos;
			}
		}

		private void CheckGauntletWave()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; //multiplayer clients don't need to check this the server will tell them about it

			if (ActiveGauntletCount <= 0)
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
			if (AttackTimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
				Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Begin!"), Color.OrangeRed);

			if (AttackTimer == 120)
				summonAnimTime = 60;

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
				summonAnimTime = 60;

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(-300 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-400 * PlayerDirection, 0), Grunt, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
				SpawnEnemy(arenaPos + new Vector2(-360 * PlayerDirection, -100), FlyingGrunt, false);

			if (AttackTimer == 40) //Air ranged and support
				SpawnEnemy(arenaPos + new Vector2(-440 * PlayerDirection, -150), FlyingPelter, false);

			if (AttackTimer > 160)
				CheckGauntletWave();
		}

		private void GauntletWave2()
		{
			if (AttackTimer == 1)
				summonAnimTime = 60;

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
				summonAnimTime = 60;

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(-120 * PlayerDirection, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(-160 * PlayerDirection, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(-200 * PlayerDirection, 0), Shielder, true);
			}

			if (AttackTimer == 40) //Air ranged and support
			{
				SpawnEnemy(arenaPos + new Vector2(-240 * PlayerDirection, -100), FlyingPelter, false);
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
				summonAnimTime = 60;

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(-120 * PlayerDirection, 0), Juggernaut, true);
				SpawnEnemy(arenaPos + new Vector2(-200 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-280 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-360 * PlayerDirection, 0), Grunt, true);
			}

			if (AttackTimer > 160)
				CheckGauntletWave();
		}

		private void GauntletWave5()
		{
			if (AttackTimer == 1)
				summonAnimTime = 60;

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(200, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(260, 0), Pelter, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{
				SpawnEnemy(arenaPos + new Vector2(-200, 0), Pelter, true);
				SpawnEnemy(arenaPos + new Vector2(200, -50), FlyingPelter, false);
			}

			if (AttackTimer == 40) //Air ranged and support
			{
				SpawnEnemy(arenaPos + new Vector2(-300, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(300, 0), Supporter, true);
			}

			if (AttackTimer == 160)
			{
				if (ActiveGauntletCount > 4)
				{
					AttackTimer = 159;
				}
				else
				{
					summonAnimTime = 60;
				}
			}

			if (AttackTimer == 180)
			{
				SpawnEnemy(arenaPos + new Vector2(320, -50), FlyingGrunt, false);

				SpawnEnemy(arenaPos + new Vector2(-260, 0), Pelter, true);
				SpawnEnemy(arenaPos + new Vector2(-280, 0), Pelter, true);
			}

			if (AttackTimer > 280)
				CheckGauntletWave();
		}

		private void GauntletWave6()
		{
			if (AttackTimer == 1)
				summonAnimTime = 60;

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(-120 * PlayerDirection, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(-220 * PlayerDirection, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(-300 * PlayerDirection, 0), Juggernaut, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{
				SpawnEnemy(arenaPos + new Vector2(-180 * PlayerDirection, -100), FlyingGrunt, false);
				SpawnEnemy(arenaPos + new Vector2(-220 * PlayerDirection, -125), FlyingGrunt, false);
			}

			if (AttackTimer == 40) //Air ranged and support
			{
				SpawnEnemy(arenaPos + new Vector2(-340 * PlayerDirection, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(-370 * PlayerDirection, 0), Supporter, true);
			}

			if (AttackTimer == 160)
			{
				if (ActiveGauntletCount > 4)
				{
					AttackTimer = 159;
				}
				else
				{
					summonAnimTime = 60;
				}
			}

			if (AttackTimer == 180)
			{
				SpawnEnemy(arenaPos + new Vector2(-380 * PlayerDirection, -130), FlyingPelter, false);
				SpawnEnemy(arenaPos + new Vector2(-400 * PlayerDirection, -130), FlyingPelter, false);
			}

			if (AttackTimer > 280)
				CheckGauntletWave();
		}

		private void EndGauntlet()
		{
			Phase = (int)Phases.ReturnToForeground;
			ResetAttack();
		}
	}
}