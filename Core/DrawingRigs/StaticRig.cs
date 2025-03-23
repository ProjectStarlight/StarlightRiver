using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.DrawingRigs
{
	internal class StaticRig
	{
		public List<StaticRigPoint> Points { get; set; }
	}

	public class StaticRigPoint
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Frame { get; set; }

		public Vector2 Pos => new(X, Y);
	}
}