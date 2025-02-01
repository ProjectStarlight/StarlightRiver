using StarlightRiver.Core.Systems.CutsceneSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class ThinkerOpenCutscene : Cutscene
	{
		public override void InCutscene(Player player)
		{
			// Nothing here, cutscene is managed entirely by the CutsceneFakeThinker NPC. Perhaps we should
			// move control of that here? I dont know.
		}
	}
}