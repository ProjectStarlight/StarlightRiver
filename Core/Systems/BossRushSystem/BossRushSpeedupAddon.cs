using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushSpeedupAddon : ModSystem
	{
		public static bool active;

		public static int speedupTimer;

		public override void Load()
		{
			On_Main.DoUpdate += Speedup;
		}

		/// <summary>
		/// This handles the speedup rules of the boss rush. IE that blitz should be 1.25x gamespeed and showdown 1.5x
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="gameTime"></param>
		private void Speedup(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
		{
			orig(self, ref gameTime);

			if (!BossRushSystem.isBossRush || Main.gameMenu) //dont do anything outside of bossrush but the normal update
				return;

			speedupTimer++; //track this seperately since gameTime would get sped up

			if (active) //1.25x on expert
			{
				if (speedupTimer % 3 == 0)
					orig(self, ref gameTime);

				return;
			}
		}
	}
}