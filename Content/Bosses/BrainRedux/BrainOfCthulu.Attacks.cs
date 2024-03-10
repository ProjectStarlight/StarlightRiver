using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal partial class BrainOfCthulu
	{
		#region Phase 1
		public void ShrinkingCircle()
		{
			if (AttackTimer == 1)
			{
				neurisms.ForEach(n =>
				{
					n.velocity *= 0;
					(n.ModNPC as Neurysm).State = 1;
					(n.ModNPC as Neurysm).Timer = 0;
				});
			}

			if (AttackTimer == 31)
			{
				for(int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * 750;
					(neurisms[k].ModNPC as Neurysm).State = 2;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer >= 61)
			{
				float timer = Helpers.Helper.BezierEase((AttackTimer - 60) / 180f);

				for (int k = 0; k < neurisms.Count; k++)
				{
					float rot = k / (float)neurisms.Count * 6.28f + timer * 3.14f;

					neurisms[k].Center = thinker.Center + Vector2.UnitX.RotatedBy(rot) * (750 - timer * 675f);
					(neurisms[k].ModNPC as Neurysm).State = 0;
					(neurisms[k].ModNPC as Neurysm).Timer = 0;
				}
			}

			if (AttackTimer == 240)
			{
				AttackTimer = 0;
				AttackState = 0;
			}
		}
		#endregion
	}
}
