using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
	internal abstract class SqaureCollider : MovingPlatform
	{
		public override bool CanFallThrough => false;

		public override void SafeAI()
		{
			
		}
	}
}
