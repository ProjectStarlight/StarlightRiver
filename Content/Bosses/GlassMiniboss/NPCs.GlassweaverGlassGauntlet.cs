using StarlightRiver.Content.NPCs.Vitric.Gauntlet;
using System;
using System.Linq;
using static Terraria.ModLoader.ModContent;
using Terraria.Localization;

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
			int i = Projectile.NewProjectile(Entity.GetSource_Misc("SLR:GlassGauntlet"), NPC.Center, Vector2.Zero, ProjectileType<GauntletSpawner>(), 0, 0, Main.myPlayer, type, onFloor ? 0 : 0);

			if (Main.projectile[i].ModProjectile is GauntletSpawner)
			{
				var gs = Main.projectile[i].ModProjectile as GauntletSpawner;
				gs.startPos = NPC.Center;
				gs.targetPos = pos;
			}
		}

		private void CheckGauntletWave()
		{
			if (ActiveGauntletCount <= 0)
				waitTime++;

			if (waitTime > 50)
			{
				waitTime = 0;
				ResetAttack();
				AttackPhase++;
			}
		}

		private string GetWaveText(string text) => Language.GetTextValue("Mods.StarlightRiver.Custom.NewText.GlassweaverGauntletWave."+ text);

		private void GauntletWave0()
		{
			if (AttackTimer == 1)
				Main.NewText(GetWaveText("Begin") , Color.OrangeRed);

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
				Main.NewText(GetWaveText("Wave 2"), Color.OrangeRed);

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
				Main.NewText(GetWaveText("Wave 3"), Color.OrangeRed);

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
				Main.NewText(GetWaveText("Wave 4"), Color.OrangeRed);

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
				Main.NewText(GetWaveText("Wave 5"), Color.OrangeRed);

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
				Main.NewText(GetWaveText("Wave 6"), Color.OrangeRed);

			if (AttackTimer == 20) //Ground melee
			{
				SpawnEnemy(arenaPos + new Vector2(200, 0), Shielder, true);
				SpawnEnemy(arenaPos + new Vector2(260, 0), Grunt, true);
			}

			if (AttackTimer == 30) //Ground ranged and air melee
			{
				SpawnEnemy(arenaPos + new Vector2(-200, 0), Pelter, true);
				SpawnEnemy(arenaPos + new Vector2(200, -50), FlyingGrunt, false);
			}

			if (AttackTimer == 40) //Air ranged and support
			{
				SpawnEnemy(arenaPos + new Vector2(-320, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(-380, 0), Supporter, true);
				SpawnEnemy(arenaPos + new Vector2(300, 0), Supporter, true);

				SpawnEnemy(arenaPos + new Vector2(-320, -70), FlyingPelter, false);
			}

			if (AttackTimer == 161 && ActiveGauntletCount > 4)
				AttackTimer = 160;

			if (AttackTimer == 180)
			{
				SpawnEnemy(arenaPos + new Vector2(320, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(380, 0), Grunt, true);

				SpawnEnemy(arenaPos + new Vector2(320, -50), FlyingGrunt, false);

				SpawnEnemy(arenaPos + new Vector2(-260, 0), Pelter, true);
			}

			if (AttackTimer > 280)
				CheckGauntletWave();
		}

		private void GauntletWave6()
		{
			if (AttackTimer == 1)
				Main.NewText(GetWaveText("Wave 7"), Color.OrangeRed);

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

			if (AttackTimer == 161 && ActiveGauntletCount > 4)
				AttackTimer = 160;

			if (AttackTimer == 180)
			{
				SpawnEnemy(arenaPos + new Vector2(-40 * PlayerDirection, 0), Grunt, true);
				SpawnEnemy(arenaPos + new Vector2(-460 * PlayerDirection, 0), Pelter, true);
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