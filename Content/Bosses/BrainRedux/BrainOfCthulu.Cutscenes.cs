using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class BrainOfCthulu
	{
		public void Intro()
		{
			if (Timer == 1)
				savedPos = npc.Center;

			if (Timer < 120)
				npc.Center = Vector2.SmoothStep(savedPos, thinker.Center + new Vector2(0, -250), Timer / 120f);

			foreach (Player player in Main.player.Where(n => n.active && Vector2.Distance(n.Center, thinker.Center) < 1500))
			{
				player.position += (thinker.Center - player.Center) * 0.1f * (Vector2.Distance(player.Center, thinker.Center) / 1500f);
			}

			if (Timer == 120)
			{
				(thinker.ModNPC as TheThinker)?.CreateArena();
			}

			if (Timer >= 120)
			{
				if (arenaFade < 120)
					arenaFade++;
			}

			if (Timer == 240)
			{
				State = 2;
				Timer = 0;
				AttackTimer = 0;
			}
		}
	}
}
